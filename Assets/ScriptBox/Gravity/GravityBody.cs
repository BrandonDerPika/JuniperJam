using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityBody : MonoBehaviour
{
    [Header("Body")]
    [SerializeField] private bool useGravity = true;
    [SerializeField] private float minEscapeSpeed = 1.2f;
    [SerializeField] private float maxForcePerSource = 100f;

    private Rigidbody2D rb;

    public Rigidbody2D Rigidbody => rb;
    public Vector2 Velocity => rb != null ? rb.velocity : Vector2.zero;
    public float MinEscapeSpeed => minEscapeSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!GravitySystem.GravityEnabled || rb == null)
            return;

        Vector2 totalForce = Vector2.zero;

        for (int i = 0; i < GravitySource.ActiveSources.Count; i++)
        {
            GravitySource source = GravitySource.ActiveSources[i];
            if (source == null || !source.isActiveAndEnabled)
                continue;

            Vector2 force = source.GetForceOn(this);

            if (force.sqrMagnitude > maxForcePerSource * maxForcePerSource)
                force = force.normalized * maxForcePerSource;

            totalForce += force;
        }

        rb.AddForce(totalForce, ForceMode2D.Force);
    }
}