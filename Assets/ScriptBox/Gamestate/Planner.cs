using UnityEngine;

public class Planner : MonoBehaviour
{
    public enum PlanetType
    {
        Circle,
        Triangle,
        Square
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject circlePlanetPrefab;
    [SerializeField] private GameObject trianglePlanetPrefab;
    [SerializeField] private GameObject squarePlanetPrefab;

    [Header("Limits")]
    [SerializeField] private int circleLimit = 3;
    [SerializeField] private int triangleLimit = 1;
    [SerializeField] private int squareLimit = 0;

    [Header("References")]
    [SerializeField] private Camera cam;
    [Range(0f, 1f)] [SerializeField] private float dragOpacity = 0.5f;
    [SerializeField] private BottomPanel bottomPanel;

    [Header("Placement Rules")]
    [SerializeField] private LayerMask blockingLayers;          
    [SerializeField] private float radiusBuffer = 0.05f; 

    private PlanetType selectedType = PlanetType.Circle;
    private GameObject currentDragPreview;
    private bool isDragging = false;
    private bool isPlacementBlocked = false;
    private bool isRepositioningExistingPlanet = false;
    private float currentPlacementCheckRadius = 0.5f; 

    public void SetCircleLimit(int value) => circleLimit = Mathf.Max(0, value);
    public void SetTriangleLimit(int value) => triangleLimit = Mathf.Max(0, value);
    public void SetSquareLimit(int value) => squareLimit = Mathf.Max(0, value);

    private void Update()
    {
        if (GameState.Instance == null)
            return;

        if (!GameState.Instance.IsPlanningOrPaused)
        {
            if (isDragging)
            {
                CancelDrag();
            }
            return;
        }

        if (isDragging)
        {
            HandleDragMovement();
        }
        else
        {
            HandleRepositioningCheck(); 
        }
        
        HandleDeletion();
    }

    // Called by ui buttons
    public void StartDraggingPlanet(int typeIndex)
    {
        if (!GameState.Instance.IsPlanningOrPaused || isDragging)
            return;

        selectedType = (PlanetType)typeIndex;

        if (!HasAvailablePlanets(selectedType))
            return;

        GameObject prefab = GetSelectedPrefab();
        if (prefab == null)
            return;

        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        currentDragPreview = Instantiate(prefab, worldPos, Quaternion.identity);
        
        isRepositioningExistingPlanet = false;
        CalculateDynamicRadius(currentDragPreview);
        TogglePhysicsInteractions(currentDragPreview, false);
        SetObjectColor(currentDragPreview, Color.white, dragOpacity);

        isDragging = true;

        if (bottomPanel != null)
        {
            bottomPanel.SlideDownForDrag();
        }
    }

    // checks for left click holds on existing objects in world space
    private void HandleRepositioningCheck()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Planet"))
        {
            GravitySource source = hit.collider.GetComponent<GravitySource>();
            if (source != null)
            {
                currentDragPreview = hit.collider.gameObject;
                selectedType = source.Type;
                isRepositioningExistingPlanet = true; 

                CalculateDynamicRadius(currentDragPreview);
                TogglePhysicsInteractions(currentDragPreview, false);
                SetObjectColor(currentDragPreview, Color.white, dragOpacity);

                isDragging = true;

                if (bottomPanel != null)
                {
                    bottomPanel.SlideDownForDrag();
                }
            }
        }
    }

    private void HandleDragMovement()
    {
        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        currentDragPreview.transform.position = worldPos;

        Collider2D hit = Physics2D.OverlapCircle(worldPos, currentPlacementCheckRadius, blockingLayers);
        
        if (hit != null && hit.gameObject == currentDragPreview)
        {
            hit = null; 
        }

        if (hit != null)
        {
            isPlacementBlocked = true;
            SetObjectColor(currentDragPreview, Color.red, dragOpacity);
        }
        else
        {
            isPlacementBlocked = false;
            SetObjectColor(currentDragPreview, Color.white, dragOpacity);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isPlacementBlocked)
            {
                CancelDrag(); 
            }
            else
            {
                FinalizePlacement();
            }
        }
    }

    private void FinalizePlacement()
    {
        TogglePhysicsInteractions(currentDragPreview, true);
        SetObjectColor(currentDragPreview, Color.white, 1f);
        
        if (!isRepositioningExistingPlanet)
        {
            ModifyLimit(selectedType, -1);
        }
        
        currentDragPreview = null;
        isDragging = false;
        isRepositioningExistingPlanet = false;

        if (bottomPanel != null)
        {
            bottomPanel.SlideUpAfterDrag();
        }
    }

    private void CancelDrag()
    {
        if (currentDragPreview != null)
        {
            if (isRepositioningExistingPlanet)
            {
                TogglePhysicsInteractions(currentDragPreview, true);
                SetObjectColor(currentDragPreview, Color.white, 1f);
            }
            else
            {
                Destroy(currentDragPreview);
            }
        }
        
        isDragging = false;
        isRepositioningExistingPlanet = false;
        currentDragPreview = null;

        if (bottomPanel != null)
        {
            bottomPanel.SlideUpAfterDrag();
        }
    }

    private void HandleDeletion()
    {
        if (!GameState.Instance.IsPlanningOrPaused || isDragging)
            return;

        if (!Input.GetMouseButtonDown(1))
            return;

        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Planet"))
        {
            GravitySource source = hit.collider.GetComponent<GravitySource>();
            if (source != null)
            {
                ModifyLimit(source.Type, 1);
                Destroy(hit.collider.gameObject);
            }
        }
    }

    private bool HasAvailablePlanets(PlanetType type)
    {
        return GetLimitValue(type) > 0;
    }

    private int GetLimitValue(PlanetType type)
    {
        return type switch
        {
            PlanetType.Circle => circleLimit,
            PlanetType.Triangle => triangleLimit,
            PlanetType.Square => squareLimit,
            _ => 0
        };
    }

    private void ModifyLimit(PlanetType type, int change)
    {
        switch (type)
        {
            case PlanetType.Circle:
                circleLimit = Mathf.Max(0, circleLimit + change);
                break;
            case PlanetType.Triangle:
                triangleLimit = Mathf.Max(0, triangleLimit + change);
                break;
            case PlanetType.Square:
                squareLimit = Mathf.Max(0, squareLimit + change);
                break;
        }
    }

    private GameObject GetSelectedPrefab()
    {
        switch (selectedType)
        {
            case PlanetType.Circle:
                return circlePlanetPrefab;
            case PlanetType.Triangle:
                return trianglePlanetPrefab;
            case PlanetType.Square:
                return squarePlanetPrefab;
            default:
                return circlePlanetPrefab;
        }
    }

    private void CalculateDynamicRadius(GameObject targetObj)
    {
        Collider2D col = targetObj.GetComponent<Collider2D>();
        if (col != null)
        {
            currentPlacementCheckRadius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.y) + radiusBuffer;
        }
        else
        {
            currentPlacementCheckRadius = 0.5f;
        }
    }

    private void TogglePhysicsInteractions(GameObject obj, bool isSolid)
    {
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.isTrigger = !isSolid;
        }

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (!isSolid)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
    }

    private void SetObjectColor(GameObject obj, Color targetColor, float alpha)
    {
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            targetColor.a = alpha;
            spriteRenderer.color = targetColor;
        }
    }
}
