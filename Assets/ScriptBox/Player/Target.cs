using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject player;
    public GameObject uiToEnable;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for specific identity intersection with the tracking entity reference
        if (other.gameObject == player)
        {
            uiToEnable.SetActive(true);
        }
    }
}