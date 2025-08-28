using UnityEngine;
using System.Collections.Generic;

public class HealthChangeTextManager : MonoBehaviour
{
    public static HealthChangeTextManager Instance { get; private set; }

    [Header("Text Prefab")]
    [SerializeField] private GameObject healthChangeTextPrefab;
    
    [Header("Animation Settings")]
    [SerializeField] private float textLifetime = 2f;
    [SerializeField] private float textFadeTime = 0.5f;
    [SerializeField] private float textFloatDistance = 50f;
    [SerializeField] private float textFloatSpeed = 1f;
    [SerializeField] private Vector3 textFloatDirection = Vector3.up;
    [SerializeField] private bool useCurve = false;
    [SerializeField] private AnimationCurve floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Position Settings")]
    [SerializeField] private Vector3 textOffset = Vector3.zero;
    [SerializeField] private bool randomizeOffset = false;
    [SerializeField] private float randomOffsetRange = 10f;
    
    [Header("Colors")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color healColor = Color.green;
    
    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 10;
    
    private Queue<GameObject> textPool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeTextPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeTextPool()
    {
        if (healthChangeTextPrefab == null)
        {
            Debug.LogError("HealthChangeTextPrefab no está asignado en HealthChangeTextManager!");
            return;
        }

        Debug.Log($"HealthChangeTextManager: Inicializando pool con {poolSize} objetos");
        
        // Llenar el pool
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewTextObject();
        }
        
        Debug.Log($"HealthChangeTextManager: Pool inicializado con {textPool.Count} objetos");
    }

    private void CreateNewTextObject()
    {
        if (healthChangeTextPrefab != null)
        {
            GameObject textObj = Instantiate(healthChangeTextPrefab, transform);
            textObj.SetActive(false);
            textPool.Enqueue(textObj);
        }
    }

    public void ShowHealthChange(Vector3 position, int amount, bool isHeal)
    {
        Debug.Log($"HealthChangeTextManager: Mostrando cambio de vida en {position}, cantidad: {amount}, es curación: {isHeal}");
        
        GameObject textObj = GetTextFromPool();
        if (textObj != null)
        {
            // Aplicar offset y randomización
            Vector3 finalPosition = position + textOffset;
            if (randomizeOffset)
            {
                finalPosition += new Vector3(
                    Random.Range(-randomOffsetRange, randomOffsetRange),
                    Random.Range(-randomOffsetRange, randomOffsetRange),
                    Random.Range(-randomOffsetRange, randomOffsetRange)
                );
            }
            
            textObj.transform.position = finalPosition;
            textObj.SetActive(true);
            
            var healthChangeText = textObj.GetComponent<HealthChangeText>();
            if (healthChangeText != null)
            {
                healthChangeText.ShowText(amount, isHeal ? healColor : damageColor, isHeal ? "+" : "-");
            }
            else
            {
                Debug.LogError("HealthChangeTextManager: HealthChangeText script no encontrado en el objeto del pool!");
            }
        }
        else
        {
            Debug.LogError("HealthChangeTextManager: No se pudo obtener objeto del pool!");
        }
    }

    private GameObject GetTextFromPool()
    {
        if (textPool.Count == 0)
        {
            Debug.LogWarning("HealthChangeTextManager: Pool vacío, creando nuevo objeto");
            CreateNewTextObject();
        }
        
        GameObject obj = textPool.Dequeue();
        Debug.Log($"HealthChangeTextManager: Objeto obtenido del pool. Objetos restantes: {textPool.Count}");
        return obj;
    }

    public void ReturnTextToPool(GameObject textObj)
    {
        if (textObj != null)
        {
            Debug.Log($"HealthChangeTextManager: Devolviendo objeto al pool. Objetos en pool: {textPool.Count}");
            textObj.SetActive(false);
            textObj.transform.SetParent(transform);
            textPool.Enqueue(textObj);
            Debug.Log($"HealthChangeTextManager: Objeto devuelto al pool. Total en pool: {textPool.Count}");
        }
        else
        {
            Debug.LogError("HealthChangeTextManager: Intentando devolver objeto null al pool!");
        }
    }

    public void SetColors(Color damage, Color heal)
    {
        damageColor = damage;
        healColor = heal;
    }

    public void SetTextSettings(float lifetime, float fadeTime, float floatDistance)
    {
        textLifetime = lifetime;
        textFadeTime = fadeTime;
        textFloatDistance = floatDistance;
    }

    // Métodos para obtener los parámetros de animación
    public float GetTextLifetime() => textLifetime;
    public float GetTextFadeTime() => textFadeTime;
    public float GetTextFloatDistance() => textFloatDistance;
    public float GetTextFloatSpeed() => textFloatSpeed;
    public Vector3 GetTextFloatDirection() => textFloatDirection;
    public bool GetUseCurve() => useCurve;
    public AnimationCurve GetFloatCurve() => floatCurve;
}
