using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAlternator : MonoBehaviour
{
    [Header("Sprite Settings")]
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite differentSprite;
    [SerializeField] private float delayInSeconds = 1f;

    private SpriteRenderer spriteRenderer;
    private bool isAlternating = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (originalSprite == null && spriteRenderer.sprite != null)
        {
            originalSprite = spriteRenderer.sprite;
        }

        if (originalSprite != null && differentSprite != null)
        {
            StartAlternating();
        }
    }

    public void StartAlternating()
    {
        if (!isAlternating)
        {
            StartCoroutine(AlternateSpritesRoutine());
        }
    }

    public void StopAlternating()
    {
        isAlternating = false;
        StopAllCoroutines();
    }

    private IEnumerator AlternateSpritesRoutine()
    {
        isAlternating = true;
        bool useOriginal = true;

        while (isAlternating)
        {
            spriteRenderer.sprite = useOriginal ? originalSprite : differentSprite;
            
            useOriginal = !useOriginal;

            yield return new WaitForSeconds(delayInSeconds);
        }
    }
}
