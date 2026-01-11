using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameplaySFXChannel_2D : MonoBehaviour
{
    public AudioSource sfxSource;

    private void Awake()
    {
        // 自動抓同一個 GameObject 上的 AudioSource
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        // 強制設定成 SFX 合理狀態（不影響你 Inspector 的 Volume）
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f; // 2D
    }

    /// <summary>
    /// 播放一次性 Gameplay SFX（受傷 / 攻擊 / 爆炸）
    /// </summary>
    public void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip);
    }
}
