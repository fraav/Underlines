using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInitializer : MonoBehaviour
{
    void Start()
    {
        // Verificar si hay una transición en curso
        if (SceneTransitionManager.Instance != null && 
            !SceneTransitionManager.Instance.IsTransitioning)
        {
            // Realizar cualquier inicialización necesaria
            InitializeScene();
        }
    }

    private void InitializeScene()
    {
        // Lógica de inicialización específica de la escena
        Debug.Log("Inicializando escena: " + SceneManager.GetActiveScene().name);
        
        // Ejemplo: Buscar referencias importantes
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("No se encontró el jugador en la escena");
        }
    }
}