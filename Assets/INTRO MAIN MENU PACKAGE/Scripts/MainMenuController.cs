using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button socialButton;
    [SerializeField] private GameObject socialTooltip;
    [SerializeField] private Animator glitchAnimator;
    
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
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.7f);
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        
        socialTooltip.SetActive(false);
        SetupSocialButton();
        
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    #region UI Event Handlers
    public void OnVolumeChanged(float volume)
    {
        AudioManager.Instance.SetVolume(volume);
    }

    public void OnFullscreenChanged(bool isFullscreen)
    {
        DisplayManager.Instance.SetFullscreen(isFullscreen);
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
        SceneTransitionManager.Instance.TransitionToScene("BattleScene");
    }

    public void OpenCredits()
    {
        AudioManager.Instance.PlayButtonClick();
        SceneTransitionManager.Instance.TransitionToScene("Credits");
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlayButtonClick();
        
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToQuit();
        }
        else
        {
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
        
        socialTooltip.GetComponentInChildren<TMP_Text>().text = socialTooltipText;
    }
    #endregion

    #region Coroutines
    private IEnumerator StartupSequence()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PlayFadeOutOnSceneLoad();
        }
        
        yield return null;
        
        if (glitchAnimator != null)
        {
            glitchAnimator.SetTrigger("Start");
            yield return new WaitForSeconds(0.8f);
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