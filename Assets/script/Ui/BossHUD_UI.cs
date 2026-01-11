using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BossHUD_UI
/// Boss 出生後在 HUD 最上方顯示：名字 / 血條 / 階段(Phase)
/// - 不改 EnemyHealth
/// - 透過 BossInfo2D 找到 Boss
/// - 支援 Shield/Shell Phase 視覺（殼 / 發光 / 顏色）
/// </summary>
public class BossHUD_UI : MonoBehaviour
{
    [Header("Root")]
    public GameObject root;                 // 整組 Boss HUD

    [Header("Text")]
    public TextMeshProUGUI bossNameText;    // Boss 名字
    public TextMeshProUGUI bossPhaseText;   // Phase / 狀態文字

    [Header("HP Bar")]
    public Image bossHpFillImage;           // HP Fill（Image / Filled）
    public GameObject shieldOverlay;        // 殼 Overlay（整組 UI 物件）

    [Header("PhaseColors")]
    public Color normalHPColor = Color.red;
    public Color shieldHPColor = new Color(0.3f, 0.7f, 1f);

    [Header("Phase 3 Flash")]
    public bool phase3FlashEnabled = true;
    public Color phase3FlashColor = new Color(1f, 0.3f, 0.3f, 1f); // （偏紅，可在 Inspector 調）
    public float phase3FlashSpeed = 6f; // （數字越大閃越快）

    [Header("Phase 3 SFX (optional)")]
    public AudioClip phase3EnterSfx;        // 進入 Phase3 時播一次（可拖 boss scream）
    public bool phase3EnterSfxOnce = true;  // 是否只播一次

    Coroutine phase3FlashCo;
    bool isPhase3Flashing = false;
    bool wasPhase3 = false;                // 用來判斷「剛進入 Phase3」

    [Header("Auto Find Boss")]
    public bool autoFindBoss = true;
    public float autoFindInterval = 0.25f;

    EnemyHealth bossHealth;
    BossInfo2D bossInfo;
    BossPhaseState2D bossPhase;

    float nextFindTime;

    [Header("Phase 3 Flicker (On/Off)")]
    public bool phase3FlickerEnabled = true;
    public float phase3FlickerInterval = 0.12f; // 閃爍速度
    public bool flickerHPBarOnly = true;        // true=只閃血條, false=整個 HUD

    [Header("Phase 3 Screen Shake")]
    public bool phase3ShakeEnabled = true;
    public float phase3ShakeDuration = 0.25f;
    public float phase3ShakeAmplitude = 0.18f;
    public float phase3ShakeFrequency = 35f;

    Coroutine phase3FlickerCo;



    void Start()
    {
        SetVisible(false);
    }

    void Update()
    {
        if (!autoFindBoss) return;

        if (bossHealth == null && Time.time >= nextFindTime)
        {
            nextFindTime = Time.time + autoFindInterval;
            TryBindFirstBoss();
        }
    }

    void OnDestroy()
    {
        UnbindBoss();
    }

    // =========================
    // Boss Binding
    // =========================

    void TryBindFirstBoss()
    {
        var infos = FindObjectsByType<BossInfo2D>(FindObjectsSortMode.None);
        if (infos == null || infos.Length == 0) return;

        for (int i = 0; i < infos.Length; i++)
        {
            var info = infos[i];
            if (info == null) continue;
            if (!info.gameObject.activeInHierarchy) continue;

            var health = info.GetComponent<EnemyHealth>();
            if (health == null) health = info.GetComponentInParent<EnemyHealth>();
            if (health == null) health = info.GetComponentInChildren<EnemyHealth>();
            if (health == null) continue;

            BindBoss(info, health);
            return;
        }
    }

    void BindBoss(BossInfo2D info, EnemyHealth health)
    {
        UnbindBoss();

        bossInfo = info;
        bossHealth = health;

        bossPhase = null;
        if (bossInfo != null) bossPhase = bossInfo.GetComponent<BossPhaseState2D>();
        if (bossPhase == null && bossHealth != null) bossPhase = bossHealth.GetComponent<BossPhaseState2D>();
        if (bossPhase == null && bossInfo != null) bossPhase = bossInfo.GetComponentInParent<BossPhaseState2D>();
        if (bossPhase == null && bossInfo != null) bossPhase = bossInfo.GetComponentInChildren<BossPhaseState2D>();
        if (bossPhase == null && bossHealth != null) bossPhase = bossHealth.GetComponentInParent<BossPhaseState2D>();

        SetVisible(true);

        // Boss 名字
        if (bossNameText != null)
            bossNameText.text = string.IsNullOrEmpty(info.bossName) ? "BOSS" : info.bossName;

        // HP：初始化（EnemyHealth 沒提供 currentHP 公開值）
        UpdateHPBar(bossHealth.maxHP, bossHealth.maxHP);

        // 訂閱
        bossHealth.OnHPChanged += UpdateHPBar;
        bossHealth.OnDead += HandleBossDead;

        // Phase
        wasPhase3 = false; // 重綁時重置
        UpdatePhaseVisual();
        if (bossPhase != null)
            bossPhase.OnPhaseChanged += HandlePhaseChanged;
    }

