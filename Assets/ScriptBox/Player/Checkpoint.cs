using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static bool HasReachedCheckpoint { get; private set; } = false;

    [SerializeField] private Animator animator;
    private bool isTriggered = false;

    private void Start()
    {
        HasReachedCheckpoint = false;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.CompareTag("Player"))
        {
            isTriggered = true;
            HasReachedCheckpoint = true;

            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.angularVelocity = 0f;

                playerRb.position = new Vector2(transform.position.x, transform.position.y);
                collision.transform.position = new Vector3(transform.position.x, transform.position.y, collision.transform.position.z);
            }

            if (animator != null)
            {
                animator.SetTrigger("TriggerCheckpoint"); 
            }

            if (SFXManager.Instance != null)
                SFXManager.Instance.PlayCheckpoint();
                
            if (GameState.Instance != null)
            {
                GameState.Instance.TriggerCheckpoint();
                GameState.Instance.SetPhase(GameState.Phase.Pause);
            }
        }
    }
}