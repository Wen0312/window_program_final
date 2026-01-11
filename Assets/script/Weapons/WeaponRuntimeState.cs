using UnityEngine;

/// <summary>
/// WeaponRuntimeState
/// 
/// 每一把武器各自擁有的一份「Runtime 狀態」
/// - 不處理 Input
/// - 不處理射擊邏輯
/// - 不依賴 MonoBehaviour
/// - 不存進 ScriptableObject
/// 
/// 目的：
/// 將「武器規格（WeaponData）」與「即時狀態（Ammo / Reload）」分離
/// </summary>
public class WeaponRuntimeState
{
    // =========================
    // Ammo
    // =========================

    public int currentAmmo;

    // =========================
    // Reload Runtime
    // =========================

    public bool isReloading;
    public float reloadEndTime;

    // =========================
    // Constructor
    // =========================

    /// <summary>
    /// 建立一把武器的初始 runtime 狀態
    /// 注意：初始化策略（是否滿彈）由呼叫端決定
    /// </summary>
    public WeaponRuntimeState(int initialAmmo)
    {
        currentAmmo = initialAmmo;
        isReloading = false;
        reloadEndTime = 0f;
    }

    /// <summary>
    /// 重置 Reload 狀態（例如切武器、強制中斷時使用）
    /// </summary>
    public void ResetReload()
    {
        isReloading = false;
        reloadEndTime = 0f;
    }
}
