using UnityEngine;
using System.Collections;

public class SceneInitializer : MonoBehaviour
{
    IEnumerator Start()
    {
        // Esperar 2 frames para asegurar inicialización
        yield return null;
        yield return null;
        
        if (SceneTransitionManager.Instance != null)
        {
            // Solo activar FadeOut si no hay transición activa
            if (!SceneTransitionManager.Instance.IsTransitioning)
            {
                SceneTransitionManager.Instance.PlayFadeOutOnSceneLoad();
            }
        }
    }
}