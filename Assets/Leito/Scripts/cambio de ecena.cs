using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonSceneChanger : MonoBehaviour
{
    [Header("Configuración de Escena")]
    public string sceneName; // Nombre de la escena a cargar
    public int sceneIndex = -1; // Índice de la escena (opcional)

    private Button button;

    private void Start()
    {
        // Obtener el componente Button
        button = GetComponent<Button>();

        // Asignar el método al evento del botón
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    public void OnButtonClick()
    {
        // Verificar qué método usar (nombre o índice)
        if (!string.IsNullOrEmpty(sceneName))
        {
            LoadSceneByName();
        }
        else if (sceneIndex >= 0)
        {
            LoadSceneByIndex();
        }
        else
        {
            Debug.LogError("No se ha configurado nombre o índice de escena");
        }
    }

    private void LoadSceneByName()
    {
        Debug.Log("Cargando escena: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    private void LoadSceneByIndex()
    {
        Debug.Log("Cargando escena con índice: " + sceneIndex);
        SceneManager.LoadScene(sceneIndex);
    }
}