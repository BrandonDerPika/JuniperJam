using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verify structural identity against the designated tracking tag profile
        if (other.CompareTag(playerTag))
        {
            if (LevelManager.Instance != null)
            {
                // Tell the level manager we hit our target and want to progress
                LevelManager.Instance.CompleteCurrentLevel();
            }
            else
            {
                Debug.LogWarning("Target triggered, but no LevelManager instance found in scene!");
            }
        }
    }
}