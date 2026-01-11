using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject hudPanel; // 死亡時隱藏 HUD

    PlayerHealth player;

    void Awake()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void OnEnable()
    {
        player = FindFirstObjectByType<PlayerHealth>();
        if (player != null)
            player.OnDead += ShowGameOver;
    }

    void OnDisable()
    {
        if (player != null)
            player.OnDead -= ShowGameOver;
    }

    void ShowGameOver()
    {
        // 1. 通知 Manager 鎖住狀態 (防止按 ESC)
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameOver();

        // ★★★ 新增的部分：呼叫 AudioManager 停止一般關卡音樂 ★★★
        if (AudioManager_2D.Instance != null)
        {
            // 注意：請確保你的 AudioManager_2D.cs 裡已經加上了 StopGameplayBGM() 這個函式
            // (就是我們上一段對話中加的那個功能)
            AudioManager_2D.Instance.StopGameplayBGM();
        }
        // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

        // 2. UI 處理
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);

        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        // 確保重啟時狀態重置 (GameManager Start 會做，但保險起見)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}