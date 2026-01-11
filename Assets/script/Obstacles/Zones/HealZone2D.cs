using UnityEngine;

public class HealZone2D : MonoBehaviour
{
    public StatusEffectData healEffect; // «ü¦V Heal ªº SO
    public LayerMask affectMask;        // Player / Enemy

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & affectMask.value) == 0) return;

        var r = other.GetComponentInParent<StatusReceiver2D>();
        if (r != null) r.Apply(healEffect, gameObject);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var r = other.GetComponentInParent<StatusReceiver2D>();
        if (r != null) r.Remove(healEffect);
    }
}
