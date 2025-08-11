using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip backgroundMusic;
    public AudioClip menuMusic;

    [Header("Game Sound Effects")]
    public AudioClip coinSound;
    public AudioClip jumpSound;
    public AudioClip damageSound;
    public AudioClip gameOverSound;
    public AudioClip victorySound;
    public AudioClip buttonClickSound;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Tạo AudioSource nếu chưa có
            SetupAudioSources();
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Play background music by default
        if (backgroundMusic != null)
            PlayMusic(backgroundMusic);
    }

    void SetupAudioSources()
    {
        // Tạo Music Source nếu chưa có
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        // Tạo SFX Source nếu chưa có
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    void LoadSettings()
    {
        // Load volume settings
        bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        bool soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;

        SetMusicVolume(musicEnabled ? 0.7f : 0f);
        SetSFXVolume(soundEnabled ? 1f : 0f);
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = volume;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            // Nếu đang phát clip khác thì mới thay đổi
            if (musicSource.clip != clip)
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
            else if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null && sfxSource.volume > 0)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Convenience methods
    public void PlayCoinSound()
    {
        PlaySFX(coinSound);
    }

    public void PlayJumpSound()
    {
        PlaySFX(jumpSound);
    }

    public void PlayDamageSound()
    {
        PlaySFX(damageSound);
    }

    public void PlayGameOverSound()
    {
        PlaySFX(gameOverSound);
    }

    public void PlayVictorySound()
    {
        PlaySFX(victorySound);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    // Method để chuyển giữa menu music và gameplay music
    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameMusic()
    {
        PlayMusic(backgroundMusic);
    }
}