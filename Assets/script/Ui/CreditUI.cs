using UnityEngine;

public class CreditUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject creditPanel;      // 整個 Credit 介面的容器
    public RectTransform scrollingContent; // 要捲動的文字物件 (Text 的 RectTransform)
    public StartMenuUI startMenu;       // 用來返回主選單
    
    [Header("Image Feature")]
    public GameObject targetImageObject; // ★ 新增：這是你要顯示的那張圖片物件 (Image)

    [Header("Settings")]
    public float scrollSpeed = 100f;    // 捲動速度
    public float resetPositionY = 1500f;// 捲到多高要重置 (根據你的文字長度調整)

    private Vector2 initialPosition;

    void Awake()
    {
        if (scrollingContent != null)
        {
            initialPosition = scrollingContent.anchoredPosition;
        }

        // 預設關閉
        if (creditPanel != null) creditPanel.SetActive(false);
        if (targetImageObject != null) targetImageObject.SetActive(false); // 圖片預設隱藏
    }

    void Update()
    {
        // 只有當介面開啟時才捲動
        if (creditPanel != null && creditPanel.activeSelf && scrollingContent != null)
        {
            // ★ 關鍵：使用 unscaledDeltaTime，這樣就算遊戲暫停 (TimeScale=0) 文字也會動
            scrollingContent.anchoredPosition += Vector2.up * scrollSpeed * Time.unscaledDeltaTime;

            // 循環捲動邏輯 (如果你希望它捲完重頭開始)
            if (scrollingContent.anchoredPosition.y >= resetPositionY)
            {
                scrollingContent.anchoredPosition = initialPosition;
            }
        }
    }

    // --- 給外部呼叫的函式 ---

    public void Show()
    {
        if (creditPanel != null) creditPanel.SetActive(true);
        if (targetImageObject != null) targetImageObject.SetActive(false);
        // 每次打開都重置位置
        if (scrollingContent != null) scrollingContent.anchoredPosition = initialPosition;
    }

    public void Hide()
    {
        if (creditPanel != null) creditPanel.SetActive(false);
    }

    // --- 按鈕功能 ---

    // 按鈕 1: 返回主選單
    public void OnBackClicked()
    {
        Hide();
        if (startMenu != null)
        {
            startMenu.ShowMainPanel(); // 呼叫 StartMenu 顯示回原本的按鈕
        }
    }

    // 按鈕 2: 例如打開網頁 (Github / Portfolio)
    public void OnShowImageToggle()
    {
        if (targetImageObject != null)
        {
            // 讀取目前的狀態，然後設為相反 (開變關，關變開)
            bool isActive = targetImageObject.activeSelf;
            targetImageObject.SetActive(!isActive);
        }
    }
}