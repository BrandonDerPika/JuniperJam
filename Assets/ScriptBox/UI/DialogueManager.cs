using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    [SerializeField] private RectTransform dialoguePanel;
    [SerializeField] private Text dialogueText;

    [Header("Movement Settings")]
    [SerializeField] private Vector2 activeAnchoredPosition = new Vector2(0f, 100f);
    [SerializeField] private Vector2 hiddenAnchoredPosition = new Vector2(0f, -500f);
    [SerializeField] private float slideSpeed = 5f;

    private List<string> currentDialogueLines = new List<string>();
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private Coroutine movementRoutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.anchoredPosition = hiddenAnchoredPosition;
        }
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            AdvanceDialogue();
        }
    }

    public void StartDialogue(List<string> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            HidePanel();
            return;
        }

        currentDialogueLines = new List<string>(lines);
        currentLineIndex = 0;
        isDialogueActive = true;

        if (dialogueText != null)
        {
            dialogueText.text = currentDialogueLines[currentLineIndex];
        }

        ShowPanel();
    }

    private void AdvanceDialogue()
    {
        currentLineIndex++;

        if (currentLineIndex < currentDialogueLines.Count)
        {
            if (dialogueText != null)
            {
                dialogueText.text = currentDialogueLines[currentLineIndex];
            }
        }
        else
        {
            isDialogueActive = false;
            HidePanel();
        }
    }

    private void ShowPanel()
    {
        if (movementRoutine != null) StopCoroutine(movementRoutine);
        movementRoutine = StartCoroutine(SlidePanelRoutine(activeAnchoredPosition));
    }

    private void HidePanel()
    {
        if (movementRoutine != null) StopCoroutine(movementRoutine);
        movementRoutine = StartCoroutine(SlidePanelRoutine(hiddenAnchoredPosition));
    }

    private IEnumerator SlidePanelRoutine(Vector2 targetAnchoredPos)
    {
        if (dialoguePanel == null) yield break;

        while (Vector2.Distance(dialoguePanel.anchoredPosition, targetAnchoredPos) > 0.5f)
        {
            dialoguePanel.anchoredPosition = Vector2.Lerp(
                dialoguePanel.anchoredPosition, 
                targetAnchoredPos, 
                Time.deltaTime * slideSpeed
            );
            yield return null;
        }

        dialoguePanel.anchoredPosition = targetAnchoredPos;
    }
}