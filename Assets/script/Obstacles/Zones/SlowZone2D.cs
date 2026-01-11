using UnityEngine;

public class SlowZone2D : MonoBehaviour
{
    public StatusEffectData slowEffect;   // 指向一個 Slow 的 SO
    public LayerMask affectMask;          // Player / Enemy

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & affectMask.value) == 0) return;

        var r = other.GetComponentInParent<StatusReceiver2D>();
        if (r != null) r.Apply(slowEffect, gameObject);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var r = other.GetComponentInParent<StatusReceiver2D>();
        if (r != null) r.Remove(slowEffect);
    }
}
