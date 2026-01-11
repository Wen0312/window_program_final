using UnityEngine;

public class TopDownPlayerMove2D : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip footstepClip;        // 腳步聲素材
    public float footstepInterval = 0.4f; // 兩次腳步聲的間隔時間（越小越快）
    private float footstepTimer;

    [Header("Move")]
    public float moveSpeed = 6f;

    [Header("ADS")]
    [Range(0.1f, 1f)]
    public float adsSpeedMultiplier = 0.5f; // ADS 時移動減速倍率（角色預設）

    public Animator anim;

    Rigidbody2D rb;

    // 由外部（PlayerInputMode2D）餵入的移動向量
    public Vector2 moveInput;

    bool isADS = false;

    // ===== ADS Move Override（由 WeaponData 暫態覆寫）=====
    float adsSpeedMultiplierOverride = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // 平滑
    }

    void Update()
    {
        // 偵測是否正在移動（維持你原本腳步聲邏輯）
        if (moveInput.sqrMagnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0)
            {
                PlayFootstepSound();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            // 沒移動時重設計時器
            footstepTimer = 0;
        }
    }

    void FixedUpdate()
    {
        float speed = moveSpeed;

        if (isADS)
        {
            // ADS 時：若有武器覆寫，優先用；否則用角色預設
            float mul = (adsSpeedMultiplierOverride > 0f)
                ? adsSpeedMultiplierOverride
                : adsSpeedMultiplier;

            speed *= mul;
        }

        rb.MovePosition(
            rb.position + moveInput * speed * Time.fixedDeltaTime
        );
    }

    // =========================
    // Public API (Input comes from outside)
    // =========================

    /// <summary>
    /// Set movement input vector (normalized or raw).
    /// This should be called by PlayerInputMode2D.
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    /// <summary>
    /// Set ADS state (called by PlayerInputMode2D).
    /// </summary>
    public void SetADS_Public(bool ads)
    {
        isADS = ads;
    }

    /// <summary>
    /// Set ADS speed multiplier override (weapon-based, transient).
    /// value > 0  : override
    /// value <= 0 : clear override, fallback to character default
    /// </summary>
    public void SetADSSpeedMultiplier_Public(float value)
    {
        adsSpeedMultiplierOverride = value;
    }

    void PlayFootstepSound()
    {
        if (AudioManager_2D.Instance != null && footstepClip != null)
        {
            AudioManager_2D.Instance.PlayFootstep(footstepClip);
        }
    }

    public Vector2 GetCurrentInput()
    {
        // 回傳 moveInput 給外部視覺腳本讀取
        return moveInput;
    }
}
