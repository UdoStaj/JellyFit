using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsUIManager : MonoBehaviour
{
    [Header("Toggles")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    [Header("Buttons")]
    [SerializeField] private Button SaveButton;

    [Header("Mixers")]
    [SerializeField] private AudioMixer MusicMixer;
    [SerializeField] private AudioMixer SFXMixer;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    private void OnEnable()
    {
        // Kayýtlý ayarlarý yükle
        LoadSettings();

        // Slider dinleyicileri ayarla
        musicSlider.onValueChanged.AddListener(delegate { OnMusicSliderChange(); });
        SFXSlider.onValueChanged.AddListener(delegate { OnSFXSliderChange(); });

        // (Ýsteðe baðlý) Save butonuna kayýt fonksiyonu baðla
        if (SaveButton != null)
            SaveButton.onClick.AddListener(SaveSettings);
    }

    private void OnDisable()
    {
        // Dinleyicileri kaldýr
        musicSlider.onValueChanged.RemoveAllListeners();
        SFXSlider.onValueChanged.RemoveAllListeners();

        if (SaveButton != null)
            SaveButton.onClick.RemoveAllListeners();
    }

    private void OnMusicSliderChange()
    {
        MusicMixer.SetFloat("Volume", musicSlider.value);
    }

    private void OnSFXSliderChange()
    {
        SFXMixer.SetFloat("Volume", SFXSlider.value);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, musicSlider.value);
        PlayerPrefs.SetFloat(SFXVolumeKey, SFXSlider.value);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        float musicVol = PlayerPrefs.GetFloat(MusicVolumeKey, 1f); // Varsayýlan deðer 0
        float sfxVol = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);     // Varsayýlan deðer 0

        musicSlider.value = musicVol;
        SFXSlider.value = sfxVol;

        MusicMixer.SetFloat("Volume", musicVol);
        SFXMixer.SetFloat("Volume", sfxVol);
    }
}
