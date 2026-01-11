using UnityEngine;

public class ObstacleHealth2D : MonoBehaviour, IDamageable
{
    [Header("On Death")]
    public GameObject destroyVfx;
    public float explosionDuration = 0.4f;

    private float currentHP;
    private ObstacleData cachedData;
    private bool isInitialized = false;
    public Animator anim;

    // 由 Core 調用，傳入 Data
    public void Initialize(ObstacleData data)
    {
        cachedData = data;
        currentHP = data.maxHP;
        isInitialized = true;
        
    }

    public void TakeDamage(float amount, Vector2 hitPoint, GameObject instigator)
    {
        // 如果 Data 設定 maxHP <= 0 或是根本還沒初始化完成，就不受傷
        if (!isInitialized || cachedData.maxHP <= 0) return;

        currentHP -= amount;

        // 播放受傷音效
        if (cachedData.hitSfx != null && AudioManager_2D.Instance != null)
            AudioManager_2D.Instance.PlayGameplaySFX(cachedData.hitSfx);

        if (currentHP <= 0f) Die(hitPoint, instigator);
    }

    void Die(Vector2 hitPoint, GameObject instigator)
    {
       
        if (anim != null)
        {
            anim.SetTrigger("Explode"); // 對應你在 Animator 設定的名字
        }
       
        // 播放破壞音效
        if (cachedData != null && cachedData.destroySfx != null && AudioManager_2D.Instance != null)
            AudioManager_2D.Instance.PlayGameplaySFX(cachedData.destroySfx);

        // 處理爆炸 logic (如果有 ExplodeOnDeath2D 腳本)
        var explode = GetComponent<ExplodeOnDeath2D>();
        if (explode != null)
        {
            explode.lastInstigator = instigator;
            explode.Explode();
        }

        if (destroyVfx != null)
            Instantiate(destroyVfx, hitPoint, Quaternion.identity);


        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 0.8f);
    }
}
