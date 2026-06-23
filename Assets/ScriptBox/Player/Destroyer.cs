using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Destroyer : MonoBehaviour
{
    [Header("Momentum Transfer Settings")]
    [Range(0f, 1f)] [SerializeField] private float speedTransferPercentage = 0.8f; 
    [Range(0f, 1f)] [SerializeField] private float playerSpeedRetention = 0.4f;   

    [Header("Safety Speed Caps")]
    [SerializeField] private float absoluteMaxLaunchSpeed = 40f; 

    private Rigidbody2D rb;
    private Vector2 initialPosition;
    private Vector2 initialVelocity;
    private bool isSubscribed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPosition = rb.position;
        initialVelocity = rb.velocity;
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
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameState.Instance == null || !GameState.Instance.IsRunning) return;

        if (collision.gameObject.CompareTag("Player"))
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
}
