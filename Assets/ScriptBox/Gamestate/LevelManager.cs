using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [System.Serializable]
    public struct LevelData
    {
        public string levelName;
        [Header("Planet Resource Budgets")]
        public int circlePlanetLimit;
        public int trianglePlanetLimit;
        public int squarePlanetLimit;
        
        [Header("Level Scene Setup Object")]
        public GameObject levelLayoutObject; 

        [Header("Preview Sequence Configuration")]
        public Transform previewStartPoint;
        public Transform previewEndPoint;
        public float startPointWaitTime;

        [Header("Sequence Points")]
        public Transform levelEndObject;

        [Header("VFX Configuration")]
        public Transform waveOriginObject; 

        [Header("Dialogue Configuration")]
        [TextArea(3, 5)] public List<string> levelDialogue;
    }

    [Header("Level Configuration")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();
    [SerializeField] private int currentLevelIndex = 0;

    [Header("Scene References")]
    [SerializeField] private Planner plannerScript;
    [SerializeField] private GameObject victoryScreenUI;
    [SerializeField] private CameraPan cameraPanScript;

    [Header("VFX Settings")]
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private string transitionAnimationName = "Transition_Play";

    [Header("Camera Control Settings")]
    [SerializeField] private float cameraPanSpeed = 3f;

    private bool isTransitioning = false;
    private BorderWave localWaveSettings;
    private Coroutine currentPreviewCoroutine;

    public System.Action<int> OnLevelLoaded;

    public int CurrentLevelIndex => currentLevelIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        localWaveSettings = GetComponent<BorderWave>();
    }

    private void Start()
    {
        if (plannerScript == null) plannerScript = FindFirstObjectByType<Planner>();
        if (cameraPanScript == null) cameraPanScript = FindFirstObjectByType<CameraPan>();

        DeactivateAllLayouts();
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        if (levels == null || levels.Count == 0) return;

        if (index >= levels.Count)
        {
            CompleteAllLevels();
            return;
        }

        if (currentPreviewCoroutine != null) StopCoroutine(currentPreviewCoroutine);

        if (currentLevelIndex < levels.Count && levels[currentLevelIndex].levelLayoutObject != null)
        {
            levels[currentLevelIndex].levelLayoutObject.SetActive(false);
        }

        currentLevelIndex = index;
        LevelData currentLevel = levels[currentLevelIndex];

        if (plannerScript != null)
        {
            plannerScript.SetCircleLimit(currentLevel.circlePlanetLimit);
            plannerScript.SetTriangleLimit(currentLevel.trianglePlanetLimit);
            plannerScript.SetSquareLimit(currentLevel.squarePlanetLimit);
        }

        ClearCurrentPlanets();

        if (currentLevel.levelLayoutObject != null)
        {
            currentLevel.levelLayoutObject.SetActive(true);
        }

        if (GameState.Instance != null)
        {
            GameState.Instance.SetPhase(GameState.Phase.Planning);
        }
        
        if (victoryScreenUI != null) victoryScreenUI.SetActive(false);

        if (cameraPanScript != null)
        {
            currentPreviewCoroutine = StartCoroutine(LevelPreviewSequence(currentLevel));
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(currentLevel.levelDialogue);
        }
        
        if (OnLevelLoaded != null)
        {
            OnLevelLoaded.Invoke(currentLevelIndex);
        }

        isTransitioning = false;
    }

    private IEnumerator LevelPreviewSequence(LevelData level)
    {
        cameraPanScript.SetCameraLock(true);

        Vector3 startPos = level.previewStartPoint != null ? level.previewStartPoint.position : new Vector3(0, 0, -10f);
        Vector3 endPos = level.previewEndPoint != null ? level.previewEndPoint.position : new Vector3(0, 0, -10f);
        startPos.z = -10f;
        endPos.z = -10f;

        cameraPanScript.transform.position = startPos;
        cameraPanScript.SetPanTarget(startPos);

        float waitTime = level.startPointWaitTime > 0 ? level.startPointWaitTime : 3f;
        yield return new WaitForSeconds(waitTime);

        while (Vector2.Distance(cameraPanScript.transform.position, endPos) > 0.05f)
        {
            Vector3 nextPosition = Vector3.MoveTowards(cameraPanScript.transform.position, endPos, cameraPanSpeed * Time.deltaTime);
            nextPosition.z = -10f;
            
            cameraPanScript.SetPanTarget(nextPosition);
            yield return null; 
        }

        cameraPanScript.SetPanTarget(endPos);

        cameraPanScript.SetCameraLock(currentLevelIndex == 0); 
    }

    public void CompleteCurrentLevel()
    {
        if (isTransitioning) return;
        StartCoroutine(LevelSequence(false));
    }

    public void SkipCurrentLevel()
    {
        if (isTransitioning) return;
        StartCoroutine(LevelSequence(true));
    }

    private IEnumerator LevelSequence(bool isSkipping)
    {
        isTransitioning = true;
        LevelData currentLevel = levels[currentLevelIndex];

        if (!isSkipping)
        {
            Transform waveOriginTarget = currentLevel.waveOriginObject != null ? currentLevel.waveOriginObject : currentLevel.levelEndObject;
            Transform cameraFocusTarget = currentLevel.levelEndObject != null ? currentLevel.levelEndObject : currentLevel.waveOriginObject;

            if (waveOriginTarget == null && cameraFocusTarget == null)
            {
                Debug.LogError("[LevelManager] Missing both waveOriginObject and levelEndObject! Nowhere to anchor sequence.");
                isTransitioning = false;
                yield break;
            }

            Vector3 wavePosition = waveOriginTarget != null ? waveOriginTarget.position : cameraFocusTarget.position;
            Vector3 camTargetPosition = cameraFocusTarget != null ? cameraFocusTarget.position : waveOriginTarget.position;
            camTargetPosition.z = -10f; 

            GameObject waveObject = new GameObject("DynamicBorderWave");
            waveObject.transform.position = wavePosition;
            BorderWave activeWaveScript = waveObject.AddComponent<BorderWave>();

            if (localWaveSettings != null)
            {
                activeWaveScript.InitializeCustom(
                    localWaveSettings.expansionSpeed,
                    localWaveSettings.maxRadius,
                    localWaveSettings.thickness,
                    localWaveSettings.segments,
                    localWaveSettings.waveColor,
                    localWaveSettings.uniqueHitTargets,
                    localWaveSettings.defaultAnimatorTrigger,
                    localWaveSettings.fallbackDarkenColor,
                    localWaveSettings.ignoreTag
                );
            }
            else
            {
                activeWaveScript.InitializeCustom(8f, 12f, 0.25f, 60, Color.white, new List<BorderWave.UniqueHitTarget>(), "Explode", Color.gray, "IgnoreWave");
            }

            if (cameraPanScript != null)
            {
                cameraPanScript.SetCameraLock(true);
                
                while (Vector2.Distance(cameraPanScript.transform.position, camTargetPosition) > 0.01f)
                {
                    Vector3 nextPosition = Vector3.MoveTowards(cameraPanScript.transform.position, camTargetPosition, cameraPanSpeed * Time.deltaTime);
                    nextPosition.z = -10f;
                    
                    cameraPanScript.SetPanTarget(nextPosition);
                    yield return null; 
                }
            }
            yield return new WaitForSeconds(1.2f); 
        }

        float totalDuration = 0.5f;
        float halfDuration = 0.25f;

        if (transitionAnimator != null)
        {
            transitionAnimator.gameObject.SetActive(true);
            transitionAnimator.Play(transitionAnimationName);
            
            yield return null;

            AnimatorStateInfo stateInfo = transitionAnimator.GetCurrentAnimatorStateInfo(0);
            totalDuration = stateInfo.length > 0 ? stateInfo.length : 1f;
            halfDuration = totalDuration / 2f;

            yield return new WaitForSeconds(halfDuration);
        }
        else
        {
            yield return new WaitForSeconds(0.25f);
        }

        int nextLevel = currentLevelIndex + 1;
        if (nextLevel < levels.Count)
        {
            LoadLevel(nextLevel);
        }
        else
        {
            CompleteAllLevels();
        }

        if (transitionAnimator != null)
        {
            yield return new WaitForSeconds(totalDuration - halfDuration);
            transitionAnimator.gameObject.SetActive(false);
        }
    }

    private void ClearCurrentPlanets()
    {
        List<GravitySource> sourcesToDestroy = new List<GravitySource>(GravitySource.ActiveSources);
        foreach (GravitySource source in sourcesToDestroy)
        {
            if (source != null && !source.IsStatic) 
                Destroy(source.gameObject);
        }
    }

    public void RestartLevel()
    {
        StopAllCoroutines();

        ClearCurrentPlanets();

        if (transitionAnimator != null)
        {
            transitionAnimator.gameObject.SetActive(false);
        }

        isTransitioning = false;
        LoadLevel(currentLevelIndex);
    }

    private void DeactivateAllLayouts()
    {
        foreach (LevelData level in levels)
        {
            if (level.levelLayoutObject != null) level.levelLayoutObject.SetActive(false);
        }
    }

    private void CompleteAllLevels()
    {
        if (currentLevelIndex < levels.Count && levels[currentLevelIndex].levelLayoutObject != null)
        {
            levels[currentLevelIndex].levelLayoutObject.SetActive(false);
        }
        
        if (transitionAnimator != null)
        {
            transitionAnimator.gameObject.SetActive(false);
        }

        if (victoryScreenUI != null) victoryScreenUI.SetActive(true);
        if (GameState.Instance != null) GameState.Instance.SetPhase(GameState.Phase.Planning);
    }
}