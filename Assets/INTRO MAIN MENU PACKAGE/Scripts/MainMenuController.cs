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
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button socialButton;
    [SerializeField] private GameObject socialTooltip;
    [SerializeField] private Animator glitchAnimator;
    
    [Header("Volume Texts")]
    [SerializeField] private TMP_Text masterVolumeText;
    [SerializeField] private TMP_Text musicVolumeText;
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
        
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.7f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        
        socialTooltip.SetActive(false);
        SetupSocialButton();
        
        UpdateVolumeTexts();
        
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    #region Volume Management
    public void OnMasterVolumeChanged(float volume)
    {
        AudioManager.Instance.SetMasterVolume(volume);
        UpdateVolumeTexts();
    }

    public void OnMusicVolumeChanged(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
        UpdateVolumeTexts();
    }

    public void OnSFXVolumeChanged(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
        UpdateVolumeTexts();
    }

    private void UpdateVolumeTexts()
    {
        if (masterVolumeText != null) 
            masterVolumeText.text = Mathf.RoundToInt(masterVolumeSlider.value * 100) + "%";
        
        if (musicVolumeText != null) 
            musicVolumeText.text = Mathf.RoundToInt(musicVolumeSlider.value * 100) + "%";
        
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
            AudioManager.Instance.PlayButtonClick();
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
        AudioManager.Instance.PlayButtonClick();
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        AudioManager.Instance.PlayButtonClick();
        optionsPanel.SetActive(false);
    }

    public void StartGame()
    {
        AudioManager.Instance.PlayButtonClick();
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.StartCoroutine(
                SceneTransitionManager.Instance.LoadSceneWithCleanup("BattleScene")
            );
        }
        else
        {
            yield return SimpleFadeOut();
            CleanupBattleObjectsFallback();
            SceneManager.LoadScene("BattleScene");
        }
        
        AudioManager.Instance.PlayGameMusic();
    }

    private void CleanupBattleObjectsFallback()
    {
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
        AudioManager.Instance.PlayButtonClick();
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
        
        AudioManager.Instance.PlayCreditsMusic();
        SceneManager.LoadScene("Credits");
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlayButtonClick();
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
            SceneTransitionManager.Instance.fadeOnStart = true;
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
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.fadePanel != null)
        {
            Image fadePanel = SceneTransitionManager.Instance.fadePanel;
            float timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                fadePanel.color = new Color(0, 0, 0, timer);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator SimpleFadeIn()
    {
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.fadePanel != null)
        {
            Image fadePanel = SceneTransitionManager.Instance.fadePanel;
            float timer = 1f;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                fadePanel.color = new Color(0, 0, 0, timer);
                yield return null;
            }
        }
        else
        {
            yield return null;
        }
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