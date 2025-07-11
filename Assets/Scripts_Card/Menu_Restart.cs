using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_Restart: MonoBehaviour
{
    // Llamado desde el botón "Nuevo Juego" en el menú
    public void StartNewGame()
    {
        // Reinicia la economía
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.ResetMoney();
        }
        else
        {
            Debug.LogWarning("EconomyManager.Instance no encontrado");
        }

        // Reinicia el estado del juego (cartas y mejoras)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameState();
        }
        else
        {
            Debug.LogWarning("GameManager.Instance no encontrado");
        }
            
        // Opcional: Borra otros PlayerPrefs si es necesario
        PlayerPrefs.DeleteKey("FirstRun"); // Para forzar reinicio en el próximo juego

        // Carga la primera escena del juego
        SceneManager.LoadScene("2_MainMenu");
    }

    // Llamado desde el botón "Continuar" (si aplica)
    public void ContinueGame(string contin_ue)
    {
        // Simplemente carga la escena del juego sin reiniciar
        SceneManager.LoadScene(contin_ue);
    }

    // Llamado desde el botón "Salir"
    public void QuitGame()
    {
        Application.Quit();
    }
}