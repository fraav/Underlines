using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

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

    private float targetVolume;
    private bool isFading;

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

    void Update()
    {
        if (isFading)
        {
            backgroundMusic.volume = Mathf.MoveTowards(
                backgroundMusic.volume, 
                targetVolume, 
                Time.unscaledDeltaTime / musicFadeDuration
            );

            if (Mathf.Approximately(backgroundMusic.volume, targetVolume))
            {
                isFading = false;
                
                // Si el volumen llegó a 0, detener la música
                if (targetVolume == 0f)
                {
                    backgroundMusic.Stop();
                }
            }
        }
    }

    void InitializeAudio()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.7f);
        SetVolume(savedVolume);
        
        // Configurar volúmenes relativos
        backgroundMusic.volume = savedVolume;
        sfxSource.volume = savedVolume * sfxVolumeMultiplier;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cambiar música automáticamente según la escena
        switch (scene.name)
        {
            case "MainMenu":
                PlayMenuMusic();
                break;
            case "BattleScene":
            case "Level1": // Puedes añadir más nombres de escenas de juego
                PlayGameMusic();
                break;
            case "Credits":
                PlayCreditsMusic();
                break;
            default:
                // No cambiar música para escenas no especificadas
                break;
        }
    }

    public void PlayMenuMusic()
    {
        ChangeMusic(menuMusic);
    }

    public void PlayGameMusic()
    {
        ChangeMusic(gameMusic);
    }

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
            targetVolume = 0f;
            isFading = true;
            yield return new WaitUntil(() => !isFading);
        }

        // Cambiar clip
        backgroundMusic.clip = newClip;
        backgroundMusic.Play();

        // Fade In
        targetVolume = PlayerPrefs.GetFloat("MasterVolume", 0.7f);
        isFading = true;
    }

    public void PlayButtonClick()
    {
        sfxSource.PlayOneShot(buttonClickSound, sfxSource.volume);
    }

    public void SetVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        
        // Aplicar volúmenes relativos
        AudioListener.volume = clampedVolume;
        backgroundMusic.volume = clampedVolume;
        sfxSource.volume = clampedVolume * sfxVolumeMultiplier;
        
        targetVolume = clampedVolume; // Actualizar volumen objetivo para fades

        PlayerPrefs.SetFloat("MasterVolume", clampedVolume);
        PlayerPrefs.Save();
    }

    public void StopMusic()
    {
        StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        targetVolume = 0f;
        isFading = true;
        yield return new WaitUntil(() => !isFading);
    }
}