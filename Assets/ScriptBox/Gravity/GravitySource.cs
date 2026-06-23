using System.Collections.Generic;
using UnityEngine;

public abstract class GravitySource : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private Planner.PlanetType planetType = Planner.PlanetType.Circle;
    public Planner.PlanetType Type => planetType;

    public static readonly List<GravitySource> ActiveSources = new List<GravitySource>();

    [Header("Core Gravity")]
    [SerializeField] private float gravityStrength = 25f;
    [SerializeField] private float influenceRadius = 12f;
    [SerializeField] private float softening = 0.15f;

    [Header("Slingshot Tuning")]
    [SerializeField, Range(0f, 1f)] private float tangentialAssist = 0.12f;
    [SerializeField] private float tangentialAssistRadius = 7f;
    [SerializeField] private float escapeAssistRadius = 1.75f;
    [SerializeField] private float escapeAssistStrength = 10f;

    public float GravityStrength => gravityStrength;
    public float InfluenceRadius => influenceRadius;
    public float TangentialAssist => tangentialAssist;
    public float TangentialAssistRadius => tangentialAssistRadius;
    public float EscapeAssistRadius => escapeAssistRadius;
    public float EscapeAssistStrength => escapeAssistStrength;
    public float Softening => softening;

    protected virtual void OnEnable()
    {
        if (!ActiveSources.Contains(this))
            ActiveSources.Add(this);
    }

    protected virtual void OnDisable()
    {
        ActiveSources.Remove(this);
    }

    public Vector2 GetForceOn(GravityBody body)
    {
        if (body == null || body.Rigidbody == null)
            return Vector2.zero;

        Vector2 bodyPos = body.Rigidbody.position;
        Vector2 sourcePos = GetGravityOrigin(bodyPos);

        Vector2 toSource = sourcePos - bodyPos;
        float distance = toSource.magnitude;

        // check
        if (distance > influenceRadius || distance <= Mathf.Epsilon)
            return Vector2.zero;

        Vector2 inwardDir = GetGravityDirection(bodyPos, body);

        // calculations
        float range01 = Mathf.Clamp01(distance / influenceRadius);
        float rangeFalloff = 1f - range01;
        float inverseSoftened = 1f / (1f + softening * distance * distance);

        float radialStrength = gravityStrength * rangeFalloff * rangeFalloff * inverseSoftened;
        Vector2 force = inwardDir * radialStrength;

        if (tangentialAssist > 0f && distance <= tangentialAssistRadius)
        {
            Vector2 tangent = new Vector2(-inwardDir.y, inwardDir.x);

            // Match structural trajectory sign with incoming velocity profile
            if (Vector2.Dot(body.Velocity, tangent) < 0f)
                tangent = -tangent;

            float assist01 = Mathf.Clamp01(1f - (distance / tangentialAssistRadius));
            float assistStrength = gravityStrength * tangentialAssist * assist01 * assist01;

            force += tangent * assistStrength;
        }

        if (distance < escapeAssistRadius && body.Velocity.magnitude < body.MinEscapeSpeed)
        {
            float speed01 = Mathf.Clamp01(body.Velocity.magnitude / Mathf.Max(body.MinEscapeSpeed, 0.01f));
            float escapeBoost = (1f - speed01) * escapeAssistStrength;
            force += -inwardDir * escapeBoost;
        }

        return force;
    }

    protected virtual Vector2 GetGravityOrigin(Vector2 worldPosition)
    {
        return transform.position;
    }

    protected abstract Vector2 GetGravityDirection(Vector2 worldPosition, GravityBody body);

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, influenceRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tangentialAssistRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, escapeAssistRadius);
    }
#endif
}
