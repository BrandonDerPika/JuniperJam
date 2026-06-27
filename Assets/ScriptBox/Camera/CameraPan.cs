using UnityEngine;
using UnityEngine.UI;

public class CameraPan : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float smoothTime = 0.15f;

    [Header("Controls")]
    public bool canMoveHorizontal = true;
    public bool canMoveVertical = true;

    [Header("UI References")]
    [SerializeField] private GameObject horizontalUIParent;
    [SerializeField] private GameObject verticalUIParent;

    [Header("Individual Arrow Images")]
    [SerializeField] private Image leftArrow;
    [SerializeField] private Image rightArrow;
    [SerializeField] private Image upArrow;
    [SerializeField] private Image downArrow;

    [Header("Detection Areas")]
    [SerializeField] private RectTransform leftDetectionArea;
    [SerializeField] private RectTransform rightDetectionArea;
    [SerializeField] private RectTransform upDetectionArea;
    [SerializeField] private RectTransform downDetectionArea;

    [Header("Overriding UI")]
    [SerializeField] private RectTransform blockingMenu;

    [Header("Sprite Variants")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite activeSprite;

    private Vector3 targetPosition;
    private Vector3 velocity;
    private Camera uiCamera; 

    private bool isLockedBySequence = false;

    private void Start()
    {
        targetPosition = transform.position;
        UpdateUIElements();

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            uiCamera = canvas.worldCamera;
        }
    }

    private void Update()
    {
        UpdateUIElements();
        
        // Block processing player inputs when sequence locks are active,
        // but do not block positions processed directly through SetPanTarget
        if (isLockedBySequence) return;
        if (!canMoveHorizontal && !canMoveVertical) return;

        Vector2 mousePos = Input.mousePosition;

        if (blockingMenu != null && blockingMenu.gameObject.activeInHierarchy)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(blockingMenu, mousePos, uiCamera))
            {
                ResetArrowSprites();
                return; 
            }
        }

        Vector3 input = Vector3.zero;
        ResetArrowSprites();

        if (canMoveHorizontal)
        {
            if (leftDetectionArea != null && RectTransformUtility.RectangleContainsScreenPoint(leftDetectionArea, mousePos, uiCamera))
            {
                input.x = -1f; 
                SetArrowActive(leftArrow);
            }
            else if (rightDetectionArea != null && RectTransformUtility.RectangleContainsScreenPoint(rightDetectionArea, mousePos, uiCamera))
            {
                input.x = 1f;  
                SetArrowActive(rightArrow);
            }
        }

        if (canMoveVertical)
        {
            if (downDetectionArea != null && RectTransformUtility.RectangleContainsScreenPoint(downDetectionArea, mousePos, uiCamera))
            {
                input.y = -1f; 
                SetArrowActive(downArrow);
            }
            else if (upDetectionArea != null && RectTransformUtility.RectangleContainsScreenPoint(upDetectionArea, mousePos, uiCamera))
            {
                input.y = 1f;  
                SetArrowActive(upArrow);
            }
        }

        input.Normalize();
        
        targetPosition.x += input.x * moveSpeed * Time.deltaTime;
        targetPosition.y += input.y * moveSpeed * Time.deltaTime;
    }

    private void LateUpdate()
    {
        // Keep smooth positioning updates independent of active logic lock state values
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
    
    public void SetCameraLock(bool shouldLock)
    {
        isLockedBySequence = shouldLock;
        if (shouldLock) ResetArrowSprites();
    }

    public void SetPanTarget(Vector3 destination)
    {
        targetPosition = destination;
    }

    public Vector3 GetPanTarget()
    {
        return targetPosition;
    }

    private void UpdateUIElements()
    {
        bool showHorizontal = canMoveHorizontal && !isLockedBySequence;
        bool showVertical = canMoveVertical && !isLockedBySequence;

        if (horizontalUIParent != null && horizontalUIParent.activeSelf != showHorizontal)
        {
            horizontalUIParent.SetActive(showHorizontal);
        }

        if (verticalUIParent != null && verticalUIParent.activeSelf != showVertical)
        {
            verticalUIParent.SetActive(showVertical);
        }
    }

    private void ResetArrowSprites()
    {
        if (defaultSprite == null) return;

        if (leftArrow != null) leftArrow.sprite = defaultSprite;
        if (rightArrow != null) rightArrow.sprite = defaultSprite;
        if (upArrow != null) upArrow.sprite = defaultSprite;
        if (downArrow != null) downArrow.sprite = defaultSprite;
    }

    private void SetArrowActive(Image arrowImage)
    {
        if (arrowImage != null && activeSprite != null)
        {
            arrowImage.sprite = activeSprite;
        }
    }
}