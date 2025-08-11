using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleSettings : MonoBehaviour
{
    [Header("Simple Controls")]
    public Toggle soundToggle;
    public Toggle musicToggle;

    [Header("Display Text (Optional)")]
    public TextMeshProUGUI soundText;
    public TextMeshProUGUI musicText;

    void Start()
    {
        LoadSettings();
        SetupToggles();
    }

    void LoadSettings()
    {
        // Load saved settings (1 = ON, 0 = OFF)
        bool soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

        if (soundToggle != null)
            soundToggle.isOn = soundEnabled;
        if (musicToggle != null)
            musicToggle.isOn = musicEnabled;

        // Apply settings immediately
        ApplySoundSetting(soundEnabled);
        ApplyMusicSetting(musicEnabled);

        UpdateTexts();
    }

    void SetupToggles()
    {
        soundToggle?.onValueChanged.AddListener(OnSoundToggled);
        musicToggle?.onValueChanged.AddListener(OnMusicToggled);
    }

    void OnSoundToggled(bool isOn)
    {
        PlayerPrefs.SetInt("SoundEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplySoundSetting(isOn);
        UpdateTexts();
    }

    void OnMusicToggled(bool isOn)
    {
        PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicSetting(isOn);
        UpdateTexts();
    }

    void ApplySoundSetting(bool enabled)
    {
        // Sử dụng null check an toàn
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(enabled ? 1f : 0f);
        else
            Debug.LogWarning("AudioManager not found!");
    }

    void ApplyMusicSetting(bool enabled)
    {
        // Sử dụng null check an toàn
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(enabled ? 0.7f : 0f);
        else
            Debug.LogWarning("AudioManager not found!");
    }

    void UpdateTexts()
    {
        if (soundText != null && soundToggle != null)
            soundText.text = soundToggle.isOn ? "🔊 SOUND" : "🔇 SOUND";
        if (musicText != null && musicToggle != null)
            musicText.text = musicToggle.isOn ? "🎵 MUSIC" : "🎵 MUSIC";
    }
}