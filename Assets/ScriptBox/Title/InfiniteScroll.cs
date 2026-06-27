using UnityEngine;

public class InfiniteScroll : MonoBehaviour
{
    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraSpeed = 1f;

    [Header("Planet Spawning")]
    public GameObject[] planetPrefabs;
    public float spawnInterval = 1.5f;
    public float spawnOffset = 3f;

    [Header("Planet Movement")]
    public float planetScrollSpeed = 0.25f;
    public float destroyXOffset = 10f;

    [Header("State")]
    public bool isEnabled = false;

    private float timer;

    void Start()
    {
        timer = spawnInterval;
    }

    void Update()
    {
        if (!isEnabled) return;

        cameraTransform.position += Vector3.right * cameraSpeed * Time.deltaTime;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnPlanet();
            timer = spawnInterval;
        }
    }

    void SpawnPlanet()
    {
        if (planetPrefabs == null || planetPrefabs.Length == 0) return;

        GameObject prefab = planetPrefabs[Random.Range(0, planetPrefabs.Length)];

        Vector3 spawnPos = new Vector3(
            cameraTransform.position.x + spawnOffset,
            Random.Range(-2f, 2f),
            0f
        );

        GameObject planet = Instantiate(prefab, spawnPos, Quaternion.identity);

        planet.AddComponent<PlanetScroller>().Init(planetScrollSpeed, destroyXOffset);
    }
}