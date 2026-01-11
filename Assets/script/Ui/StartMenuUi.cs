using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class StartMenuUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startPanel;

    [Header("References")]
    public CreditUI creditUI;       // 保留原本變數
    public GameObject hudPanel;     // 保留原本變數[Header("Settings")]
    public SettingsUI settingsUI; // ★ 記得在 Inspector 拖進去
    public SelectLevelUI selectLevelUI;

    // 給主選單上的「Setting」按鈕呼叫
    public void OnSettingsClicked()
    {
        if (settingsUI != null)
        {
            settingsUI.OpenFromStartMenu();
        }
    }

    void Start()
    {
        if (GameManager.IsLoadingNextLevel)
        {
            // === 情況 A：正在載入下一關 ===
            if (startPanel != null) startPanel.SetActive(false);

            // 確保遊戲狀態是「Playing」，讓角色可以動
            GameManager.Instance.ResumeGame();

            GameManager.IsLoadingNextLevel = false;
        }
        else
        {
            // === 情況 B：剛進遊戲 / 回主選單 ===
            if (startPanel != null) startPanel.SetActive(true);

            // ★★★ 關鍵修改：直接套用 PauseMenuUI 的邏輯 ★★★
            // PauseMenuUI 呼叫這個就能停住角色，這裡也一定可以！
            // 這會設定 State = Paused 並且 TimeScale = 0
            GameManager.Instance.PauseGame();

            // 確保滑鼠顯示，讓你可以點選單
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 音樂處理
            if (AudioManager_2D.Instance != null)
            {
                AudioManager_2D.Instance.PlayUIBGM(UIBGMType.Start);
                AudioManager_2D.Instance.StopGameplayBGM();
            }
        }
    }

    // 當按下 Start 按鈕
    public void OnStartGame()
    {
        startPanel.SetActive(false);
        GameManager.CurrentMapIndex = 0;
        GameManager.MaxUnlockedMapIndex = 0;
        GameManager.IsLoadingNextLevel = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // 恢復時間
        GameManager.Instance.ResumeGame();

        // ★★★ 4. 遊戲開始，把玩家輸入打開 ★★★
        GameManager.Instance.ResumeGame();

        if (AudioManager_2D.Instance != null)
        {
            AudioManager_2D.Instance.PlayBGMByLevelIndex(GameManager.CurrentMapIndex);
            AudioManager_2D.Instance.StopUIBGM();
        }
    }

    public void OnExitGame()
    {
        Debug.Log("Exit Game");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void OnCreditsClicked()
    {
        // 1. 隱藏主選單
        if (startPanel != null) startPanel.SetActive(false);

        // 2. 顯示 Credit 介面
        if (creditUI != null)
        {
            creditUI.Show();
        }
        else
        {
            Debug.LogError("CreditUI 沒拉！請檢查 StartMenuUI 的 Inspector");
        }
    }

    // 給 CreditUI 的「Back 按鈕」呼叫 (當從 Credit 返回時)
    public void ShowMainPanel()
    {
        if (startPanel != null) startPanel.SetActive(true);
    }
    public void OnContinueClicked()
    {
        // 使用最後一次遊玩的關卡
        GameManager.CurrentMapIndex = GameManager.MaxUnlockedMapIndex;
        GameManager.IsLoadingNextLevel = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}

