using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Destroyer : MonoBehaviour
{
    [Header("Momentum Transfer Settings")]
    [Range(0f, 1f)] [SerializeField] private float speedTransferPercentage = 0.8f; 
    [Range(0f, 1f)] [SerializeField] private float playerSpeedRetention = 0.4f;   

    [Header("Safety Speed Caps")]
    [SerializeField] private float absoluteMaxLaunchSpeed = 40f; 

    [Header("Hit Visuals")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Sprite alternateHitSprite;

    private Rigidbody2D rb;
    private Vector2 initialPosition;
    private Vector2 initialVelocity;
    private bool isSubscribed = false;
    private Sprite defaultSprite;
    private Coroutine flashRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPosition = rb.position;
        initialVelocity = rb.velocity;

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }

        if (targetRenderer != null)
        {
            defaultSprite = targetRenderer.sprite;
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (isSubscribed && GameState.Instance != null)
        {
            GameState.Instance.OnPhaseChanged -= HandlePhaseChanged;
            GameState.Instance.OnCheckpointSnapped -= HandleCheckpointSnapped;
            isSubscribed = false;
        }
    }

    private void Update()
    {
        if (!isSubscribed) TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (!isSubscribed && GameState.Instance != null)
        {
            GameState.Instance.OnPhaseChanged += HandlePhaseChanged;
            GameState.Instance.OnCheckpointSnapped += HandleCheckpointSnapped;
            isSubscribed = true;
        }
    }

    private void HandleCheckpointSnapped()
    {
        initialPosition = rb.position;
        initialVelocity = rb.velocity;
    }

    private void HandlePhaseChanged(GameState.Phase newPhase)
    {
        if (newPhase == GameState.Phase.Planning || newPhase == GameState.Phase.Pause)
        {
            rb.position = initialPosition;
            rb.velocity = initialVelocity;
            rb.angularVelocity = 0f; 

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            if (targetRenderer != null)
            {
                targetRenderer.sprite = defaultSprite;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameState.Instance == null || !GameState.Instance.IsRunning) return;

        bool isPlayer = collision.gameObject.CompareTag("Player");
        bool isAsteroid = collision.gameObject.CompareTag("Asteroid");

        if (isPlayer || isAsteroid)
        {
            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlayRandomExplosion();
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }
            flashRoutine = StartCoroutine(FlashSpriteRoutine());
        }

        if (isPlayer)
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 centerLine = rb.position - playerRb.position;
                Vector2 destroyerLaunchDirection = centerLine.sqrMagnitude > 0.001f ? centerLine.normalized : collision.relativeVelocity.normalized;

                float incomingSpeed = collision.relativeVelocity.magnitude;
                if (incomingSpeed <= 0.001f || float.IsNaN(incomingSpeed)) return;

                Vector2 playerIncomingDir = collision.relativeVelocity.normalized;

                float transferredSpeed = incomingSpeed * speedTransferPercentage;
                transferredSpeed = Mathf.Clamp(transferredSpeed, 0f, absoluteMaxLaunchSpeed);
                rb.velocity = destroyerLaunchDirection * transferredSpeed;

                float centerFactor = Mathf.Clamp01(Vector2.Dot(playerIncomingDir, destroyerLaunchDirection));
                float edgeGlanceWeight = (1f - centerFactor) * 0.5f;

                Vector2 tangent = new Vector2(-destroyerLaunchDirection.y, destroyerLaunchDirection.x);

                if (Vector2.Dot(playerIncomingDir, tangent) < 0f)
                {
                    tangent = -tangent;
                }

                Vector2 playerDeflectionDirection = Vector2.Lerp(playerIncomingDir, tangent, edgeGlanceWeight).normalized;
                playerRb.velocity = playerDeflectionDirection * (incomingSpeed * playerSpeedRetention);
            }
        }
    }

    private IEnumerator FlashSpriteRoutine()
    {
        if (targetRenderer != null && alternateHitSprite != null)
        {
            targetRenderer.sprite = alternateHitSprite;
        }

        yield return new WaitForSeconds(0.25f);

        if (targetRenderer != null)
        {
            targetRenderer.sprite = defaultSprite;
        }

        flashRoutine = null;
    }
}
