using UnityEngine;

/// <summary>
/// WeaponVisual2D
/// 專門處理「武器視覺跟著瞄準旋轉」，不處理攻擊、不讀 Input
/// - 資料來源：WeaponData（ScriptableObject）
/// - 方向來源：PlayerAim2D
/// - 生命週期：跟著 WeaponController2D.currentWeapon
/// </summary>
public class WeaponVisual2D : MonoBehaviour
{
    [Header("Refs")]
    public WeaponController2D weaponController;
    public PlayerAim2D aimer;

    [Header("Render")]
    public SpriteRenderer spriteRenderer;



    WeaponData currentWeapon;

    // 基準位置（來自 WeaponData.weaponSpriteLocalOffset）
    Vector3 baseLocalPos;

    // 後座偏移（WeaponVisual local space，套用到 Sprite localPosition）
    Vector3 recoilOffsetLocal;

    // 後座角度（會加在 Aim 旋轉上）
    float recoilRotZ;

    // Sprite 原始位置（local）
    Vector3 spriteBaseLocalPos;

    void Awake()
    {
        // 自動抓，省得你漏拖
        if (weaponController == null)
            weaponController = GetComponentInParent<WeaponController2D>();

        if (aimer == null)
            aimer = GetComponentInParent<PlayerAim2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (weaponController == null) return;
        if (weaponController.currentWeapon == null)
        {
            SetVisible(false);
            return;
        }

        if (currentWeapon != weaponController.currentWeapon)
        {
            ApplyWeaponVisual(weaponController.currentWeapon);
        }

        // 1) 先更新 recoil 數值
        UpdateRecoil(Time.deltaTime);

        // 2) 再做瞄準旋轉（把 recoilRotZ 加上去）
        UpdateAimRotation();

        // 3) 最後套用 Sprite 後退位移
        ApplyRecoilLocalPosition();
    }



    // =========================
    // Public API（給 WeaponController 在「成功開火」時呼叫）
    // =========================
    /// <summary>
    /// 開火成功後呼叫：觸發武器後座（只影響視覺，不影響準心/彈道）
    /// strength：可用來做霰彈/大槍後座更大（1 = 標準）
    /// </summary>
    // =========================
    // Public API（給 WeaponController 在「成功開火」時呼叫）
    // =========================
    public void KickRecoil_Public(float strength = 1f)
    {
        if (aimer == null) return;

        WeaponData w = weaponController != null
            ? weaponController.currentWeapon
            : currentWeapon;

        if (w == null) return;

        Vector2 aimDir = aimer.AimDir;
        if (aimDir.sqrMagnitude < 0.0001f) return;
        aimDir.Normalize();

        // ADS multiplier（只讀狀態）
        float adsMul = 1f;
        if (weaponController != null && weaponController.IsADS_Public())
            adsMul = Mathf.Max(0f, w.adsRecoilMultiplier);

        float s = Mathf.Max(0.01f, strength) * adsMul;

        // 世界方向（往反方向推）
        Vector3 worldKickDir = -(Vector3)aimDir;

        // 關鍵修正：方向轉成 WeaponVisual local
        Vector3 localKickDir = transform.InverseTransformDirection(worldKickDir);

        // 位移後座
        recoilOffsetLocal += localKickDir.normalized * (w.recoilKickDistance * s);
        float maxD = Mathf.Max(0.0001f, w.recoilMaxDistance);
        if (recoilOffsetLocal.magnitude > maxD)
            recoilOffsetLocal = recoilOffsetLocal.normalized * maxD;

        // 旋轉後座
        float sign = w.recoilRandomRotSign
            ? (Random.value < 0.5f ? -1f : 1f)
            : 1f;

        recoilRotZ += sign * (w.recoilKickRotationDeg * s);
    }





    // =========================
    // Core
    // =========================


    void ApplyWeaponVisual(WeaponData data)
    {
        currentWeapon = data;

        if (spriteRenderer == null)
            return;

        if (data.weaponSprite == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        spriteRenderer.sprite = data.weaponSprite;
        spriteRenderer.sortingOrder = data.weaponSortingOrder;

        // 記住基準位置（之後 recoil 都以這個為基準）
        spriteRenderer.transform.localPosition = data.weaponSpriteLocalOffset;
        spriteBaseLocalPos = spriteRenderer.transform.localPosition;
        // 套用本地縮放
        transform.localScale = new Vector3(
            data.weaponSpriteLocalScale.x,
            data.weaponSpriteLocalScale.y,
            1f
        );

        // 先回到基準（避免切槍殘留偏移）
        recoilOffsetLocal = Vector3.zero;
        recoilRotZ = 0f;
        // 初始角度（通常是讓 sprite 的「朝右」對齊 AimDir）
        transform.localRotation = Quaternion.Euler(0f, 0f, data.weaponSpriteAngleOffset);
    }

    void UpdateRecoil(float dt)
    {
        if (currentWeapon == null)
            return;

        recoilOffsetLocal = Vector3.Lerp(
            recoilOffsetLocal,
            Vector3.zero,
            1f - Mathf.Exp(-currentWeapon.recoilReturnSpeed * dt)
        );

        recoilRotZ = Mathf.Lerp(
            recoilRotZ,
            0f,
            1f - Mathf.Exp(-currentWeapon.recoilRotReturnSpeed * dt)
        );
    }
    void UpdateAimRotation()
    {
        if (aimer == null) return;
        if (currentWeapon == null) return;

        Vector2 aimDir = aimer.AimDir;
        if (aimDir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            angle + currentWeapon.weaponSpriteAngleOffset + recoilRotZ
        );

        if (currentWeapon.weaponFlipOnAimLeft)
        {
            Vector3 scale = transform.localScale;
            scale.y = Mathf.Abs(scale.y) * (aimDir.x < 0 ? -1 : 1);
            transform.localScale = scale;
        }
    }



    void ApplyRecoilLocalPosition()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.transform.localPosition =
            spriteBaseLocalPos + recoilOffsetLocal;
    }

    void SetVisible(bool visible)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;
    }
    void OnEnable()
    {
        if (weaponController != null)
            weaponController.OnShotSuccess_Recoil += KickRecoil_Public;
    }

    void OnDisable()
    {
        if (weaponController != null)
            weaponController.OnShotSuccess_Recoil -= KickRecoil_Public;
    }

}
