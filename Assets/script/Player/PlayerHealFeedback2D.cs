using UnityEngine;
using System.Collections;

public class PlayerHealFeedback2D : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth health;
    public SpriteRenderer spriteRenderer;
    public AudioClip healSfx;            // optional

    [Header("Flash")]
    public float flashDuration = 0.08f;
    public Color healColor = new Color(0.4f, 1f, 0.4f, 1f);

    Color originalColor;
    Coroutine flashCo;

    void Awake()
    {
        if (health == null) health = GetComponent<PlayerHealth>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void OnEnable()
    {
        if (health != null)
            health.OnHealed += HandleHealed;
    }

    void OnDisable()
    {
        if (health != null)
            health.OnHealed -= HandleHealed;
    }

    void HandleHealed(float healedAmount)
    {
        // healedAmount 一定 > 0（PlayerHealth.Heal 裡已保證）
        if (spriteRenderer != null)
        {
            if (flashCo != null) StopCoroutine(flashCo);
            flashCo = StartCoroutine(FlashHeal());
        }

        // 音效走你專案的 AudioManager_2D（不要在 Zone 播）
        if (healSfx != null && AudioManager_2D.Instance != null)
            AudioManager_2D.Instance.PlayGameplaySFX(healSfx);
    }

    IEnumerator FlashHeal()
    {
        // 先閃綠
        spriteRenderer.color = healColor;
        yield return new WaitForSeconds(flashDuration);

        // 還原
        spriteRenderer.color = originalColor;
        flashCo = null;
    }
}
