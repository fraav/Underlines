using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuFadeReset : MonoBehaviour
{
    void Start()
    {
        // Verificar si estamos en el menú principal
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            ResetFadeState();
        }
    }

    private void ResetFadeState()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ResetFadeState();
        }
    }
}