using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Objects")]
    [SerializeField] private List<GameObject> levels = new List<GameObject>();

    private int currentLevelIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        LoadLevel(currentLevelIndex);
    }

    // Activates a single level and disables all others
    private void LoadLevel(int levelIndex)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            levels[i].SetActive(i == levelIndex);
        }
    }

    // Advances progression to the next available level
    public void CompleteLevel()
    {
        currentLevelIndex++;

        if (currentLevelIndex >= levels.Count)
        {
            Debug.Log("all levels complete!");
            return;
        }

        LoadLevel(currentLevelIndex);
    }
}