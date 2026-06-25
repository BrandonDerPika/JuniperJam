using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static bool HasReachedCheckpoint { get; private set; } = false;

    private bool isTriggered = false;

    private void Start()
    {
        HasReachedCheckpoint = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.CompareTag("Player"))
        {
            isTriggered = true;
            HasReachedCheckpoint = true;
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
