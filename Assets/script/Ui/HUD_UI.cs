using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD_UI : MonoBehaviour
{
    [Header("Health UI (Image Method)")]
    // 這裡我們不放 Slider，改放 Image
    public Image hpFillImage;        // 這是那張「紅色」會縮短的圖

    [Header("Weapon UI")]
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI ammoText;

    private PlayerHealth playerHealth;
    private WeaponController2D weaponController;

    void Start()
    {
        // 自動抓取
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        weaponController = FindFirstObjectByType<WeaponController2D>();

        if (playerHealth != null)
        {
            // 初始化顯示
            UpdateHPBar(playerHealth.maxHP, playerHealth.maxHP);
            // 訂閱事件
            playerHealth.OnHPChanged += UpdateHPBar;
        }
    }

    void Update()
    {
        // 更新子彈文字
        if (weaponController != null && weaponController.currentWeapon != null)
        {
            if (ammoText != null)
            {
                // 如果正在換彈，顯示 "Reloading..." 或單純數字都可以
                if (weaponController.IsReloading_Public())
                    ammoText.text = "Rel...";
                else
                    ammoText.text = weaponController.GetCurrentAmmo_Public().ToString();
            }

            if (weaponNameText != null)
                weaponNameText.text = weaponController.currentWeapon.weaponName;
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHPChanged -= UpdateHPBar;
        }
    }

    void UpdateHPBar(float currentHP, float maxHP)
    {
        // 計算血量百分比 (0 ~ 1)
        float fillAmount = Mathf.Clamp01(currentHP / maxHP);

        // 控制圖片的填充量
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = fillAmount;
        }
    }
}