using UnityEngine;

public static class GravitySystem
{
    public static bool GravityEnabled { get; private set; } = false;

    public static void SetGravityEnabled(bool enabled)
    {
        GravityEnabled = enabled;
    }
}