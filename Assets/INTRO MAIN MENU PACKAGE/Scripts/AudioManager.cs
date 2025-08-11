using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer Groups")]
    public AudioMixer audioMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup combatMusicGroup;

    [Header("Music Settings")]
    public AudioSource backgroundMusic;
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip creditsMusic;
    [Range(0f, 1f)] public float musicFadeDuration = 1f;

    [Header("SFX Settings")]
    public AudioSource sfxSource;
    public AudioClip buttonClickSound;
    [Range(0f, 1f)] public float sfxVolumeMultiplier = 0.7f;

    [Header("Combat Music Settings")]
    public AudioLowPassFilter combatMusicFilter;
    public float shopFilterCutoff = 1000f;
    public float normalFilterCutoff = 22000f;
    public float filterTransitionDuration = 1f;

    private float targetVolume;
    private bool isFading;
    private Coroutine filterTransitionCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void InitializeAudio()
    {
        // Configurar grupos de audio
        if (audioMixer != null)
        {
            if (backgroundMusic != null) 
                backgroundMusic.outputAudioMixerGroup = musicGroup;
            if (sfxSource != null)
                sfxSource.outputAudioMixerGroup = sfxGroup;
        }

        // Crear filtro si no existe
        if (combatMusicFilter == null && backgroundMusic != null)
        {
            combatMusicFilter = backgroundMusic.gameObject.AddComponent<AudioLowPassFilter>();
            combatMusicFilter.cutoffFrequency = normalFilterCutoff;
        }

        // Cargar configuraciones de volumen
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        
        SetMusicVolume(savedMusicVolume);
        SetSFXVolume(savedSFXVolume);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cambiar música automáticamente según la escena
        switch (scene.name)
        {
            case "MainMenu":
                PlayMenuMusic();
                ApplyShopEffect(false);
                break;
            case "BattleScene":
            case "Level1":
                PlayGameMusic();
                ApplyShopEffect(false);
                break;
            case "ShopScene":
                if (backgroundMusic.clip == gameMusic)
                {
                    ApplyShopEffect(true);
                }
                break;
            case "Credits":
                PlayCreditsMusic();
                ApplyShopEffect(false);
                break;
            default:
                ApplyShopEffect(false);
                break;
        }
    }

    // CORRECCIÓN: Modificador public en posición correcta
    public void ApplyShopEffect(bool enable)
    {
        if (combatMusicFilter == null || backgroundMusic.clip != gameMusic) return;

        if (filterTransitionCoroutine != null)
        {
            StopCoroutine(filterTransitionCoroutine);
        }
        
        filterTransitionCoroutine = StartCoroutine(TransitionFilter(
            enable ? shopFilterCutoff : normalFilterCutoff
        ));
    }

    private IEnumerator TransitionFilter(float targetCutoff)
    {
        float currentCutoff = combatMusicFilter.cutoffFrequency;
        float timer = 0f;
        
        while (timer < filterTransitionDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / filterTransitionDuration;
            combatMusicFilter.cutoffFrequency = Mathf.Lerp(currentCutoff, targetCutoff, progress);
            yield return null;
        }
        
        combatMusicFilter.cutoffFrequency = targetCutoff;
        filterTransitionCoroutine = null;
    }

    // CORRECCIÓN: Modificador public en posición correcta
    public void PlayMenuMusic()
    {
        ChangeMusic(menuMusic);
    }

    // CORRECCIÓN: Modificador public en posición correcta
    public void PlayGameMusic()
    {
        ChangeMusic(gameMusic);
    }

    // CORRECCIÓN: Modificador public en posición correcta
    public void PlayCreditsMusic()
    {
        ChangeMusic(creditsMusic);
    }

    private void ChangeMusic(AudioClip newClip)
    {
        if (backgroundMusic.clip == newClip && backgroundMusic.isPlaying)
            return;

        StartCoroutine(FadeChangeMusic(newClip));
    }

    private IEnumerator FadeChangeMusic(AudioClip newClip)
    {
        // Fade Out
        if (backgroundMusic.isPlaying)
        {
            float startVolume = backgroundMusic.volume;
            float timer = 0f;
            
            while (timer < musicFadeDuration / 2)
            {
                timer += Time.unscaledDeltaTime;
                backgroundMusic.volume = Mathf.Lerp(startVolume, 0f, timer / (musicFadeDuration / 2));
                yield return null;
            }
        }

        // Cambiar clip
        backgroundMusic.clip = newClip;
        backgroundMusic.Play();

        // Configurar grupo de audio
        if (newClip == gameMusic && combatMusicGroup != null)
        {
            backgroundMusic.outputAudioMixerGroup = combatMusicGroup;
        }
        else if (musicGroup != null)
        {
            backgroundMusic.outputAudioMixerGroup = musicGroup;
        }

        // Resetear filtro
        if (combatMusicFilter != null)
        {
            combatMusicFilter.cutoffFrequency = normalFilterCutoff;
        }

        // Fade In
        float timerIn = 0f;
        while (timerIn < musicFadeDuration / 2)
        {
            timerIn += Time.unscaledDeltaTime;
            backgroundMusic.volume = Mathf.Lerp(0f, PlayerPrefs.GetFloat("MusicVolume", 0.7f), 
                                                timerIn / (musicFadeDuration / 2));
            yield return null;
        }
    }

    // CORRECCIÓN: Modificador public en posición correcta
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // CORRECCIÓN: Modificador public en posición correcta
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    // CORRECCIÓN: Modificador public en posición correcta
    public void SetMasterVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        AudioListener.volume = clampedVolume;
        PlayerPrefs.SetFloat("MasterVolume", clampedVolume);
        PlayerPrefs.Save();
    }

    // CORRECCIÓN: Modificador public en posición correcta
    public void SetMusicVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = clampedVolume;
        }
        PlayerPrefs.SetFloat("MusicVolume", clampedVolume);
        PlayerPrefs.Save();
    }

    // CORRECCIÓN: Modificador public en posición correcta
    public void SetSFXVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = clampedVolume * sfxVolumeMultiplier;
        }
        PlayerPrefs.SetFloat("SFXVolume", clampedVolume);
        PlayerPrefs.Save();
    }

    // CORRECCIÓN: Modificador public en posición correcta
    public void StopMusic()
    {
        StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        if (backgroundMusic == null) yield break;
        
        float startVolume = backgroundMusic.volume;
        float timer = 0f;
        
        while (timer < musicFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            backgroundMusic.volume = Mathf.Lerp(startVolume, 0f, timer / musicFadeDuration);
            yield return null;
        }
        
        backgroundMusic.Stop();
    }
}