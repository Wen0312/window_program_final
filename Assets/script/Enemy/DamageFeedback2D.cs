using UnityEngine;
using TMPro;
using System.Collections;

public class DamageFeedback2D : MonoBehaviour
{
    [Header("References")]
    public EnemyHealth health;                 // «ü¦V¦P¤@°¦¼Ä¤Hªº EnemyHealth
    public SpriteRenderer spriteRenderer;      // ­n°{¥Õªº¨º±i sprite¡]EnemyVisual ¨º­Ó¡^
    public AudioSource audioSource;            // ¥i¿ï
    public AudioClip hitSfx;                   // ¥i¿ï

    [Header("Flash")]
    public float flashDuration = 0.08f;

    [Header("Damage Popup (Optional)")]
    public bool showPopup = false;
    public GameObject popupPrefab;           // ©ñ¤@­Ó TMP 3D/TextMeshPro prefab
    public Vector3 popupOffset = new Vector3(0, 0.6f, 0);

    Color originalColor;
    Coroutine flashCo;

    void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        health = GetComponentInParent<EnemyHealth>() ?? GetComponent<EnemyHealth>();
    }

    void Awake()
    {
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void OnEnable()
    {
        if (health != null)
            health.OnDamaged += HandleDamaged;
    }

    void OnDisable()
    {
        if (health != null)
            health.OnDamaged -= HandleDamaged;
    }

    void HandleDamaged(float dmg, Vector2 hitPoint, GameObject instigator)
    {
        // 1) °{¥Õ
        if (spriteRenderer != null)
        {
            if (flashCo != null) StopCoroutine(flashCo);
            flashCo = StartCoroutine(FlashRed());
        }

        // 2) ­µ®Ä¡]¥i¿ï¡^
        if (audioSource != null && hitSfx != null)
            audioSource.PlayOneShot(hitSfx);

        // 3) ¸õ¼Æ¦r¡]¥i¿ï¡^
        if (showPopup && popupPrefab != null)
        {
            Vector3 basePos = (health != null ? health.transform.position : transform.position);
            Vector3 pos = basePos + popupOffset;
            pos.z = -1f; //«OÃÒ¦b¬Û¾÷«e­±¤@ÂI

            GameObject go = Instantiate(popupPrefab, pos, Quaternion.identity);
            var tmp = go.GetComponent<TextMeshPro>();
            if (tmp != null) tmp.text = Mathf.CeilToInt(dmg).ToString();

            Destroy(go, 0.6f);
        }

        if (hitSfx != null && AudioManager_2D.Instance != null)
        {
            // 這裡不需要自己的 audioSource，而是叫 AudioManager 幫忙播
            AudioManager_2D.Instance.PlayGameplaySFX(hitSfx);
        }
    }

    IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        flashCo = null;
    }
}
