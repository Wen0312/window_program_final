using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameplayBGMChannel_2D : MonoBehaviour
{
    [Header("Audio Sources")]
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;

        if (source.clip == clip && source.isPlaying)
            return;

        source.Stop();
        source.clip = clip;
        source.Play();
    }

    public void Stop()
    {
        source.Stop();
        source.clip = null;
    }
}
