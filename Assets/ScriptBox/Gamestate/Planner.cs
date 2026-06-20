using UnityEngine;

public class Planner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject circlePlanetPrefab;
    [SerializeField] private GameObject trianglePlanetPrefab;
    [SerializeField] private GameObject squarePlanetPrefab;

    [Header("References")]
    [SerializeField] private Camera cam;

    private enum PlanetType
    {
        Circle,
        Triangle,
        Square
    }

    private PlanetType selectedType = PlanetType.Circle;

    private void Update()
    {
        if (GameState.Instance == null)
            return;

        // Prevent modification or object generation outside of the designated planning sequence
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
            PrintDebug("Selected Planet: Circle");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedType = PlanetType.Triangle;
            PrintDebug("Selected Planet: Triangle");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedType = PlanetType.Square;
            PrintDebug("Selected Planet: Square");
        }
    }

    private void HandlePlacement()
    {
        if (!GameState.Instance.IsPlanning)
            return;

        if (!Input.GetKeyDown(KeyCode.Q))
            return;

        // Convert screen coordinate bounds to match simulation world coordinates
        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        GameObject prefab = GetSelectedPrefab();
        if (prefab == null)
        {
            PrintDebug("No prefab assigned.");
            return;
        }

        Instantiate(prefab, worldPos, Quaternion.identity);

        PrintDebug($"Placed {selectedType} planet at {worldPos}");
    }

    private void HandleDeletion()
    {
        if (!GameState.Instance.IsPlanning)
            return;

        // Listen for context click (Right Click) to handle object clearing rules
        if (!Input.GetMouseButtonDown(1))
            return;

        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        
        // Execute a structural point overlap cast to detect elements beneath the pointer target
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            // Verify structural component profile before committing to disposal routines
            GravitySource source = hit.collider.GetComponent<GravitySource>();
            if (source != null)
            {
                PrintDebug($"Removing target instance: {hit.collider.gameObject.name}");
                Destroy(hit.collider.gameObject);
            }
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