using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    [Header("Target Tags")]
    [SerializeField] private string asteroidTag = "Asteroid";
    [SerializeField] private string destroyerTag = "Destroyer";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(asteroidTag) || collision.CompareTag(destroyerTag))
        {
            Destroy(collision.gameObject);
        }
    }
}