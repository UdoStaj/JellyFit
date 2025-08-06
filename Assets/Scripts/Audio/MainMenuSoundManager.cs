using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuSoundManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;

    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    public static MainMenuSoundManager Instance;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip clickClip;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayClick()
    {
        if (clickClip != null)
            sfxSource.PlayOneShot(clickClip);
    }

    private void Start()
    {
        // Kaydedilen deðeri geri yükle
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;

        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);

        // Listener ekle
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume <= 0.0001f ? 0.0001f : volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume <= 0.0001f ? 0.0001f : volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}

