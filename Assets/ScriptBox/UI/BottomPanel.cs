using UnityEngine;
using System.Collections;

public class BottomPanel : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject panelA;
    public GameObject panelB;

    [Header("Movement Settings")]
    public float moveDistanceY = 100f;
    public float moveSpeed = 12f;

    private RectTransform panelBRect;
    private Vector2 panelBOriginalPosition;

    void Start()
    {
        if (panelB != null)
        {
            panelBRect = panelB.GetComponent<RectTransform>();
            panelBOriginalPosition = panelBRect.anchoredPosition;
        }
    }

    // Called by Button 1
    public void OnClickButton1()
    {
        panelB.SetActive(true);
        panelBRect.anchoredPosition = panelBOriginalPosition; 
        
        Vector2 targetPos = panelBOriginalPosition + new Vector2(0, moveDistanceY);
        
        StopAllCoroutines();
        StartCoroutine(MoveUI(panelBRect, targetPos));
        panelA.SetActive(false);
    }

    // Called by Button 2
    public void OnClickButton2()
    {
        panelA.SetActive(true); 

        StopAllCoroutines();
        StartCoroutine(MoveUI(panelBRect, panelBOriginalPosition, () => 
        {
            panelB.SetActive(false);
        }));
    }

    public void SlideDownForDrag()
    {
        if (panelBRect == null) return;
        StopAllCoroutines();
        StartCoroutine(MoveUI(panelBRect, panelBOriginalPosition));
    }

    public void SlideUpAfterDrag()
    {
        if (panelBRect == null) return;
        Vector2 targetPos = panelBOriginalPosition + new Vector2(0, moveDistanceY);
        StopAllCoroutines();
        StartCoroutine(MoveUI(panelBRect, targetPos));
    }

    private IEnumerator MoveUI(RectTransform rect, Vector2 targetPosition, System.Action onComplete = null)
    {
        while (Vector2.Distance(rect.anchoredPosition, targetPosition) > 1.0f)
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPosition, Time.deltaTime * moveSpeed);
            yield return null;
        }
        rect.anchoredPosition = targetPosition;
        onComplete?.Invoke();
    }
}
