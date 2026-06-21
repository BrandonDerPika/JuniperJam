using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Destroyer : MonoBehaviour
{
    [Header("Momentum Transfer Settings")]
    [Range(0f, 1f)] 
    [SerializeField] private float speedTransferPercentage = 0.8f; 
    
    [Range(0f, 1f)] 
    [SerializeField] private float playerSpeedRetention = 0.4f;   

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
            isSubscribed = true;
        }
    }

    private void HandlePhaseChanged(GameState.Phase newPhase)
    {
        if (newPhase == GameState.Phase.Planning)
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
                // Line of Centers
                Vector2 centerLine = rb.position - playerRb.position;
                Vector2 destroyerLaunchDirection = centerLine.sqrMagnitude > 0.001f ? centerLine.normalized : collision.relativeVelocity.normalized;

                // Get incoming speed and direction
                float incomingSpeed = collision.relativeVelocity.magnitude;
                if (incomingSpeed <= 0.001f || float.IsNaN(incomingSpeed)) return;

                Vector2 playerIncomingDir = collision.relativeVelocity.normalized;

                // Launch Destroyer forward along the line of centers
                float transferredSpeed = incomingSpeed * speedTransferPercentage;
                transferredSpeed = Mathf.Clamp(transferredSpeed, 0f, absoluteMaxLaunchSpeed);
                rb.velocity = destroyerLaunchDirection * transferredSpeed;

                // Calculate how "centered" the hit is.
                float centerFactor = Mathf.Clamp01(Vector2.Dot(playerIncomingDir, destroyerLaunchDirection));
                
                // If the hit is dead-center, edgeGlanceWeight becomes 0 (no side deflection).
                // If it's a glancing blow, edgeGlanceWeight scales up towards 0.5f.
                float edgeGlanceWeight = (1f - centerFactor) * 0.5f;

                // Calculate the Tangent Plane (Perpendicular to the launch direction)
                Vector2 tangent = new Vector2(-destroyerLaunchDirection.y, destroyerLaunchDirection.x);

                // Align the tangent with the players side movement drift direction
                if (Vector2.Dot(playerIncomingDir, tangent) < 0f)
                {
                    tangent = -tangent;
                }

                // Dead center = stays perfectly inline with playerIncomingDir
                // Edge hit = blends out towards the tangent vector path
                Vector2 playerDeflectionDirection = Vector2.Lerp(playerIncomingDir, tangent, edgeGlanceWeight).normalized;

                playerRb.velocity = playerDeflectionDirection * (incomingSpeed * playerSpeedRetention);
                
                Debug.Log($"Hit processed. Center Accuracy: {centerFactor * 100}%. Deflection Weight Applied: {edgeGlanceWeight}");
            }
        }
    }
}