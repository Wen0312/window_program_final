using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelClearUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject clearPanel;

    [Header("Audio")]
    public AudioClip clearSFX;

    void Awake()
    {
        if (clearPanel != null)
            clearPanel.SetActive(false);
    }

    // --- 預留給地塊觸發的接口 ---
    public void ShowClear()
    {
        if (clearPanel != null)
            clearPanel.SetActive(true);

        // 播放音效 (適配你的 AudioManager)
        if (clearSFX != null && AudioManager_2D.Instance != null)
        {
            // 透過 AudioManager 播放，這會自動進入 gameplaySFXSource
            AudioManager_2D.Instance.PlayGameplaySFX(clearSFX);
        }

        Time.timeScale = 0f; // 暫停遊戲
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;

        // 計算下一關
        int nextIndex = GameManager.CurrentMapIndex + 1;

        // ★ 解鎖進度（只會往前，不會倒退）
        if (nextIndex > GameManager.MaxUnlockedMapIndex)
        {
            GameManager.MaxUnlockedMapIndex = nextIndex;
            PlayerPrefs.SetInt("MAX_UNLOCKED_MAP", GameManager.MaxUnlockedMapIndex);
        }

        GameManager.CurrentMapIndex = nextIndex;
        GameManager.IsLoadingNextLevel = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        // 回主選單時，記得把狀態都重置
        GameManager.CurrentMapIndex = 0;
        GameManager.IsLoadingNextLevel = false; // ★ 確保回主選單時會顯示 Start UI

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}