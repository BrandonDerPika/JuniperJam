using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityBody : MonoBehaviour
{
    [Header("Body")]
    [SerializeField] private bool useGravity = true;
    [SerializeField] private float minEscapeSpeed = 1.2f;
    [SerializeField] private float maxForcePerSource = 100f;
    
    [Header("Safety Net Caps")]
    [SerializeField] private float maxVelocityMagnitude = 50f; // HARD CAP: Keeps objects from hitting infinite lightspeed spikes

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
        if (!GravitySystem.GravityEnabled || rb == null || !useGravity)
            return;

        Vector2 totalForce = Vector2.zero;

        for (int i = 0; i < GravitySource.ActiveSources.Count; i++)
        {
            GravitySource source = GravitySource.ActiveSources[i];
            if (source == null || !source.isActiveAndEnabled)
                continue;

            Vector2 force = source.GetForceOn(this);

            // If the gravity source returns an invalid math operation (NaN), discard it
            if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsInfinity(force.x) || float.IsInfinity(force.y))
                continue;

            if (force.sqrMagnitude > maxForcePerSource * maxForcePerSource)
                force = force.normalized * maxForcePerSource;

            totalForce += force;
        }

        rb.AddForce(totalForce, ForceMode2D.Force);

        // If physics forces punch the object past the hard speed speed limit, clamp it instantly
        if (rb.velocity.sqrMagnitude > maxVelocityMagnitude * maxVelocityMagnitude)
        {
            rb.velocity = rb.velocity.normalized * maxVelocityMagnitude;
        }
    }
}