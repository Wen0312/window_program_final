using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UISFXChannel_2D : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }
}
