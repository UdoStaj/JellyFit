using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource sfxSource;
    public AudioSource bgmSource;

    public AudioClip dragClip;
    public AudioClip sliceClip;
    public AudioClip successClip;
    public AudioClip errorClip;
    public AudioClip bgmClip;
    public AudioClip clickClip;

    private void Awake()
    {
        // Singleton yapýsý
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBGM();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayDrag() => PlaySFX(dragClip);
    public void PlaySlice() => PlaySFX(sliceClip);
    public void PlaySuccess() => PlaySFX(successClip);
    public void PlayError() => PlaySFX(errorClip);
    public void PlayClick() => PlaySFX(clickClip);

    public void PlayBGM()
    {
        if (bgmClip != null && !bgmSource.isPlaying)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
}
