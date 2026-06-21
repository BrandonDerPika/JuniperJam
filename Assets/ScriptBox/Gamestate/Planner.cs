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

    private PlanetType selectedType = PlanetType.Circle;

    public void SetCircleLimit(int value) => circleLimit = Mathf.Max(0, value);
    public void SetTriangleLimit(int value) => triangleLimit = Mathf.Max(0, value);
    public void SetSquareLimit(int value) => squareLimit = Mathf.Max(0, value);

    private void Update()
    {
        if (GameState.Instance == null)
            return;

        // Prevent modification or object generation outside of the planning phase
        if (!GameState.Instance.IsPlanning)
            return;

        HandleSelectionInput();
        HandlePlacement();
        HandleDeletion();
    }

    private void HandleSelectionInput()
    {
        if (!GameState.Instance.IsPlanning)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedType = PlanetType.Circle;
            PrintDebug($"Selected Planet: Circle (Remaining: {circleLimit})");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedType = PlanetType.Triangle;
            PrintDebug($"Selected Planet: Triangle (Remaining: {triangleLimit})");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedType = PlanetType.Square;
            PrintDebug($"Selected Planet: Square (Remaining: {squareLimit})");
        }
    }

    private void HandlePlacement()
    {
        if (!GameState.Instance.IsPlanning)
            return;

        if (!Input.GetKeyDown(KeyCode.Q))
            return;

        // Verify limit before placing
        if (!HasAvailablePlanets(selectedType))
        {
            PrintDebug($"Cannot place {selectedType} planet: Limit reached!");
            return;
        }

        // Convert screen cords to world cords
        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        GameObject prefab = GetSelectedPrefab();
        if (prefab == null)
        {
            PrintDebug("No prefab assigned.");
            return;
        }

        Instantiate(prefab, worldPos, Quaternion.identity);
        
        // Subtract from budget 
        ModifyLimit(selectedType, -1);

        PrintDebug($"Placed {selectedType} planet at {worldPos}. Remaining: {GetLimitValue(selectedType)}");
    }

    private void HandleDeletion()
    {
        if (!GameState.Instance.IsPlanning)
            return;

        if (!Input.GetMouseButtonDown(1))
            return;

        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            GravitySource source = hit.collider.GetComponent<GravitySource>();
            if (source != null)
            {
                ModifyLimit(source.Type, 1);

                PrintDebug($"Removing target instance: {hit.collider.gameObject.name}. Refunded {source.Type}. Current: {GetLimitValue(source.Type)}");
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

    private void PrintDebug(string message)
    {
        Debug.Log(message);
    }
}