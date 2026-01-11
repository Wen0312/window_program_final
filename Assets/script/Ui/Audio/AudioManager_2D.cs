using System.Collections.Generic;
using UnityEngine;

public class AudioManager_2D : MonoBehaviour
{
    public static AudioManager_2D Instance;

    [Header("Audio Sources")]
    public AudioSource uiBGMSource;
    public AudioSource gameplayBGMSource;
    public AudioSource uiSFXSource;
    public AudioSource gameplaySFXSource;

    [Header("UI BGM Clips")]
    public AudioClip startUIBGM;
    public AudioClip gameOverUIBGM;

    [Header("Old Reference (Optional use)")]
    public AudioClip normalA_BGM;
    public AudioClip normalB_BGM;
    public AudioClip boss5_BGM;
    public AudioClip boss10_BGM;

    // ★★★ 新增：這裡就是你要的列表功能 ★★★
    [Header("Level BGM Configuration")]
    [Tooltip("請依照關卡順序拖入 BGM。Element 0 = Index 0 (第1關), Element 1 = Index 1 (第2關)...")]
    public List<AudioClip> levelClips;

    [Header("UI SFX Clips")]
    public AudioClip hoverClip;
    public AudioClip clickClip;

    [Header("SFX Optimization")]
    [Tooltip("least time between same sfx")]
    public float sfxCooldown = 0.1f;

    private Dictionary<AudioClip, float> sfxLastPlayTimes = new Dictionary<AudioClip, float>();

    public float BGMVolume { get; private set; } = 1f;
    public float SFXVolume { get; private set; } = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    // ======================
    // UI BGM
    // ======================
    public void PlayUIBGM(UIBGMType type)
    {
        AudioClip target = null;

        if (type == UIBGMType.Start) target = startUIBGM;
        if (type == UIBGMType.GameOver) target = gameOverUIBGM;

        if (uiBGMSource.clip == target) return;

        uiBGMSource.Stop();
        uiBGMSource.clip = target;
        uiBGMSource.volume = BGMVolume;
        if (target != null)
        {
            uiBGMSource.loop = true;
            uiBGMSource.Play();
        }
    }

    public void StopUIBGM()
    {
        uiBGMSource.Stop();
        uiBGMSource.clip = null;
    }

    // ======================
    // Gameplay BGM（重點）
    // ======================
    public void PlayGameplayBGM(GameplayBGMType type)
    {
        AudioClip target = null;
        // 這裡維持原本邏輯，給舊系統使用
        switch (type)
        {
            case GameplayBGMType.Normal_A: target = normalA_BGM; break;
            case GameplayBGMType.Normal_B: target = normalB_BGM; break;
            case GameplayBGMType.Boss_5: target = boss5_BGM; break;
            case GameplayBGMType.Boss_10: target = boss10_BGM; break;
        }

        if (target == null) return;
        if (gameplayBGMSource.clip == target) return;
        gameplayBGMSource.volume = BGMVolume;

        gameplayBGMSource.Stop();
        gameplayBGMSource.clip = target;
        gameplayBGMSource.loop = true;
        gameplayBGMSource.Play();
    }

    public void PauseGameplayBGM(bool pause)
    {
        if (pause) gameplayBGMSource.Pause();
        else gameplayBGMSource.UnPause();
    }

    // ======================
    // UI SFX
    // ======================
    public void PlayUIHover()
    {
        if (hoverClip)
            uiSFXSource.PlayOneShot(hoverClip);
    }

    public void PlayUIClick()
    {
        if (clickClip)
            uiSFXSource.PlayOneShot(clickClip);
    }

    public void PlayGameplayBGM_NormalA()
    {
        if (gameplayBGMSource.clip == normalA_BGM)
            return;

        gameplayBGMSource.Stop();
        gameplayBGMSource.clip = normalA_BGM;
        gameplayBGMSource.loop = true;
        gameplayBGMSource.Play();
    }

