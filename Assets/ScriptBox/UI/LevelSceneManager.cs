using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSceneManager : MonoBehaviour
{
    [System.Serializable]
    public struct ManagedElement
    {
        public GameObject targetObject;
        public int enableAtLevelIndex;
        public bool exactLevelOnly;
    }

    [Header("UI & Interaction Controls")]
    [SerializeField] private Button skipButton;
    [SerializeField] private int skipButtonDisableIndex = 0;

    [Header("Generic Managed Scene Elements")]
    [SerializeField] private List<ManagedElement> managedElements = new List<ManagedElement>();

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelLoaded += EvaluateSceneElements;
            EvaluateSceneElements(LevelManager.Instance.CurrentLevelIndex);
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelLoaded -= EvaluateSceneElements;
        }
    }

    private void EvaluateSceneElements(int currentLevelIndex)
    {
        if (skipButton != null)
        {
            skipButton.interactable = (currentLevelIndex != skipButtonDisableIndex);
        }

        foreach (ManagedElement element in managedElements)
        {
            if (element.targetObject == null) continue;

            if (element.exactLevelOnly)
            {
                element.targetObject.SetActive(currentLevelIndex == element.enableAtLevelIndex);
            }
            else
            {
                element.targetObject.SetActive(currentLevelIndex >= element.enableAtLevelIndex);
            }
        }
    }
}