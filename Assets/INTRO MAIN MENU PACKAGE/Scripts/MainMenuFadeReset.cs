using UnityEngine;
using UnityEngine.UI;

public class MainMenuFadeReset : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private Color fadeColor = Color.black;

    void Start()
    {
        ResetFadeState();
        
        // Verificación adicional con el manager
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ResetFadeState();
        }
    }

    private void ResetFadeState()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
            fadeOverlay.gameObject.SetActive(false);
        }
    }
}