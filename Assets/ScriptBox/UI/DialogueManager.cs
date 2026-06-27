using System;
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

    [Header("Typing Speed")]
    [SerializeField] private float timePerCharacter = 0.03f;

    private List<string> currentDialogueLines = new List<string>();
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private bool isCurrentlyTyping = false;
    
    private Coroutine movementRoutine;
    private Coroutine typingRoutine;

    public int CurrentLineIndex => currentLineIndex;
    public bool IsDialogueActive => isDialogueActive;
    public event Action<int> OnLineChanged;

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

        ShowPanel();
        StartTypingLine(currentDialogueLines[currentLineIndex]);
        
        // Notify tracking script that we started on line 0
        OnLineChanged?.Invoke(currentLineIndex);
    }


    public void OnDialogueButtonPressed()
    {
        if (!isDialogueActive) return;

        if (isCurrentlyTyping)
        {
            if (typingRoutine != null) StopCoroutine(typingRoutine);
            isCurrentlyTyping = false;
            if (dialogueText != null) dialogueText.text = currentDialogueLines[currentLineIndex];
        }
        else
        {
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        currentLineIndex++;

        if (currentLineIndex < currentDialogueLines.Count)
        {
            StartTypingLine(currentDialogueLines[currentLineIndex]);
            OnLineChanged?.Invoke(currentLineIndex);
        }
        else
        {
            isDialogueActive = false;
            HidePanel();
            // indicate dialogue closed/finished
            OnLineChanged?.Invoke(-1);
        }
    }

    private void StartTypingLine(string line)
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeTextRoutine(line));
    }

    private IEnumerator TypeTextRoutine(string targetText)
    {
        isCurrentlyTyping = true;
        if (dialogueText != null) dialogueText.text = "";

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayDialogueVoice();
            SFXManager.Instance.PlayTextScroll();
        }

        for (int i = 0; i <= targetText.Length; i++)
        {
            if (dialogueText != null)
            {
                dialogueText.text = targetText.Substring(0, i);
            }
            yield return new WaitForSeconds(timePerCharacter);
        }

        isCurrentlyTyping = false;
        typingRoutine = null;
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
