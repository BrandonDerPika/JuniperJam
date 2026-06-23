using UnityEngine;

public class Circle : GravitySource
{
    protected override Vector2 GetGravityDirection(Vector2 worldPosition, GravityBody body)
    {
        Vector2 toCenter = (Vector2)transform.position - worldPosition;

        // Guard against division by zero errors when spatial vectors completely overlap
        if (toCenter.sqrMagnitude <= Mathf.Epsilon)
            return Vector2.up;

        return toCenter.normalized;
    }
}
