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

    [Header("Level 2 UI Alternator Settings")]
    [SerializeField] private Image targetUiImage;
    [SerializeField] private Sprite frameA;
    [SerializeField] private Sprite frameB;
    [SerializeField] private float swapDelay = 1f;

    private Coroutine alternatorCoroutine;

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

        if (currentLevelIndex == 2)
        {
            if (alternatorCoroutine == null && targetUiImage != null && frameA != null && frameB != null)
            {
                alternatorCoroutine = StartCoroutine(UiAlternatorRoutine());
            }
        }
        else
        {
            if (alternatorCoroutine != null)
            {
                StopCoroutine(alternatorCoroutine);
                alternatorCoroutine = null;
            }
        }
    }

    private IEnumerator UiAlternatorRoutine()
    {
        bool useFrameA = true;

        while (true)
        {
            targetUiImage.sprite = useFrameA ? frameA : frameB;
            useFrameA = !useFrameA;
            yield return new WaitForSeconds(swapDelay);
        }
    }
}