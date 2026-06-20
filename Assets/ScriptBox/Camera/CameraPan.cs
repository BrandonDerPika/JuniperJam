using UnityEngine;

public class CameraPan : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float smoothTime = 0.15f;

    private Vector3 targetPosition;
    private Vector3 velocity;

    private void Start()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            input.y += 1;

        if (Input.GetKey(KeyCode.S))
            input.y -= 1;

        if (Input.GetKey(KeyCode.A))
            input.x -= 1;

        if (Input.GetKey(KeyCode.D))
            input.x += 1;

        input.Normalize();

        targetPosition += input * moveSpeed * Time.deltaTime;
    }

    private void LateUpdate()
    {
        // Execute frame transitions inside LateUpdate to guarantee sync accuracy with player movement updates
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}