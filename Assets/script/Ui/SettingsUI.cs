using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Return Targets")]
    public GameObject startMenuPanel;
    public GameObject pauseMenuPanel;

    private bool isFromPauseMenu = false;

    void Start()
    {
        if (AudioManager_2D.Instance == null) return;

        // 初始化 Slider 顯示
        bgmSlider.SetValueWithoutNotify(AudioManager_2D.Instance.BGMVolume);
        sfxSlider.SetValueWithoutNotify(AudioManager_2D.Instance.SFXVolume);

        // 綁定事件（只綁一次）
        bgmSlider.onValueChanged.AddListener(OnBGMValueChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXValueChanged);
    }

    // 使用 OnEnable 而不是 Start，這樣每次打開設定介面都會刷新 Slider 狀態

    public void OnBGMValueChanged(float value)
    {
        if (AudioManager_2D.Instance != null)
        {
            // 這邊只調整音量，不會 Stop 音樂，所以原本的 BGM 會繼續播
            AudioManager_2D.Instance.SetBGMVolume(value);
        }
    }

    public void OnSFXValueChanged(float value)
    {
        if (AudioManager_2D.Instance != null)
        {
            AudioManager_2D.Instance.SetSFXVolume(value);
        }
    }

    // === 開啟設定選單 ===

    public void OpenFromStartMenu()
    {
        isFromPauseMenu = false;
        if (AudioManager_2D.Instance != null)
        {
            AudioManager_2D.Instance.PlayUIBGM(UIBGMType.Start);
        }
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
    }

    public void OpenFromPauseMenu()
    {
        isFromPauseMenu = true;
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    // === 關閉設定選單 (Back 按鈕) ===
    public void OnBackButtonClicked()
    {
        // 播放點擊音效
        if (AudioManager_2D.Instance != null) AudioManager_2D.Instance.PlayUIClick();

        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (isFromPauseMenu)
        {
            // 回到遊戲暫停選單 (音樂繼續保持原本的 Level BGM)
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        }
        else
        {
            // 回到主選單 (音樂繼續保持原本的 Start BGM)
            if (startMenuPanel != null) startMenuPanel.SetActive(true);
        }
    }
}