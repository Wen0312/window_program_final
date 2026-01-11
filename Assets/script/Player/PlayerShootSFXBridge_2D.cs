using UnityEngine;

public class PlayerShootSFXBridge_2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private WeaponController2D weaponController;
    [SerializeField] private AudioManager_2D audioManager;

    private void Awake()
    {
        if (weaponController == null)
            weaponController = GetComponent<WeaponController2D>();

        if (audioManager == null)
            audioManager = AudioManager_2D.Instance;
    }

    public void PlayShootSFX()
    {
        if (weaponController == null) return;

        if (audioManager == null) audioManager = AudioManager_2D.Instance;
        if (audioManager == null) return;

        WeaponData weaponData = weaponController.currentWeapon;
        if (weaponData == null || weaponData.shootSfx == null) return;

        // 只傳入 clip 參數
        audioManager.PlayGameplaySFX(weaponData.shootSfx);
    }
}