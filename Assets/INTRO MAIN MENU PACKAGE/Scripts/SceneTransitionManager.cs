using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Transition Settings")]
    public Image fadePanel;
    public float fadeDuration = 1f;
    public bool fadeOnStart = true;
    public Color fadeColor = Color.white;

    public bool IsTransitioning { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            if (fadePanel == null) CreateFadePanel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CreateFadePanel()
    {
        GameObject canvasObj = new GameObject("TransitionCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);

        GameObject panelObj = new GameObject("FadePanel");
        panelObj.transform.SetParent(canvasObj.transform);
        fadePanel = panelObj.AddComponent<Image>();
        
        fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        fadePanel.rectTransform.anchorMin = Vector2.zero;
        fadePanel.rectTransform.anchorMax = Vector2.one;
        fadePanel.rectTransform.offsetMin = Vector2.zero;
        fadePanel.rectTransform.offsetMax = Vector2.zero;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadeOnStart) StartCoroutine(FadeIn());
    }

    public void ResetFadeState()
    {
        if (fadePanel != null)
        {
            fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        }
        IsTransitioning = false;
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(Transition(sceneName));
    }

    public IEnumerator LoadSceneWithCleanup(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        
        CleanupBattleObjects();
        
        SceneManager.LoadScene(sceneName);
    }

    private void CleanupBattleObjects()
    {
        CardDisplay[] cards = FindObjectsOfType<CardDisplay>(true);
        foreach (CardDisplay card in cards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        
        HandManager handManager = FindObjectOfType<HandManager>(true);
        if (handManager != null) Destroy(handManager.gameObject);
    }

    private IEnumerator Transition(string sceneName)
    {
        yield return FadeOut();
        SceneManager.LoadScene(sceneName);
    }

    public void PlayFadeOutOnSceneLoad()
    {
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut()
    {
        IsTransitioning = true;
        
        if (fadePanel == null) 
        {
            IsTransitioning = false;
            yield break;
        }
        
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        
        IsTransitioning = false;
    }

    public IEnumerator FadeIn()
    {
        IsTransitioning = true;
        
        if (fadePanel == null) 
        {
            IsTransitioning = false;
            yield break;
        }
        
        float timer = fadeDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        
        IsTransitioning = false;
    }

    public void TransitionToQuit()
    {
        StartCoroutine(QuitGame());
    }

    private IEnumerator QuitGame()
    {
        yield return FadeOut();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}