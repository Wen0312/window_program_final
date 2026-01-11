using UnityEngine;

public class PlayerWeaponInput : MonoBehaviour
{
    [Header("Refs")]
    public WeaponController2D weaponController;
    public WeaponData[] weapons;
    private PlayerShootSFXBridge_2D shootSFXBridge;

    int currentIndex = 0;
    
    void Start()
    {
        if (weaponController != null && weapons != null && weapons.Length > 0)
            weaponController.SetWeapon(weapons[currentIndex]);
    }
    void Awake()
    {
        shootSFXBridge = GetComponent<PlayerShootSFXBridge_2D>();
    }

    // =========================
    // Public API for InputMode
    // =========================
    public void SwitchWeapon_Public(int delta)
    {
        SwitchWeapon(delta);
    }


    void SwitchWeapon(int delta)
    {
        if (weapons == null || weapons.Length == 0) return;

        currentIndex = (currentIndex + delta + weapons.Length) % weapons.Length;
        weaponController.SetWeapon(weapons[currentIndex]);
    }
}
