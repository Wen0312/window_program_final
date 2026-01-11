using System.Collections.Generic;
using UnityEngine;

public class SlowZone2D_Legacy : MonoBehaviour
{
    [Header("Slow")]
    [Tooltip("0.5 = 50% speed")]
    [Range(0.05f, 1f)]
    public float slowMultiplier = 0.5f;

    // 追蹤目前在區域內的對象（避免重複 Enter/Exit 出 bug）
    readonly HashSet<ISlowable2D> inside = new HashSet<ISlowable2D>();

    void OnTriggerEnter2D(Collider2D other)
    {
        var slowable = other.GetComponentInParent<ISlowable2D>();
        if (slowable == null) return;

        if (inside.Add(slowable))
        {
            slowable.SetSlowMultiplier(slowMultiplier);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var slowable = other.GetComponentInParent<ISlowable2D>();
        if (slowable == null) return;

        if (inside.Remove(slowable))
        {
            slowable.ClearSlowMultiplier();
        }
    }

    void OnDisable()
    {
        // 如果鐵絲網被打爆、Disable/Destroy，確保把人恢復速度
        foreach (var s in inside)
            s.ClearSlowMultiplier();

        inside.Clear();
    }
}
