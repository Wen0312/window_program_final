using System;
using System.Collections; // 有用到 Coroutine（保留）
using System.Collections.Generic;
using UnityEngine;

public class WeaponController2D : MonoBehaviour
{
    public WeaponData currentWeapon;
    public Transform firePoint;

    // =========================
    // SFX Bridge Events（只負責事件，不處理播放）
    // =========================
    public event Action<AudioClip> OnShootSfx;
    public event Action<AudioClip> OnReloadSfx;
    public event Action<AudioClip> OnEquipSfx;

    // =========================
    // Visual Bridge Events（只通知，不處理視覺）
    // WeaponVisual2D 會訂閱這個事件來做後座力效果
    // strength：可用來調整不同武器的後座強度（預設 1）
    // =========================
    public event Action<float> OnShotSuccess_Recoil;

    // 射擊時間 gate（統一管理射速 / 換彈 / 裝備延遲）
    float nextAllowedFireTime;

    // =========================
    // Reload runtime（Phase 2：改由 RuntimeState 管理）
    // =========================

    // 每把武器各自的 runtime 狀態（Ammo / Reload）
    Dictionary<WeaponData, WeaponRuntimeState> runtimeStates =
        new Dictionary<WeaponData, WeaponRuntimeState>();

    // 目前正在使用的 runtime state
    WeaponRuntimeState currentState;

    // =========================
    // ADS runtime（只存狀態，不讀 Input）
    // =========================
    bool isADS = false;

    // 提供給 Visual / Camera 查詢（不讀 Input）
    public bool IsADS_Public() => isADS;

    void Update()
    {
        if (currentState == null) return;

        if (currentState.isReloading && Time.time >= currentState.reloadEndTime)
        {
            FinishReload();
        }
    }


    public void SetWeapon(WeaponData weapon)
    {
        currentWeapon = weapon;

        // =========================
        // Phase 3：切換 RuntimeState
        // =========================

        if (currentWeapon != null)
        {
            // 取得或建立這把武器的 runtime state
            if (!runtimeStates.TryGetValue(currentWeapon, out currentState))
            {
                // 第一次拿到這把武器 → 初始化滿彈
                int initialAmmo = Mathf.Max(1, currentWeapon.magSize);
                currentState = new WeaponRuntimeState(initialAmmo);
                runtimeStates.Add(currentWeapon, currentState);
            }

            // 切槍時：中斷換彈（但不補彈）
            currentState.ResetReload();

            // 射擊 gate（裝備延遲）
            nextAllowedFireTime = Time.time + Mathf.Max(0f, currentWeapon.equipDelay);

            // 裝備音效
            if (AudioManager_2D.Instance != null && currentWeapon.equipSfx != null)
            {
                AudioManager_2D.Instance.PlayGameplaySFX(currentWeapon.equipSfx);
            }
            OnEquipSfx?.Invoke(currentWeapon.equipSfx);
        }
        else
        {
            currentState = null;
            nextAllowedFireTime = Time.time;
        }
    }


    // 由 PlayerInputMode2D 統一呼叫
    // Combat 模式才會啟用，Build 模式不會呼叫
    public void SetADS_Public(bool ads)
    {
        // 偵測狀態改變：由未瞄準 → 進入 ADS
        if (ads && !isADS)
        {
            // 播放 ADS 音效
            if (currentWeapon != null && currentWeapon.adsSfx != null)
            {
                if (AudioManager_2D.Instance != null)
                {
                    AudioManager_2D.Instance.PlayGameplaySFX(currentWeapon.adsSfx);
                }
            }
        }

        // 更新 ADS 狀態
        isADS = ads;
    }

    public bool TryShoot(Vector2 aimDir)
    {
        if (currentWeapon == null) return false;
        if (firePoint == null) return false;
        if (aimDir.sqrMagnitude < 0.001f) return false;
        if (currentState == null) return false;

        // 換彈中不可射擊
        if (currentState.isReloading) return false;

        // 彈匣空了
        if (currentState.currentAmmo <= 0)
        {
            if (currentWeapon.autoReloadOnEmpty)
            {
                TryReload();
            }
            return false;
        }

        // 射速 / 裝備延遲 gate
        if (Time.time < nextAllowedFireTime) return false;

        // 消耗彈藥
        currentState.currentAmmo = Mathf.Max(0, currentState.currentAmmo - 1);
        nextAllowedFireTime = Time.time + Mathf.Max(0f, currentWeapon.fireCooldown);

        ShootInternal(aimDir.normalized);

        OnShotSuccess_Recoil?.Invoke(1f);

        if (currentWeapon.shootSfx != null)
        {
            if (AudioManager_2D.Instance != null)
            {
                AudioManager_2D.Instance.PlayGameplaySFX(currentWeapon.shootSfx);
            }
            OnShootSfx?.Invoke(currentWeapon.shootSfx);
        }

        return true;
    }


    public bool TryReload()
    {
        if (currentWeapon == null) return false;
        if (currentState == null) return false;
        if (currentState.isReloading) return false;

        int mag = Mathf.Max(1, currentWeapon.magSize);
        if (currentState.currentAmmo >= mag) return false;

        float t = Mathf.Max(0f, currentWeapon.reloadTime);
        if (t <= 0f)
        {
            currentState.currentAmmo = mag;
            return true;
        }

        currentState.isReloading = true;
        currentState.reloadEndTime = Time.time + t;

        nextAllowedFireTime = Mathf.Max(nextAllowedFireTime, currentState.reloadEndTime);

        if (currentWeapon.reloadSfx != null)
        {
            if (AudioManager_2D.Instance != null)
            {
                AudioManager_2D.Instance.PlayGameplaySFX(currentWeapon.reloadSfx);
            }
            OnReloadSfx?.Invoke(currentWeapon.reloadSfx);
        }

        return true;
    }


    void FinishReload()
    {
        if (currentState == null) return;

        currentState.isReloading = false;
        currentState.reloadEndTime = 0f;
        currentState.currentAmmo = Mathf.Max(1, currentWeapon.magSize);
    }


    void ShootInternal(Vector2 baseDir)
    {
        int count = Mathf.Max(1, currentWeapon.pellets);

        // 固定擴散（spreadAngle）
        float step = currentWeapon.spreadAngle;
        float start = -step * (count - 1) * 0.5f;

        // 隨機擴散（randomSpreadAngle），ADS 時倍率調整
        float randMax = Mathf.Max(0f, currentWeapon.randomSpreadAngle);
        if (isADS)
        {
            randMax *= Mathf.Max(0f, currentWeapon.adsSpreadMultiplier);
        }

        for (int i = 0; i < count; i++)
        {
            float angle = start + step * i;

            // 每顆子彈加入隨機擴散
            if (randMax > 0f)
            {
                angle += UnityEngine.Random.Range(-randMax, randMax);
            }

            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;

            Projectile2D proj = Instantiate(
                currentWeapon.projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

            proj.Launch(dir, currentWeapon.projectileSpeed, currentWeapon.damage, gameObject);
        }
    }

    // =========================
    // UI / Debug 查詢用（不影響核心邏輯）
    // =========================
    public int GetCurrentAmmo_Public()
    {
        return currentState != null ? currentState.currentAmmo : 0;
    }

    public bool IsReloading_Public()
    {
        return currentState != null && currentState.isReloading;
    }

}
