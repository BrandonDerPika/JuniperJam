using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Transform visualsParent;
    [SerializeField] private Animator playerBodyAnimator;
    [SerializeField] private Animator trailAnimator;
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private SpriteRenderer trailSpriteRenderer;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float minVelocityToRotate = 0.1f;

    [Header("Hit Visual Settings")]
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private float hitDuration = 0.25f;

    private Rigidbody2D rb;
    private Coroutine hitCoroutine;
    private Sprite defaultSprite;
    private bool permanentHitActive = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (bodySpriteRenderer == null)
            bodySpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (bodySpriteRenderer != null)
            defaultSprite = bodySpriteRenderer.sprite;
    }

    private void Update()
    {
        if (GameState.Instance == null)
            return;

        bool isRunning = !GameState.Instance.IsPlanningOrPaused;
        if (trailSpriteRenderer != null)
        {
            trailSpriteRenderer.enabled = isRunning; 
        }
        if (isRunning && rb != null && visualsParent != null)
        {
            HandleRotation();
        }
        if (permanentHitActive) return;

        if (hitCoroutine == null)
        {
            if (playerBodyAnimator != null)
            {
                playerBodyAnimator.enabled = true;
                playerBodyAnimator.SetBool("IsFlying", isRunning);
            }

            if (trailAnimator != null)
            {
                trailAnimator.SetBool("ShowTrail", isRunning);
            }
        }
    }

    private void HandleRotation()
    {
        if (rb.velocity.sqrMagnitude > minVelocityToRotate * minVelocityToRotate)
        {
            float targetAngle = Mathf.Atan2(-rb.velocity.y, -rb.velocity.x) * Mathf.Rad2Deg;
            float adjustmentModifier = 0f; 

            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle + adjustmentModifier);

            visualsParent.rotation = Quaternion.Lerp(visualsParent.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    public void TriggerHitVisual()
    {
        if (permanentHitActive || bodySpriteRenderer == null || hitSprite == null)
            return;

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        hitCoroutine = StartCoroutine(HitSpriteRoutine());
    }
    public void TriggerTargetHitVisual()
    {
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }

        permanentHitActive = true;

        if (playerBodyAnimator != null)
            playerBodyAnimator.enabled = false;

        if (bodySpriteRenderer != null && hitSprite != null)
            bodySpriteRenderer.sprite = hitSprite;
    }

    public void ResetVisuals()
    {
        permanentHitActive = false;

        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }
        if (playerBodyAnimator != null)
            playerBodyAnimator.enabled = true;

        if (bodySpriteRenderer != null && defaultSprite != null)
        {
            bodySpriteRenderer.sprite = defaultSprite;
        }
        if (visualsParent != null)
        {
            visualsParent.rotation = Quaternion.identity;
        }
    }

    private IEnumerator HitSpriteRoutine()
    {
        if (playerBodyAnimator != null)
            playerBodyAnimator.enabled = false;

        bodySpriteRenderer.sprite = hitSprite;
        
        yield return new WaitForSeconds(hitDuration);

        if (playerBodyAnimator != null)
            playerBodyAnimator.enabled = true;

        hitCoroutine = null;
    }
}
