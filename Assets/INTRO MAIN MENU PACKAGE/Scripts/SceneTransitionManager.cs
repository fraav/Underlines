using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Animation Settings")]
    public Animator transitionAnimator;
    public float minLoadTime = 0.5f;
    public float fadeAnimationBuffer = 0.2f;

    [Header("Manual Fade Settings")]
    public Image fadeOverlay;
    public float manualFadeDuration = 1f;
    public Color fadeColor = Color.black;

    private string targetScene;
    private bool isTransitioning;
    private bool isQuitting;

    public bool IsTransitioning => isTransitioning;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            InitializeFadeSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void InitializeFadeSystem()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
            fadeOverlay.gameObject.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetFadeState();
    }

    public void ResetFadeState()
    {
        if (fadeOverlay != null && !isTransitioning)
        {
            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
            fadeOverlay.gameObject.SetActive(false);
        }
    }

    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;
        targetScene = sceneName;
        StartCoroutine(TransitionSequence());
    }

    public void TransitionToQuit()
    {
        if (isTransitioning) return;
        isQuitting = true;
        StartCoroutine(TransitionSequence());
    }

    public void PlayFadeOutOnSceneLoad()
    {
        if (transitionAnimator != null && transitionAnimator.isActiveAndEnabled)
        {
            transitionAnimator.ResetTrigger("FadeIn");
            transitionAnimator.ResetTrigger("FadeOut");
            transitionAnimator.SetTrigger("FadeOut");
        }
        else
        {
            StartCoroutine(ManualFade(0));
        }
    }

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        // Fade In
        bool usedAnimator = false;
        if (transitionAnimator != null && transitionAnimator.isActiveAndEnabled)
        {
            usedAnimator = true;
            transitionAnimator.ResetTrigger("FadeIn");
            transitionAnimator.ResetTrigger("FadeOut");
            transitionAnimator.SetTrigger("FadeIn");
            
            yield return StartCoroutine(WaitForAnimation("FadeIn"));
        }
        else if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(ManualFade(1));
        }
        else
        {
            yield return new WaitForSecondsRealtime(minLoadTime);
        }

        // Scene Load
        if (!isQuitting)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);
            asyncLoad.allowSceneActivation = false;
            
            float elapsed = 0f;
            while (elapsed < minLoadTime)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            
            asyncLoad.allowSceneActivation = true;
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Fade Out
            if (usedAnimator)
            {
                PlayFadeOutOnSceneLoad();
            }
            else if (fadeOverlay != null)
            {
                yield return StartCoroutine(ManualFade(0));
            }
        }
        else
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        // Cleanup
        if (!isQuitting && fadeOverlay != null && fadeOverlay.gameObject.activeSelf)
        {
            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
            fadeOverlay.gameObject.SetActive(false);
        }

        isTransitioning = false;
    }

    private IEnumerator WaitForAnimation(string stateName)
    {
        if (transitionAnimator == null) yield break;
        
        float timeout = 1f;
        float elapsed = 0f;
        
        while (!transitionAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        if (elapsed >= timeout)
        {
            Debug.LogWarning($"Animation {stateName} timed out");
            yield break;
        }
        
        float animLength = transitionAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(animLength + fadeAnimationBuffer);
    }

    private IEnumerator ManualFade(float targetAlpha)
    {
        if (fadeOverlay == null) yield break;
        
        float startAlpha = fadeOverlay.color.a;
        float elapsed = 0f;
        
        while (elapsed < manualFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / manualFadeDuration);
            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, newAlpha);
            yield return null;
        }
        
        fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, targetAlpha);
        
        if (Mathf.Approximately(targetAlpha, 0f))
        {
            yield return new WaitForSecondsRealtime(0.1f);
            fadeOverlay.gameObject.SetActive(false);
        }
    }
}