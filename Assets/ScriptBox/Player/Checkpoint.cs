using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static bool HasReachedCheckpoint { get; private set; } = false;

    private bool isTriggered = false;

    private void Start()
    {
        // Reset static data on level load
        HasReachedCheckpoint = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.CompareTag("Player"))
        {
            isTriggered = true;
            HasReachedCheckpoint = true;
            
            Debug.Log("Checkpoint Reached! Capturing snapshot across all space objects.");

            if (GameState.Instance != null)
            {
                // Freeze
                GameState.Instance.TriggerCheckpoint();
                
                // Pause dynamic objects
                GameState.Instance.SetPhase(GameState.Phase.Pause);
            }
        }
    }
}
