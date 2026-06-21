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
    }

    [Header("Level Configuration")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();
    [SerializeField] private int currentLevelIndex = 0;

    [Header("Scene References")]
    [SerializeField] private Planner plannerScript;
    [SerializeField] private GameObject victoryScreenUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (plannerScript == null)
        {
            plannerScript = FindFirstObjectByType<Planner>();
        }

        // Initialize and make sure everything except the first active level is disabled
        DeactivateAllLayouts();
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogWarning("No levels configured in the LevelManager!");
            return;
        }

        // Check
        if (index >= levels.Count)
        {
            CompleteAllLevels();
            return;
        }

        // Deactivate the old level layout before updating the index
        if (currentLevelIndex < levels.Count && levels[currentLevelIndex].levelLayoutObject != null)
        {
            levels[currentLevelIndex].levelLayoutObject.SetActive(false);
        }

        currentLevelIndex = index;
        LevelData currentLevel = levels[currentLevelIndex];
        Debug.Log($"Loading Level {currentLevelIndex + 1}: {currentLevel.levelName}");

        // Force set the exact resource limits on Planner script
        if (plannerScript != null)
        {
            plannerScript.SetCircleLimit(currentLevel.circlePlanetLimit);
            plannerScript.SetTriangleLimit(currentLevel.trianglePlanetLimit);
            plannerScript.SetSquareLimit(currentLevel.squarePlanetLimit);
        }

        // Clear out any physical planets already placed in the scene
        ClearCurrentPlanets();

        // Activate the game object layout for the level
        if (currentLevel.levelLayoutObject != null)
        {
            currentLevel.levelLayoutObject.SetActive(true);
        }

        // Force the game state back into planning mode for new setup
        if (GameState.Instance != null)
        {
            GameState.Instance.SetPhase(GameState.Phase.Planning);
        }
        
        if (victoryScreenUI != null) victoryScreenUI.SetActive(false);
    }

    public void CompleteCurrentLevel()
    {
        Debug.Log($"Level {currentLevelIndex + 1} Cleared!");
        
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
        // Safe check loop ensuring everything is turned off cleanly at boot
        foreach (LevelData level in levels)
        {
            if (level.levelLayoutObject != null)
            {
                level.levelLayoutObject.SetActive(false);
            }
        }
    }

    private void CompleteAllLevels()
    {
        Debug.Log("Congratulations!");
        
        // Turn off the final level's objects upon completion
        if (currentLevelIndex < levels.Count && levels[currentLevelIndex].levelLayoutObject != null)
        {
            levels[currentLevelIndex].levelLayoutObject.SetActive(false);
        }

        if (victoryScreenUI != null)
        {
            victoryScreenUI.SetActive(true);
        }
        
        if (GameState.Instance != null)
        {
            GameState.Instance.SetPhase(GameState.Phase.Planning);
        }

        // delete all planets
    }
}