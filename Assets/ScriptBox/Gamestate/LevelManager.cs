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

    private bool isTransitioning = false;
    private BorderWave localWaveSettings;

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
        
        if (transitionAnimator != null)
        {
            transitionAnimator.gameObject.SetActive(false);
        }

        if (cameraPanScript != null)
        {
            cameraPanScript.SetCameraLock(false);
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(currentLevel.levelDialogue);
        }
        
        isTransitioning = false;
    }

    public void CompleteCurrentLevel()
    {
        if (isTransitioning) return;
        StartCoroutine(LevelSequence());
    }

    private IEnumerator LevelSequence()
    {
        isTransitioning = true;
        LevelData currentLevel = levels[currentLevelIndex];

        Transform originTarget = currentLevel.waveOriginObject != null ? currentLevel.waveOriginObject : currentLevel.levelEndObject;

        if (originTarget == null)
        {
            Debug.LogError("[LevelManager] Missing both waveOriginObject and levelEndObject! Nowhere to anchor the circle effect.");
            isTransitioning = false;
            yield break;
        }

        Vector3 targetPosition = originTarget.position;

        GameObject waveObject = new GameObject("DynamicBorderWave");
        waveObject.transform.position = targetPosition;
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
            Vector3 camTargetPosition = new Vector3(targetPosition.x, targetPosition.y, -10f);
            cameraPanScript.SetPanTarget(camTargetPosition);
            
            while (Vector2.Distance(cameraPanScript.transform.position, camTargetPosition) > 0.05f)
            {
                yield return null; 
            }
        }
        yield return new WaitForSeconds(1f);

        if (transitionAnimator != null)
        {
            transitionAnimator.gameObject.SetActive(true);
            transitionAnimator.Play(transitionAnimationName);
            
            yield return null;

            AnimatorStateInfo stateInfo = transitionAnimator.GetCurrentAnimatorStateInfo(0);
            float halfDuration = stateInfo.length > 0 ? stateInfo.length / 2f : 0.5f;

            yield return new WaitForSeconds(halfDuration);
        }
        else
        {
            yield return new WaitForSeconds(0.25f);
        }

        if (cameraPanScript != null)
        {
            cameraPanScript.SetPanTarget(new Vector3(0f, 0f, -10f));
        }

        if (transitionAnimator == null)
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
    }

    private void ClearCurrentPlanets()
    {
        List<GravitySource> sourcesToDestroy = new List<GravitySource>(GravitySource.ActiveSources);
        foreach (GravitySource source in sourcesToDestroy)
        {
            if (source != null) Destroy(source.gameObject);
        }
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