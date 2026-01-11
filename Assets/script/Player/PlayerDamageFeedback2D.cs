using UnityEngine;
using System.Collections;

public class PlayerDamageFeedback2D : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth health;
    public SpriteRenderer spriteRenderer;
    public AudioSource audioSource;     // optional
    public AudioClip hitSfx;            // optional

    [Header("Flash")]
    public float flashDuration = 0.08f;

    Color originalColor;
    Coroutine flashCo;

    void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        health = GetComponentInParent<PlayerHealth>() ?? GetComponent<PlayerHealth>();
    }

    void Awake()
    {
        //保險：沒拖也能自動抓
        if (health == null)
            health = GetComponentInParent<PlayerHealth>() ?? GetComponent<PlayerHealth>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void OnEnable()
    {
        if (health != null)
            health.OnDamaged += HandleDamaged; // 假設 PlayerHealth.OnDamaged 是 Action<float>
    }

    void OnDisable()
    {
        if (health != null)
            health.OnDamaged -= HandleDamaged;
    }

    void HandleDamaged(float dmg)
    {
        // 1) 反白
        if (spriteRenderer != null)
        {
            if (flashCo != null) StopCoroutine(flashCo);
            flashCo = StartCoroutine(FlashRed());
        }

        // 2) 音效（可選）
        if (audioSource != null && hitSfx != null)
            audioSource.PlayOneShot(hitSfx);
    }

    IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        flashCo = null;
    }
}
