using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Transform visualsParent; // <-- Drag the "Visuals" child object here!
    [SerializeField] private Animator playerBodyAnimator;
    [SerializeField] private Animator trailAnimator;
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private SpriteRenderer trailSpriteRenderer;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f; // How fast it turns to face its velocity
    [SerializeField] private float minVelocityToRotate = 0.1f; // Don't rotate if practically still

    [Header("Hit Visual Settings")]
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private float hitDuration = 0.25f;

    private Rigidbody2D rb;
    private Coroutine hitCoroutine;
    private Sprite defaultSprite;

    private void Awake()
    {
        // Get the Rigidbody2D from the main Player object
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

        // Force the trail sprite to be completely invisible during planning/pause
        if (trailSpriteRenderer != null)
        {
            trailSpriteRenderer.enabled = isRunning; 
        }

        // Handle Rotation towards Velocity when running
        if (isRunning && rb != null && visualsParent != null)
        {
            HandleRotation();
        }

        // Only update animations if the hit flash isn't forcing a specific sprite
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
        // Only rotate if the player is moving fast enough
        if (rb.velocity.sqrMagnitude > minVelocityToRotate * minVelocityToRotate)
        {
            // Calculate the angle in degrees from the velocity vector
            float targetAngle = Mathf.Atan2(-rb.velocity.y, -rb.velocity.x) * Mathf.Rad2Deg;

            // If your sprite naturally faces UP, change this to: targetAngle - 90f;
            // If your sprite naturally faces RIGHT, leave it at: targetAngle;
            float adjustmentModifier = 0f; 

            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle + adjustmentModifier);

            // Smoothly rotate towards the target direction over time
            visualsParent.rotation = Quaternion.Lerp(visualsParent.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void TriggerHitVisual()
    {
        if (bodySpriteRenderer == null || hitSprite == null)
            return;

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        hitCoroutine = StartCoroutine(HitSpriteRoutine());
    }

    public void ResetVisuals()
    {
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

        // Reset the rotation back to default when planning/pausing resets
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