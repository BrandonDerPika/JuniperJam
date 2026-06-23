using UnityEngine;
using UnityEngine.UI;

public class CameraPan : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float smoothTime = 0.15f;

    [Header("Edge Detection (Percentage 0.0 to 1.0)")]
    [Range(0f, 0.5f)] public float horizontalThresholdPercentage = 0.05f;
    [Range(0f, 0.5f)] public float verticalThresholdPercentage = 0.05f;

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

    [Header("Sprite Variants")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite activeSprite;

    private Vector3 targetPosition;
    private Vector3 velocity;

    private void Start()
    {
        targetPosition = transform.position;
        UpdateUIElements();
    }

    private void Update()
    {
        UpdateUIElements();

        if (!canMoveHorizontal && !canMoveVertical) return;

        Vector3 input = Vector3.zero;
        
        float normalizedX = Input.mousePosition.x / Screen.width;
        float normalizedY = Input.mousePosition.y / Screen.height;

        ResetArrowSprites();

        if (canMoveHorizontal)
        {
            // Left Edge Check
            if (normalizedX >= 0f && normalizedX <= horizontalThresholdPercentage)
            {
                input.x = -1f; 
                SetArrowActive(leftArrow);
            }
            // Right Edge Check 
            else if (normalizedX >= (1f - horizontalThresholdPercentage) && normalizedX <= 1f)
            {
                input.x = 1f;  
                SetArrowActive(rightArrow);
            }
        }

        if (canMoveVertical)
        {
            // Bottom Edge Check 
            if (normalizedY >= 0f && normalizedY <= verticalThresholdPercentage)
            {
                input.y = -1f; 
                SetArrowActive(downArrow);
            }
            // Top Edge Check
            else if (normalizedY >= (1f - verticalThresholdPercentage) && normalizedY <= 1f)
            {
                input.y = 1f;  
                SetArrowActive(upArrow);
            }
        }

        input.Normalize();

        targetPosition += input * moveSpeed * Time.deltaTime;
    }

    private void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    private void UpdateUIElements()
    {
        if (horizontalUIParent != null && horizontalUIParent.activeSelf != canMoveHorizontal)
        {
            horizontalUIParent.SetActive(canMoveHorizontal);
        }

        if (verticalUIParent != null && verticalUIParent.activeSelf != canMoveVertical)
        {
            verticalUIParent.SetActive(canMoveVertical);
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
