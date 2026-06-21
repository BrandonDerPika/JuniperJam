using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layer;
        [Range(0f, 1f)]
        public float parallaxMultiplier;
    }

    [Header("Layers")]
    public ParallaxLayer[] layers;

    [Header("Camera")]
    public Transform cameraTransform;

    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layer == null) continue;

            Vector3 newPos = layer.layer.position;
            newPos.x += deltaMovement.x * layer.parallaxMultiplier;
            newPos.y += deltaMovement.y * layer.parallaxMultiplier;
            layer.layer.position = newPos;
        }

        lastCameraPosition = cameraTransform.position;
    }
}
