using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (SFXManager.Instance != null)
                SFXManager.Instance.PlayTargetHit();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CompleteCurrentLevel();
            }
        }
    }
}
