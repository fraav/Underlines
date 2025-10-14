using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    [Header("Configuración por Defecto")]
    public string targetSceneName;
    public int targetSceneBuildIndex = -1;
    
    public void TriggerTransition()
    {
        // Usar la configuración por defecto del Inspector
        if (targetSceneBuildIndex != -1)
        {
            SceneTransitionManager.Instance.TransitionToScene(targetSceneBuildIndex);
        }
        else if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName);
        }
        else
        {
            Debug.LogError("No se ha especificado una escena destino!");
        }
    }
    
    // ►►► NUEVOS MÉTODOS PARA ELEGIR ESCENA DESDE CÓDIGO ◄◄◄
    
    // Cambiar a escena por nombre
    public void TriggerTransitionToScene(string sceneName)
    {
        SceneTransitionManager.Instance.TransitionToScene(sceneName);
    }
    
    // Cambiar a escena por build index
    public void TriggerTransitionToScene(int buildIndex)
    {
        SceneTransitionManager.Instance.TransitionToScene(buildIndex);
    }
    
    // Cambiar a escena y luego ejecutar callback
    public void TriggerTransitionToScene(string sceneName, System.Action onComplete)
    {
        StartCoroutine(TransitionWithCallback(sceneName, onComplete));
    }
    
    public void TriggerTransitionToScene(int buildIndex, System.Action onComplete)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(buildIndex).name;
        StartCoroutine(TransitionWithCallback(sceneName, onComplete));
    }
    
    private System.Collections.IEnumerator TransitionWithCallback(string sceneName, System.Action onComplete)
    {
        // Esperar a que la transición esté disponible
        while (SceneTransitionManager.Instance == null)
        {
            yield return null;
        }
        
        // Realizar transición
        SceneTransitionManager.Instance.TransitionToScene(sceneName);
        
        // Ejecutar callback después de un breve delay
        yield return new WaitForSeconds(0.1f);
        onComplete?.Invoke();
    }
    
    // ►►► MÉTODOS PARA CONFIGURAR DESTINO DINÁMICAMENTE ◄◄◄
    
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
        targetSceneBuildIndex = -1; // Reset build index
    }
    
    public void SetTargetScene(int buildIndex)
    {
        targetSceneBuildIndex = buildIndex;
        targetSceneName = ""; // Reset scene name
    }
    
    // ►►► MÉTODOS DE USUARIO (colisiones, UI, etc.) ◄◄◄
    
    // Para usar con colisiones
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerTransition();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerTransition();
        }
    }
    
    // Para usar con clics de UI (usa la configuración por defecto)
    public void OnButtonClick()
    {
        TriggerTransition();
    }
    
    // Para usar con clics de UI especificando escena
    public void OnButtonClickToScene(string sceneName)
    {
        TriggerTransitionToScene(sceneName);
    }
    
    public void OnButtonClickToScene(int buildIndex)
    {
        TriggerTransitionToScene(buildIndex);
    }
    
    // ►►► MÉTODOS RÁPIDOS PARA ESCENAS COMUNES ◄◄◄
    
    public void GoToMainMenu()
    {
        TriggerTransitionToScene("MainMenu");
    }
    
    public void GoToBattleScene()
    {
        TriggerTransitionToScene("BattleScene");
    }
    
    public void GoToCredits()
    {
        TriggerTransitionToScene("Credits");
    }
    
    public void RestartCurrentScene()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        TriggerTransitionToScene(currentScene);
    }
    
    public void GoToNextScene()
    {
        int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int nextIndex = (currentIndex + 1) % UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        TriggerTransitionToScene(nextIndex);
    }
    
    public void GoToPreviousScene()
    {
        int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int previousIndex = (currentIndex - 1 + UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings) 
                          % UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        TriggerTransitionToScene(previousIndex);
    }
}