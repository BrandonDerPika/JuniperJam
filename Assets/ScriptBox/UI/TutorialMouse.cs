using System.Collections;
using UnityEngine;

public class TutorialMouse : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite clickedSprite;
    [SerializeField] private Vector3 offsetPosition;
    [SerializeField] private float speed = 5f;

    private Vector3 initialPosition;
    private bool isAnimating = false;

    private void Awake()
    {
        initialPosition = transform.position;
        gameObject.SetActive(false);
    }

    public void ActivateAnimation()
    {
        if (!isAnimating)
        {
            gameObject.SetActive(true);
            StartCoroutine(MouseAnimationRoutine());
        }
    }

    private IEnumerator MouseAnimationRoutine()
    {
        isAnimating = true;

        while (isAnimating)
        {
            spriteRenderer.sprite = clickedSprite;

            Vector3 targetPosition = initialPosition + offsetPosition;
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPosition;

            spriteRenderer.sprite = defaultSprite;

            yield return new WaitForSeconds(1f);

            transform.position = initialPosition;
        }
    }
}