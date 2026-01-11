using UnityEngine;
using UnityEngine.InputSystem; // 使用新版輸入系統獲取滑鼠

public class PlayerVisuals : MonoBehaviour
{
    [Header("關聯組件")]
    public Animator anim;
    public TopDownPlayerMove2D moveLogic; // 連結核心移動腳本

    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
        
        // 如果沒手動拖入，嘗試自動抓取同物件上的移動腳本
        if (moveLogic == null) moveLogic = GetComponent<TopDownPlayerMove2D>();
    }

    void Update()
    {
        if (moveLogic == null) return;

        // 1. 驅動動畫：從核心腳本讀取 moveInput
        if (anim != null)
        {
            // 讀取 moveLogic 裡面的 moveInput
            float currentSpeed = moveLogic.GetCurrentInput().magnitude; 
            anim.SetFloat("Speed", currentSpeed);
        }

        // 2. 處理轉身：根據滑鼠位置翻轉 Scale
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (Mouse.current == null) return;

        // 取得滑鼠世界座標 (新版 Input System 寫法)
        Vector3 screenMousePos = Mouse.current.position.ReadValue();
        Vector3 worldMousePos = mainCam.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 0));

        // 翻轉 LocalScale (不影響物理碰撞，只影響視覺)
        if (worldMousePos.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
}