    void UnbindBoss()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHPChanged -= UpdateHPBar;
            bossHealth.OnDead -= HandleBossDead;
        }

        if (bossPhase != null)
        {
            bossPhase.OnPhaseChanged -= HandlePhaseChanged;
        }

        StopPhase3Flash();

        bossHealth = null;
        bossInfo = null;
        bossPhase = null;

        wasPhase3 = false;

        SetVisible(false);
    }

    // =========================
    // Event Handlers
    // =========================

    void HandleBossDead()
    {
        UnbindBoss();
    }

    void HandlePhaseChanged(string _)
    {
        UpdatePhaseVisual();
    }

    // =========================
    // UI Update
    // =========================

    void UpdateHPBar(float currentHP, float maxHP)
    {
        if (bossHpFillImage == null) return;

        float fill = (maxHP <= 0.0001f) ? 0f : Mathf.Clamp01(currentHP / maxHP);
        bossHpFillImage.fillAmount = fill;
    }

    void UpdatePhaseVisual()
    {
        // Phase 文字（保留你原本的寫法）
        string phaseRaw = (bossPhase != null ? bossPhase.currentPhase : "");
        string phase = (phaseRaw ?? "").Trim();

        if (bossPhaseText != null && bossPhase != null)
            bossPhaseText.text = phaseRaw;

        // 更穩的判斷（不被字串格式雷到）
        bool isShield = string.Equals(phase, "Shield", StringComparison.OrdinalIgnoreCase);

        bool isPhase3 =
            string.Equals(phase, "Phase 3", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(phase, "Phase3", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(phase, "Berserker Mode", StringComparison.OrdinalIgnoreCase) ||
            phase.IndexOf("berserk", StringComparison.OrdinalIgnoreCase) >= 0 ||
            (phase.IndexOf("phase", StringComparison.OrdinalIgnoreCase) >= 0 &&
             phase.IndexOf("3", StringComparison.OrdinalIgnoreCase) >= 0);

        // Shield Overlay
        if (shieldOverlay != null)
            shieldOverlay.SetActive(isShield);

        // =========================
        // Phase 3 Enter SFX（只在「剛進入」播）
        // =========================
        if (isPhase3 && !wasPhase3)
        {
            if (phase3EnterSfx != null)
            {
                // 參考 SummonOnHPThresholdAbility2D：統一走 AudioManager_2D
                AudioManager_2D.Instance.PlayGameplaySFX(phase3EnterSfx);
            }

            if (phase3EnterSfxOnce)
                wasPhase3 = true;
        }
        else if (!isPhase3)
        {
            // 離開 Phase3 就允許下次再進入時播（如果你想只播一次，全程都不重置也行）
            wasPhase3 = false;
        }

        if (bossHpFillImage == null) return;

        // HP Bar 顏色 / Phase3 閃爍
        if (isShield)
        {
            if (isPhase3Flashing) StopPhase3Flash();
            bossHpFillImage.color = shieldHPColor;
            return;
        }

        if (isPhase3)
        {
            if (!isPhase3Flashing) StartPhase3Flash(normalHPColor);
            return;
        }
        // =========================
        // Phase 3 Enter / Exit（只觸發一次）
        // =========================
        if (isPhase3 && !wasPhase3)
        {
            // 進入 Phase 3
            StartPhase3Flicker();

            if (phase3ShakeEnabled && CameraShake2D.Instance != null)
            {
                CameraShake2D.Instance.Shake(
                    phase3ShakeDuration,
                    phase3ShakeAmplitude,
                    phase3ShakeFrequency
                );
            }
        }
        else if (!isPhase3 && wasPhase3)
        {
            // 離開 Phase 3
            StopPhase3Flicker();
        }

        wasPhase3 = isPhase3;

        if (isPhase3Flashing) StopPhase3Flash();
        bossHpFillImage.color = normalHPColor;
    }

    void SetVisible(bool visible)
    {
        if (root != null)
            root.SetActive(visible);
    }

    void StartPhase3Flash(Color baseColor)
    {
        if (!phase3FlashEnabled) return;
        if (bossHpFillImage == null) return;
        if (isPhase3Flashing) return;

        isPhase3Flashing = true;

        if (phase3FlashCo != null)
            StopCoroutine(phase3FlashCo);

        phase3FlashCo = StartCoroutine(Phase3FlashRoutine(baseColor));
    }

    void StopPhase3Flash()
    {
        isPhase3Flashing = false;

        if (phase3FlashCo != null)
        {
            StopCoroutine(phase3FlashCo);
            phase3FlashCo = null;
        }

        if (bossHpFillImage != null)
            bossHpFillImage.color = normalHPColor;
    }

    System.Collections.IEnumerator Phase3FlashRoutine(Color baseColor)
    {
        while (isPhase3Flashing)
        {
            float t = Mathf.PingPong(Time.unscaledTime * phase3FlashSpeed, 1f);
            bossHpFillImage.color = Color.Lerp(baseColor, phase3FlashColor, t);
            yield return null;
        }
    }
    void StartPhase3Flicker()
    {
        if (!phase3FlickerEnabled) return;
        if (phase3FlickerCo != null) return;

        phase3FlickerCo = StartCoroutine(Phase3FlickerRoutine());
    }

    void StopPhase3Flicker()
    {
        if (phase3FlickerCo != null)
        {
            StopCoroutine(phase3FlickerCo);
            phase3FlickerCo = null;
        }

        // 保證最後顯示
        if (flickerHPBarOnly)
        {
            if (bossHpFillImage != null)
                bossHpFillImage.enabled = true;
        }
        else
        {
            if (root != null)
                root.SetActive(true);
        }
    }

    System.Collections.IEnumerator Phase3FlickerRoutine()
    {
        while (true)
        {
            if (flickerHPBarOnly)
            {
                if (bossHpFillImage != null)
                    bossHpFillImage.enabled = !bossHpFillImage.enabled;
            }
            else
            {
                if (root != null)
                    root.SetActive(!root.activeSelf);
            }

            yield return new WaitForSecondsRealtime(
                Mathf.Max(0.02f, phase3FlickerInterval)
            );
        }
    }

}
