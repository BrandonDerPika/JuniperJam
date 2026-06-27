using UnityEngine;

public class PlanetScroller : MonoBehaviour
{
    private float speed;
    private float destroyOffset;
    private Transform cameraTransform;

    public void Init(float scrollSpeed, float destroyX)
    {
        speed = scrollSpeed;
        destroyOffset = destroyX;
        cameraTransform = Camera.main.transform;

        Vector3 pos = transform.position;
        pos.y = Random.Range(-0.5f, 0.5f);
        transform.position = pos;
    }

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
        if (transform.position.x < cameraTransform.position.x - destroyOffset)
        {
            Destroy(gameObject);
        }
    }
}
