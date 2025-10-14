using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    
    [Header("Referencias")]
    public GameObject transitionCanvas;
    public RectTransform leftDoor;
    public RectTransform rightDoor;
    public Image fadePanel;
    
    [Header("Configuración")]
    public float transitionDuration = 1.5f;
    public float closedStateDuration = 0.5f;
    
    private Vector2 leftDoorOpenPos;
    private Vector2 leftDoorClosedPos;
    private Vector2 rightDoorOpenPos;
    private Vector2 rightDoorClosedPos;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        InitializeDoors();
    }
    
    private void InitializeDoors()
    {
        // ►►► POSICIONES CORREGIDAS PARA 1920x1080 ◄◄◄
        // Cada puerta mide 960px de ancho (mitad de 1920)
        
        // POSICIONES ABIERTAS (fuera de pantalla)
        leftDoorOpenPos = new Vector2(-1442f, 0f);    // Izquierda completamente fuera
        rightDoorOpenPos = new Vector2(1442f, 0f);     // Derecha completamente fuera
        
        // POSICIONES CERRADAS (cubriendo toda la pantalla)
        // LeftDoor se mueve a: centro - 480px (mitad de su ancho) = 0
        // RightDoor se mueve a: centro + 480px (mitad de su ancho) = 0  
        // Pero como tienen pivot al centro, necesitan moverse para que sus bordes se toquen
        leftDoorClosedPos = new Vector2(-480f, 0f);   // LeftDoor: su borde derecho en x=0
        rightDoorClosedPos = new Vector2(480f, 0f);    // RightDoor: su borde izquierdo en x=0
        
        // Posicionar puertas en sus posiciones iniciales
        if (leftDoor != null) leftDoor.anchoredPosition = leftDoorOpenPos;
        if (rightDoor != null) rightDoor.anchoredPosition = rightDoorOpenPos;
    }
    
    public IEnumerator FadeOut()
    {
        if (fadePanel != null)
        {
            float elapsedTime = 0f;
            Color startColor = Color.clear;
            Color endColor = Color.black;
            
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / transitionDuration;
                fadePanel.color = Color.Lerp(startColor, endColor, progress);
                yield return null;
            }
            fadePanel.color = endColor;
        }
        else
        {
            yield return StartCoroutine(CloseDoors());
        }
    }
    
    public IEnumerator FadeIn()
    {
        if (fadePanel != null)
        {
            float elapsedTime = 0f;
            Color startColor = fadePanel.color;
            Color endColor = Color.clear;
            
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / transitionDuration;
                fadePanel.color = Color.Lerp(startColor, endColor, progress);
                yield return null;
            }
            fadePanel.color = endColor;
        }
        else
        {
            yield return StartCoroutine(OpenDoors());
        }
    }
    
    public IEnumerator TransitionWithDoors(string sceneName)
    {
        if (leftDoor != null && rightDoor != null)
        {
            // Cerrar puertas
            yield return StartCoroutine(CloseDoors());
            
            // Cargar escena
            SceneManager.LoadScene(sceneName);
            yield return null;
            
            // Mantener cerrado por un momento
            yield return new WaitForSeconds(closedStateDuration);
            
            // Abrir puertas
            yield return StartCoroutine(OpenDoors());
        }
        else
        {
            // Fallback a fade normal
            yield return FadeOut();
            SceneManager.LoadScene(sceneName);
            yield return null;
            yield return FadeIn();
        }
    }
    
    public void TransitionToQuit()
    {
        StartCoroutine(QuitSequence());
    }
    
    private IEnumerator QuitSequence()
    {
        yield return FadeOut();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionWithDoors(sceneName));
    }
    
    public void TransitionToScene(int sceneBuildIndex)
    {
        string sceneName = SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name;
        TransitionToScene(sceneName);
    }
    
    private IEnumerator CloseDoors()
    {
        if (leftDoor == null || rightDoor == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;
            float easedProgress = EaseInOutCubic(progress);
            
            // Mover puertas hacia sus posiciones cerradas
            leftDoor.anchoredPosition = Vector2.Lerp(leftDoorOpenPos, leftDoorClosedPos, easedProgress);
            rightDoor.anchoredPosition = Vector2.Lerp(rightDoorOpenPos, rightDoorClosedPos, easedProgress);
            
            yield return null;
        }
        
        // Asegurar posición final exacta
        leftDoor.anchoredPosition = leftDoorClosedPos;
        rightDoor.anchoredPosition = rightDoorClosedPos;
    }
    
    private IEnumerator OpenDoors()
    {
        if (leftDoor == null || rightDoor == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;
            float easedProgress = EaseInOutCubic(progress);
            
            // Mover puertas hacia sus posiciones abiertas
            leftDoor.anchoredPosition = Vector2.Lerp(leftDoorClosedPos, leftDoorOpenPos, easedProgress);
            rightDoor.anchoredPosition = Vector2.Lerp(rightDoorClosedPos, rightDoorOpenPos, easedProgress);
            
            yield return null;
        }
        
        // Asegurar posición final exacta
        leftDoor.anchoredPosition = leftDoorOpenPos;
        rightDoor.anchoredPosition = rightDoorOpenPos;
    }
    
    private float EaseInOutCubic(float x)
    {
        return x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
    }
    
    // ►►► MÉTODO PARA PROBAR LA ANIMACIÓN MANUALMENTE ◄◄◄
    [ContextMenu("Probar Cerrar Puertas")]
    public void TestCloseDoors()
    {
        StartCoroutine(CloseDoors());
    }
    
    [ContextMenu("Probar Abrir Puertas")]
    public void TestOpenDoors()
    {
        StartCoroutine(OpenDoors());
    }
    
    // ►►► MÉTODO PARA VER POSICIONES ACTUALES ◄◄◄
    [ContextMenu("Debug Posiciones")]
    public void DebugPositions()
    {
        Debug.Log($"LeftDoor: {leftDoor.anchoredPosition}");
        Debug.Log($"RightDoor: {rightDoor.anchoredPosition}");
    }
}