using UnityEngine;
using UnityEngine.SceneManagement; // 必須引用這行來讀取場景 Index

// (保留原本的 Enum)
public enum GameState
{
    Playing,
    Paused,
    GameOver,
    StageClear
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static int MaxUnlockedMapIndex = 0;

    public GameState CurrentState { get; private set; } = GameState.Playing;

    private float previousTimeScale = 1f;
    public static int CurrentMapIndex = 0;
    public static bool IsLoadingNextLevel = false;

    [Header("Map System")]
    public MapCatalog2D mapCatalog;

    // --- 新增部分：UI 引用 ---
    [Header("Level Flow UI")]
    public LevelClearUI levelClearUI; // 請在 Inspector 拖入
    public VictoryUI victoryUI;       // 請在 Inspector 拖入

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 目前保持註解，依你原本設定
        }
        else
        {
            Destroy(gameObject);
        }
        MaxUnlockedMapIndex = PlayerPrefs.GetInt("MAX_UNLOCKED_MAP", 0);
    }


    // --- 保留原本的 Pause 相關邏輯，完全沒動 ---
    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            PauseGame();
        }
        else if (CurrentState == GameState.Paused)
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        CurrentState = GameState.Paused;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = previousTimeScale;
    }

    // --- 補上這段遺漏的函式 ---
    public void SetGameOver()
    {
        // 設定狀態為 GameOver，這樣 PauseMenu 就會知道現在不能暫停
        CurrentState = GameState.GameOver;
        Debug.Log("Game Over State Set.");
    }

    // --- 新增部分：過關觸發邏輯 ---
    public void TriggerLevelComplete()
    {
        if (CurrentState == GameState.GameOver || CurrentState == GameState.StageClear) return;

        CurrentState = GameState.StageClear;
        Debug.Log($"Level Complete! Current Map Index: {CurrentMapIndex}");

        // ★ 防呆：如果忘記拉 Catalog
        if (mapCatalog == null)
        {
            Debug.LogError("請在 GameManager Inspector 中放入 MapCatalog2D！");
            return;
        }

        // 1. 計算下一關的 Index
        int nextIndex = CurrentMapIndex + 1;

        // 2. 判斷 Catalog 裡還有沒有下一張圖
        if (nextIndex < mapCatalog.maps.Length)
        {
            // 還有下一關 -> Show Level Clear
            if (levelClearUI != null) levelClearUI.ShowClear();
        }
        else
        {
            // 沒有下一關了 -> Show Victory
            if (victoryUI != null) victoryUI.ShowVictory();
        }
    }

    // === 給 SelectLevelUI 使用：直接載入指定關卡 ===
    public void LoadMapByIndex(int levelIndex)
    {
        // 安全檢查
        if (levelIndex < 0 || levelIndex >= mapCatalog.maps.Length)
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }

        // 記錄目前關卡
        GameManager.CurrentMapIndex = levelIndex;

        // 交給 MapBootstrapper 載入
    }


}
