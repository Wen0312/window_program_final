using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UIBGMChannel_2D : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null)
            return;

        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }
}
