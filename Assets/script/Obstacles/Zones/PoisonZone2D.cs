using UnityEngine;

public class PoisonZone2D : MonoBehaviour
{
    public StatusEffectData poisonEffect; // «ü¦V Poison ªº SO
    public LayerMask affectMask;          // Player / Enemy

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & affectMask.value) == 0) return;

        var r = other.GetComponentInParent<StatusReceiver2D>();
        if (r != null) r.Apply(poisonEffect, gameObject);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var r = other.GetComponentInParent<StatusReceiver2D>();
        if (r != null) r.Remove(poisonEffect);
    }
}
