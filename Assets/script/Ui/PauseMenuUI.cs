using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject hudPanel; // 建議把 HUD (血條/彈藥) 放在一個父物件下，暫停時隱藏

    [Header("Settings")]
    public SettingsUI settingsUI; // ★ 記得在 Inspector 拖進去

    // 給暫停選單上的「Setting」按鈕呼叫
    public void OnSettingsClicked()
    {
        if (settingsUI != null)
        {
            settingsUI.OpenFromPauseMenu();
        }
    }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        // 檢查 ESC 按鍵
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CheckPauseToggle();
        }
    }

    void CheckPauseToggle()
    {
        // 【防呆關鍵】：只有在「遊玩中」或「暫停中」才能切換
        // 如果現在是 GameOver 或 Victory，按 ESC 無效
        if (GameManager.Instance.CurrentState == GameState.Playing ||
            GameManager.Instance.CurrentState == GameState.Paused)
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
                Pause();
            else
                Resume();
        }
    }

    void Pause()
    {
        // 1. 呼叫 Manager 切換狀態
        GameManager.Instance.PauseGame();

        // 2. UI 開關
        if (pausePanel != null) pausePanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false); // 暫停時隱藏 HUD，畫面比較乾淨
    }

    public void Resume()
    {
        // 1. 呼叫 Manager 切換狀態
        GameManager.Instance.ResumeGame();

        // 2. UI 開關
        if (pausePanel != null) pausePanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        // 1. 重要：把時間流動恢復，不然回到主選單會卡住
        Time.timeScale = 1f;

        // 2. 恢復遊戲狀態 (避免卡在 Paused 狀態)
        if (GameManager.Instance != null)
        {
            // 假設你有一個重置狀態的方法，或者單純恢復成 Playing 讓 StartMenu 接手
            GameManager.Instance.ResumeGame();
        }

        // 3. 重新載入當前場景
        // 這樣會觸發 StartMenuUI 的 Start()，自然就會顯示主選單並暫停遊戲
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}