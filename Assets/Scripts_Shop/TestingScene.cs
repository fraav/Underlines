using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestingScene : MonoBehaviour
{
    /// <summary>
    /// Carga la siguiente escena aplicando el fade out/in si está disponible.
    /// </summary>
    public void LoadNextScene(string nextScene)
    {
        if (FadeManager.Instance != null)
        {
            // Esto iniciará el fade out, cargará la escena y hará fade in.
            FadeManager.Instance.FadeToScene(nextScene);
        }
        else
        {
            // Fallback sin fade
            SceneManager.LoadScene(nextScene);
        }
    }
}
