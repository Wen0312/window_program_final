using UnityEngine;

public class PlayerAim2D : MonoBehaviour
{
    public Vector2 AimDir { get; private set; } = Vector2.right;

    // =========================
    // Public API (Input comes from outside)
    // =========================

    /// <summary>
    /// Set aim target in world space.
    /// This should be called by PlayerInputMode2D.
    /// </summary>
    public void SetAimWorldPosition(Vector2 worldPos)
    {
        Vector2 dir = worldPos - (Vector2)transform.position;
        if (dir.sqrMagnitude > 0.0001f)
            AimDir = dir.normalized;
    }
}
