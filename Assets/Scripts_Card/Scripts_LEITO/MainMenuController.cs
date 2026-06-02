using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button socialButton;
    [SerializeField] private GameObject socialTooltip;
    [SerializeField] private Animator glitchAnimator;
    
    [Header("Audio")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip buttonClickSound;
    
    [Header("Volume Texts")]
    [SerializeField] private TMP_Text masterVolumeText;
    [SerializeField] private TMP_Text sfxVolumeText;
    
    [Header("Social Settings")]
    [SerializeField] private string socialWebsiteURL = "https://twitter.com/tuestudio";
    [SerializeField] private string socialTooltipText = "¡Síguenos en redes sociales!";
    
    private bool websiteOpened;

    void Start()
    {
        InitializeUI();
        StartCoroutine(StartupSequence());
    }

    private void InitializeUI()
    {
        versionText.text = $"v{Application.version}";
        
        // Cargar volumen guardado o usar valor por defecto
        float savedVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        masterVolumeSlider.value = savedVolume;
        sfxVolumeSlider.value = savedVolume;
        
        // Aplicar volumen al AudioSource si existe
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = savedVolume;
        }
        
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        
        socialTooltip.SetActive(false);
        SetupSocialButton();
        
        UpdateVolumeTexts();
        
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    #region Volume Management
    public void OnMasterVolumeChanged(float volume)
    {
        // Actualizar el volumen del AudioSource
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = volume;
        }
        
        // Guardar preferencia
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
        
        UpdateVolumeTexts();
    }

    public void OnSFXVolumeChanged(float volume)
    {
        // Actualizar el volumen del AudioSource
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = volume;
        }
        
        // Guardar preferencia
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
        
        UpdateVolumeTexts();
    }
    
    private void PlayButtonClick()
    {
        if (sfxAudioSource != null && buttonClickSound != null)
        {
            sfxAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    private void UpdateVolumeTexts()
    {
        if (masterVolumeText != null) 
            masterVolumeText.text = Mathf.RoundToInt(masterVolumeSlider.value * 100) + "%";
        
        if (sfxVolumeText != null) 
            sfxVolumeText.text = Mathf.RoundToInt(sfxVolumeSlider.value * 100) + "%";
    }
    #endregion

    #region UI Event Handlers
    public void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnSocialButtonClicked()
    {
        if (!websiteOpened)
        {
            websiteOpened = true;
            PlayButtonClick();
            Application.OpenURL(socialWebsiteURL);
            Invoke(nameof(ResetWebsiteOpened), 1f);
        }
    }
    
    private void ShowSocialTooltip() => socialTooltip.SetActive(true);
    private void HideSocialTooltip() => socialTooltip.SetActive(false);
    private void ResetWebsiteOpened() => websiteOpened = false;
    #endregion

    #region Button Actions
    public void OpenOptions()
    {
        PlayButtonClick();
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        PlayButtonClick();
        optionsPanel.SetActive(false);
    }

    public void StartGame()
    {
        PlayButtonClick();
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        // ►►► CORREGIDO: Usar TransitionToScene en lugar de LoadSceneWithCleanup ◄◄◄
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene("BattleScene");
        }
        else
        {
            yield return SimpleFadeOut();
            CleanupBattleObjectsFallback();
            SceneManager.LoadScene("BattleScene");
        }
        
        // La música de fondo ha sido eliminada
    }

    private void CleanupBattleObjectsFallback()
    {
        // Solo limpiar si es necesario (fallback)
        CardDisplay[] cards = FindObjectsOfType<CardDisplay>(true);
        foreach (CardDisplay card in cards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        
        HandManager handManager = FindObjectOfType<HandManager>(true);
        if (handManager != null) Destroy(handManager.gameObject);
    }

    public void OpenCredits()
    {
        PlayButtonClick();
        StartCoroutine(OpenCreditsSequence());
    }

    private IEnumerator OpenCreditsSequence()
    {
        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.StartCoroutine(SceneTransitionManager.Instance.FadeOut());
        }
        else
        {
            yield return SimpleFadeOut();
        }
        
        // La música de fondo ha sido eliminada
        SceneManager.LoadScene("Credits");
    }

    public void QuitGame()
    {
        PlayButtonClick();
        StartCoroutine(QuitSequence());
    }

    private IEnumerator QuitSequence()
    {
        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.StartCoroutine(SceneTransitionManager.Instance.FadeOut());
            SceneTransitionManager.Instance.TransitionToQuit();
        }
        else
        {
            yield return SimpleFadeOut();
            StartCoroutine(QuitApplicationFallback());
        }
    }
    #endregion

    #region Social Button Setup
    private void SetupSocialButton()
    {
        socialButton.onClick.AddListener(OnSocialButtonClicked);
        
        EventTrigger trigger = socialButton.gameObject.AddComponent<EventTrigger>();
        
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener((data) => ShowSocialTooltip());
        
        EventTrigger.Entry pointerExit = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener((data) => HideSocialTooltip());
        
        trigger.triggers.Add(pointerEnter);
        trigger.triggers.Add(pointerExit);
        
        if (socialTooltip != null)
        {
            TMP_Text tooltipText = socialTooltip.GetComponentInChildren<TMP_Text>();
            if (tooltipText != null) tooltipText.text = socialTooltipText;
        }
    }
    #endregion

    #region Coroutines
    private IEnumerator StartupSequence()
    {
        yield return null;
        
        if (SceneTransitionManager.Instance != null)
        {
            // ►►► CORREGIDO: Usar FadeIn directamente ◄◄◄
            yield return SceneTransitionManager.Instance.StartCoroutine(SceneTransitionManager.Instance.FadeIn());
        }
        else
        {
            yield return SimpleFadeIn();
        }
        
        if (glitchAnimator != null)
        {
            glitchAnimator.SetTrigger("Start");
            yield return new WaitForSeconds(0.8f);
        }
    }

    private IEnumerator SimpleFadeOut()
    {
        // Fallback simple si no hay SceneTransitionManager
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator SimpleFadeIn()
    {
        // Fallback simple si no hay SceneTransitionManager
        yield return null;
    }

    private IEnumerator QuitApplicationFallback()
    {
        yield return new WaitForSeconds(0.3f);
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    #endregion
}