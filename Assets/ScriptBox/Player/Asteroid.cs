using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Asteroid : MonoBehaviour
{
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
            HandlePhaseChanged(GameState.Instance.CurrentPhase);
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

    private void TriggerGameOver()
    {
        Debug.Log("Player hit an asteroid! Resetting game state phase.");

        // 🔊 PLAY DEATH SFX
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayPlayerDeath();

        if (GameState.Instance != null)
        {
            GameState.Instance.SetPhase(
                Checkpoint.HasReachedCheckpoint
                    ? GameState.Phase.Pause
                    : GameState.Phase.Planning
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameState.Instance == null || !GameState.Instance.IsRunning) return;

        if (collision.CompareTag("Player"))
        {
            TriggerGameOver();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameState.Instance == null || !GameState.Instance.IsRunning) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            TriggerGameOver();
            return;
        }

        if (collision.gameObject.CompareTag("Destroyer"))
        {
            Rigidbody2D destroyerRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (destroyerRb != null)
            {
                Vector2 centerLine = rb.position - destroyerRb.position;
                Vector2 transferDirection =
                    centerLine.sqrMagnitude > 0.001f
                        ? centerLine.normalized
                        : collision.relativeVelocity.normalized;

                float incomingSpeed = collision.relativeVelocity.magnitude;

                if (incomingSpeed <= 0.001f || float.IsNaN(incomingSpeed))
                    return;

                float transferredVelocity = incomingSpeed * 1.2f;

                rb.velocity = transferDirection * transferredVelocity;

                Debug.Log($"{gameObject.name} physically deflected by Destroyer.");
            }
        }
    }
}
