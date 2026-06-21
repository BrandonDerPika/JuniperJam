using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [Header("Cursor Textures")]
    [SerializeField] private Texture2D idleCursor;
    [SerializeField] private Texture2D clickCursor;

    [Header("Tuning")]
    [Tooltip("The pixel point inside the texture where the actual click registers. (0,0) is top-left.")]
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    private void Start()
    {
        // Set the default idle cursor at game launch
        SetHardwareCursor(idleCursor);
    }

    private void Update()
    {
        // Left clicks
        if (Input.GetMouseButtonDown(0))
        {
            SetHardwareCursor(clickCursor);
        }

        // Stops left clicking
        if (Input.GetMouseButtonUp(0))
        {
            SetHardwareCursor(idleCursor);
        }
    }

    private void SetHardwareCursor(Texture2D cursorTexture)
    {
        if (cursorTexture != null)
        {
            // reduce lag
            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        }
    }

    private void OnDisable()
    {
        // fallback
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}