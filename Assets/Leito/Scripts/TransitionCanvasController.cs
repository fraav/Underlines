using UnityEngine;

public class TransitionCanvasController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 2f;
    
    private bool isFadingIn;
    private bool isFadingOut;
    private float targetAlpha;

    void Update()
    {
        if (isFadingIn)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, fadeSpeed * Time.unscaledDeltaTime);
            if (Mathf.Approximately(canvasGroup.alpha, 1f))
            {
                isFadingIn = false;
            }
        }
        else if (isFadingOut)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeSpeed * Time.unscaledDeltaTime);
            if (Mathf.Approximately(canvasGroup.alpha, 0f))
            {
                isFadingOut = false;
                gameObject.SetActive(false);
            }
        }
    }

    public void StartFadeIn()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        isFadingIn = true;
        isFadingOut = false;
    }

    public void StartFadeOut()
    {
        canvasGroup.alpha = 1f;
        isFadingOut = true;
        isFadingIn = false;
    }
}