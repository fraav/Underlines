using UnityEngine;
using TMPro;
using System.Collections;

public class HealthChangeText : MonoBehaviour
{
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private Vector3 startPosition;
    private Coroutine animationCoroutine;
    
    void Awake()
    {
        // Obtener o crear componentes si no existen
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();
            
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            
        if (textComponent == null)
            textComponent = gameObject.AddComponent<TextMeshProUGUI>();
            
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        // Configurar el CanvasGroup
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void ShowText(int amount, Color color, string prefix)
    {
        if (textComponent == null) return;

        // Configurar el texto
        textComponent.text = $"{prefix}{amount}";
        textComponent.color = color;
        
        // Guardar posición inicial
        startPosition = transform.position;
        
        // Iniciar animación
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
            
        animationCoroutine = StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        var manager = HealthChangeTextManager.Instance;
        if (manager == null)
        {
            Debug.LogError("HealthChangeText: Manager es null!");
            yield break;
        }
        
        // Obtener parámetros del manager
        float lifetime = manager.GetTextLifetime();
        float fadeTime = manager.GetTextFadeTime();
        float floatDistance = manager.GetTextFloatDistance();
        float floatSpeed = manager.GetTextFloatSpeed();
        Vector3 floatDirection = manager.GetTextFloatDirection();
        bool useCurve = manager.GetUseCurve();
        AnimationCurve floatCurve = manager.GetFloatCurve();
        
        // Fase 1: Aparecer y flotar
        float elapsedTime = 0f;
        float floatDuration = lifetime - fadeTime;
        
        while (elapsedTime < floatDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / floatDuration;
            
            // Aplicar curva si está habilitada
            if (useCurve)
            {
                progress = floatCurve.Evaluate(progress);
            }
            
            // Mover en la dirección especificada
            Vector3 newPosition = startPosition + (floatDirection * (floatDistance * progress));
            transform.position = newPosition;
            
            yield return null;
        }
        
        // Fase 2: Desvanecer en la posición final
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeTime)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = 1f - (fadeElapsed / fadeTime);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
            else if (textComponent != null)
            {
                textComponent.alpha = alpha;
            }
                
            yield return null;
        }
        
        // Ocultar completamente
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        else if (textComponent != null)
            textComponent.alpha = 0f;
        
        // Devolver al pool usando el manager
        if (manager != null)
        {
            manager.ReturnTextToPool(gameObject);
        }
        else
        {
            Debug.LogError("HealthChangeText: Manager es null al devolver al pool!");
        }
    }

    public void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    void OnDisable()
    {
        StopAnimation();
    }
}