    public void PlayGameplaySFX(AudioClip clip)
    {
        // 1. 基本檢查
        if (clip == null || gameplaySFXSource == null) return;

        // 2. 檢查冷卻時間 (這是原本失效的地方)
        float lastTime;
        if (sfxLastPlayTimes.TryGetValue(clip, out lastTime))
        {
            // 如果現在時間距離上次播放還不到冷卻時間，直接 return，不要播放
            if (Time.time < lastTime + sfxCooldown)
            {
                return;
            }
        }

        // 刪除原本在這裡的第一個 gameplaySFXSource.PlayOneShot(clip); 
        // 你原本的代碼在這裡強制播放了一次，導致冷卻檢查無效。

        // 3. 通過檢查後，才播放音效
        gameplaySFXSource.PlayOneShot(clip);

        // 4. 更新最後播放時間
        sfxLastPlayTimes[clip] = Time.time;
    }

    // 在 AudioManager_2D 類別中
    public void PlayFootstep(AudioClip clip)
    {
        if (clip == null || gameplaySFXSource == null) return;

        // 隨機微調音高，讓腳步聲不單調
        gameplaySFXSource.pitch = Random.Range(0.9f, 1.1f);
        gameplaySFXSource.PlayOneShot(clip);
        gameplaySFXSource.pitch = 1.0f; // 播完後還原
    }

    public void StopGameplayBGM()
    {
        if (gameplayBGMSource != null)
        {
            gameplayBGMSource.Stop();
        }
    }

    // ★★★ 新增：自動根據關卡編號決定播哪首 BGM ★★★
    public void PlayBGMByLevelIndex(int mapIndex)
    {
        // 1. 防呆檢查：確認列表有沒有初始化，以及 Index 是否超出範圍
        if (levelClips == null || mapIndex < 0 || mapIndex >= levelClips.Count)
        {
            Debug.LogWarning($"[AudioManager] 找不到 Index {mapIndex} 對應的 BGM，請檢查 Level Clips 列表長度！");
            return;
        }

        // 2. 直接從列表中取出對應的 Clip
        AudioClip targetClip = levelClips[mapIndex];

        // 3. 如果該欄位是空的，或者正在播放同一首，就什麼都不做
        if (targetClip == null || gameplayBGMSource.clip == targetClip)
            return;

        // 4. 播放邏輯
        gameplayBGMSource.Stop();
        gameplayBGMSource.clip = targetClip;
        gameplayBGMSource.volume = BGMVolume;
        gameplayBGMSource.loop = true;
        gameplayBGMSource.Play();
    }

    public void SetBGMVolume(float value)
    {
        // 1. 強制顯示接收到的數值
        Debug.Log($"[Debug] Slider 傳來的原始數值: {value}");

        BGMVolume = Mathf.Clamp01(value);

        // 2. 顯示 Clamp 之後的數值
        Debug.Log($"[Debug] 修正後的 BGMVolume: {BGMVolume}");

        if (uiBGMSource != null)
        {
            uiBGMSource.volume = BGMVolume;
            // 3. 檢查 AudioSource 是否真的活著
            Debug.Log($"[Debug] UI AudioSource 音量現在是: {uiBGMSource.volume}, Mute狀態: {uiBGMSource.mute}, 播放狀態: {uiBGMSource.isPlaying}");
        }

        if (gameplayBGMSource != null) gameplayBGMSource.volume = BGMVolume;

        PlayerPrefs.SetFloat("BGM_Volume", BGMVolume);
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);

        // 同時調整 UI 和 遊戲 SFX
        if (uiSFXSource != null) uiSFXSource.volume = SFXVolume;
        if (gameplaySFXSource != null) gameplaySFXSource.volume = SFXVolume;

        // 儲存設定
        PlayerPrefs.SetFloat("SFX_Volume", SFXVolume);
    }

    private void LoadVolumeSettings()
    {
        // 讀取設定，預設值為 1 (最大聲)
        BGMVolume = PlayerPrefs.GetFloat("BGM_Volume", 1f);
        SFXVolume = PlayerPrefs.GetFloat("SFX_Volume", 1f);

        // 套用讀取到的數值
        SetBGMVolume(BGMVolume);
        SetSFXVolume(SFXVolume);
    }
}