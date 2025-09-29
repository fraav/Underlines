using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Simple fade manager: call FadeManager.Instance.FadeToScene("SceneName", fadeDuration, fadeSprite);
/// Or just FadeIn/Out with default color/image.
/// </summary>
public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("Configuration")]
    public float defaultDuration = 1f;
    public Image fadeImage;             // Assign in inspector: full‐screen Image (should cover whole screen)
    public Sprite defaultFadeSprite;    // Optional default sprite
    public Color defaultFadeColor = Color.black;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        if (fadeImage == null)
        {
            // Create Canvas and Image automatically
            var canvasGO = new GameObject("FadeCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            DontDestroyOnLoad(canvasGO);

            var imgGO = new GameObject("FadeImage");
            imgGO.transform.SetParent(canvasGO.transform);
            fadeImage = imgGO.AddComponent<Image>();
            var rt = fadeImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // Set defaults
        fadeImage.color = new Color(defaultFadeColor.r, defaultFadeColor.g, defaultFadeColor.b, 0);
        if (defaultFadeSprite != null)
            fadeImage.sprite = defaultFadeSprite;
    }

    /// <summary>
    /// Fade out then load new scene, then fade in.
    /// </summary>
    public void FadeToScene(string sceneName, float duration = -1f, Sprite fadeSprite = null)
    {
        if (duration < 0) duration = defaultDuration;
        StartCoroutine(Transition(sceneName, duration, fadeSprite));
    }

    private IEnumerator Transition(string sceneName, float duration, Sprite fadeSprite)
    {
        if (fadeSprite != null)
            fadeImage.sprite = fadeSprite;

        // Fade out
        yield return Fade(0f, 1f, duration / 2f);

        // Load scene asynchronously
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Fade in
        yield return Fade(1f, 0f, duration / 2f);
    }

    /// <summary>
    /// Manual fade out: yields when complete.
    /// </summary>
    public IEnumerator FadeOut(float duration = -1f, Sprite fadeSprite = null)
    {
        if (duration < 0) duration = defaultDuration;
        if (fadeSprite != null)
            fadeImage.sprite = fadeSprite;
        yield return Fade(0f, 1f, duration);
    }

    /// <summary>
    /// Manual fade in: yields when complete.
    /// </summary>
    public IEnumerator FadeIn(float duration = -1f)
    {
        if (duration < 0) duration = defaultDuration;
        yield return Fade(1f, 0f, duration);
    }

    private IEnumerator Fade(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadeImage.color = c;
            yield return null;
        }
        c.a = toAlpha;
        fadeImage.color = c;
    }
}
