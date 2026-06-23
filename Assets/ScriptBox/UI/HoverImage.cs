using UnityEngine;
using UnityEngine.UI;

public class HoverImage : MonoBehaviour
{
    [Header("Setup")]
    public RectTransform hoverArea; 
    public Image targetImage; 

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite hoverSprite;

    private bool isHovering = false;


    void Update()
    {
        if (hoverArea == null || targetImage == null) return;

        bool mouseInside = RectTransformUtility.RectangleContainsScreenPoint(hoverArea, Input.mousePosition, null);

        if (mouseInside && !isHovering)
        {
            isHovering = true;
            if (hoverSprite != null) targetImage.sprite = hoverSprite;
        }
        else if (!mouseInside && isHovering)
        {
            isHovering = false;
            if (normalSprite != null) targetImage.sprite = normalSprite;
        }
    }
}
