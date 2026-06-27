using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnablePlanet
    {
        public GameObject planetPrefab;
        public float spawnYPosition; 
        public Vector3 spawnScale;
    }

    [Header("Camera Scroll Settings")]
    [SerializeField] private float cameraScrollSpeed = 3f;

    [Header("Planet Floating Settings")]
    [SerializeField] private float spawnXOffsetFromCamera = 12.5f; 
    [SerializeField] private float planetFloatSpeed = 1.5f; 
    [SerializeField] private float delayBetweenPlanets = 0.5f; 
    
    [Tooltip("The script will randomly pick from this list continuously until stopped.")]
    [SerializeField] private List<SpawnablePlanet> planetsPool = new List<SpawnablePlanet>();

    [Header("Element 5 (Box 6) - Target New Graphic Variant")]
    [SerializeField] private GameObject customNewSpriteObject;

    [Header("Element 6 (Box 7) - Player Object Setup")]
    [SerializeField] private GameObject playerSpriteObject;

    [Header("Element 7 (Box 8) - Celestial Gravity Sink")]
    [SerializeField] private GameObject blackHoleObject;

    private List<GameObject> instantiatedPlanets = new List<GameObject>();
    private CameraPan cameraPanScript;
    private bool isCameraAutoScrolling = false;
    private bool isSubscribed = false;
    private Coroutine planetSpawnRoutine;
    private bool keepSpawningRandomly = false;

    // Independent position tracker to bypass any frame rendering/Damp lag
    private float customScrollX = 0f;

    private void Awake()
    {
        cameraPanScript = FindFirstObjectByType<CameraPan>();
        
        // Catch the script reference instantly if it exists in the scene hierarchy layout
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnLineChanged += HandleLineChanged;
            isSubscribed = true;
        }
    }

    private void Start()
    {
        // Safety fallback sequence if DialogueManager loaded slightly out of order
        if (!isSubscribed)
        {
            StartCoroutine(DelayedSubscribeRoutine());
        }
    }

    private IEnumerator DelayedSubscribeRoutine()
    {
        while (DialogueManager.Instance == null)
        {
            yield return null;
        }
        
        if (!isSubscribed)
        {
            DialogueManager.Instance.OnLineChanged += HandleLineChanged;
            isSubscribed = true;
            
            // If we subscribed late, manually evaluate the active dialogue sequence state
            HandleLineChanged(DialogueManager.Instance.CurrentLineIndex);
        }
    }

    private void OnDestroy()
    {
        if (isSubscribed && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnLineChanged -= HandleLineChanged;
        }
    }

    private void Update()
    {
        // 1. Force state positions down to the CameraPan layout script safely
        if (isCameraAutoScrolling && cameraPanScript != null)
        {
            customScrollX += cameraScrollSpeed * Time.deltaTime;
            cameraPanScript.SetPanTarget(new Vector3(customScrollX, 0f, -10f));
        }

        float camX = (cameraPanScript != null) ? cameraPanScript.transform.position.x : customScrollX;

        // 2. Handle planet floating movement & off-screen cleanup
        for (int i = instantiatedPlanets.Count - 1; i >= 0; i--)
        {
            GameObject planet = instantiatedPlanets[i];
            if (planet != null)
            {
                planet.transform.Translate(Vector3.left * planetFloatSpeed * Time.deltaTime, Space.World);

                if (planet.transform.position.x < camX - 20f)
                {
                    Destroy(planet);
                    instantiatedPlanets.RemoveAt(i);
                }
            }
            else
            {
                instantiatedPlanets.RemoveAt(i);
            }
        }
    }

    private void HandleLineChanged(int lineIndex)
    {
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevelIndex != 0) return;

        switch (lineIndex)
        {
            case 0: 
                if (cameraPanScript != null)
                {
                    customScrollX = cameraPanScript.transform.position.x;
                }
                else
                {
                    customScrollX = 0f;
                }

                isCameraAutoScrolling = true;
                if (customNewSpriteObject != null) customNewSpriteObject.SetActive(false);
                if (playerSpriteObject != null) playerSpriteObject.SetActive(false);
                if (blackHoleObject != null) blackHoleObject.SetActive(false);
                break;

            case 2: 
                StartInfiniteRandomSpawning();
                break;

            case 5: 
                isCameraAutoScrolling = false; 
                StopSpawningPlanets(); 
                ClearIntroPlanets();   
                
                if (cameraPanScript != null)
                {
                    cameraPanScript.SetPanTarget(new Vector3(0f, 0f, -10f));
                    cameraPanScript.transform.position = new Vector3(0f, 0f, -10f);
                }

                if (customNewSpriteObject != null) customNewSpriteObject.SetActive(true);
                break;

            case 6: 
                if (playerSpriteObject != null) playerSpriteObject.SetActive(true);
                break;

            case 7: 
                if (blackHoleObject != null) blackHoleObject.SetActive(true);
                break;

            case 12: 
                if (cameraPanScript != null)
                {
                    cameraPanScript.SetCameraLock(false);
                }
                break;

            case -1: 
                isCameraAutoScrolling = false;
                StopSpawningPlanets();
                if (cameraPanScript != null)
                {
                    cameraPanScript.SetCameraLock(false);
                }
                break;
        }
    }

    private void StartInfiniteRandomSpawning()
    {
        if (planetSpawnRoutine != null) StopCoroutine(planetSpawnRoutine);
        
        if (planetsPool == null || planetsPool.Count == 0)
        {
            Debug.LogWarning("[DialogueTrigger] No planets assigned in planetsPool layout array!");
            return;
        }

        keepSpawningRandomly = true;
        planetSpawnRoutine = StartCoroutine(SpawnPlanetsInfiniteRoutine());
    }

    private IEnumerator SpawnPlanetsInfiniteRoutine()
    {
        while (keepSpawningRandomly)
        {
            int randomIndex = Random.Range(0, planetsPool.Count);
            SpawnablePlanet setup = planetsPool[randomIndex];

            if (setup.planetPrefab != null)
            {
                Vector3 preciseWorldSpawn = new Vector3(customScrollX + spawnXOffsetFromCamera, setup.spawnYPosition, 0f);
                
                GameObject spawned = Instantiate(setup.planetPrefab, preciseWorldSpawn, Quaternion.identity);
                if (setup.spawnScale != Vector3.zero)
                {
                    spawned.transform.localScale = setup.spawnScale;
                }
                instantiatedPlanets.Add(spawned);
            }

            yield return new WaitForSeconds(delayBetweenPlanets);
        }
    }

    private void StopSpawningPlanets()
    {
        keepSpawningRandomly = false;
        if (planetSpawnRoutine != null)
        {
            StopCoroutine(planetSpawnRoutine);
            planetSpawnRoutine = null;
        }
    }

    private void ClearIntroPlanets()
    {
        StopSpawningPlanets();

        foreach (GameObject planet in instantiatedPlanets)
        {
            if (planet != null) Destroy(planet);
        }
        instantiatedPlanets.Clear();
    }
}