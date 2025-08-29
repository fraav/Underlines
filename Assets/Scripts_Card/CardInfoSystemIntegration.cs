using UnityEngine;

/// <summary>
/// Script de ejemplo que muestra cómo integrar el sistema de información de cartas
/// en una escena existente de Unity.
/// </summary>
public class CardInfoSystemIntegration : MonoBehaviour
{
    [Header("Integration Settings")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Required Components")]
    [SerializeField] private CardInfoManager cardInfoManager;
    [SerializeField] private GameObject cardInfoPanelPrefab;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupCardInfoSystem();
        }
    }
    
    /// <summary>
    /// Configura automáticamente el sistema de información de cartas
    /// </summary>
    public void SetupCardInfoSystem()
    {
        if (showDebugLogs)
            Debug.Log("CardInfoSystemIntegration: Iniciando configuración del sistema...");
        
        // Paso 1: Verificar o crear el CardInfoManager
        SetupCardInfoManager();
        
        // Paso 2: Verificar o crear el panel de información
        SetupCardInfoPanel();
        
        // Paso 3: Verificar la integración
        VerifyIntegration();
        
        if (showDebugLogs)
            Debug.Log("CardInfoSystemIntegration: Configuración completada.");
    }
    
    private void SetupCardInfoManager()
    {
        if (cardInfoManager == null)
        {
            cardInfoManager = FindObjectOfType<CardInfoManager>();
        }
        
        if (cardInfoManager == null)
        {
            if (showDebugLogs)
                Debug.Log("CardInfoSystemIntegration: Creando CardInfoManager...");
            
            GameObject managerObj = new GameObject("CardInfoManager");
            cardInfoManager = managerObj.AddComponent<CardInfoManager>();
            
            // Opcional: Hacer que persista entre escenas
            DontDestroyOnLoad(managerObj);
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("CardInfoSystemIntegration: CardInfoManager encontrado en la escena.");
        }
    }
    
    private void SetupCardInfoPanel()
    {
        if (cardInfoPanelPrefab == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("CardInfoSystemIntegration: No se ha asignado un prefab para el panel. " +
                               "Necesitarás crear el panel manualmente en la escena.");
            return;
        }
        
        // Verificar si ya existe un panel en la escena
        CardInfoPanelPrefabSetup existingPanel = FindObjectOfType<CardInfoPanelPrefabSetup>();
        if (existingPanel == null)
        {
            if (showDebugLogs)
                Debug.Log("CardInfoSystemIntegration: Instanciando panel de información...");
            
            GameObject panelInstance = Instantiate(cardInfoPanelPrefab);
            panelInstance.name = "CardInfoPanel";
            
            // Asegurar que el panel esté en el Canvas correcto
            Canvas targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas != null)
            {
                panelInstance.transform.SetParent(targetCanvas.transform, false);
            }
            else
            {
                if (showDebugLogs)
                    Debug.LogWarning("CardInfoSystemIntegration: No se encontró Canvas en la escena. " +
                                   "El panel puede no mostrarse correctamente.");
            }
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("CardInfoSystemIntegration: Panel de información ya existe en la escena.");
        }
    }
    
    private void VerifyIntegration()
    {
        bool integrationSuccessful = true;
        string issues = "";
        
        // Verificar CardInfoManager
        if (cardInfoManager == null)
        {
            integrationSuccessful = false;
            issues += "- CardInfoManager no encontrado\n";
        }
        
        // Verificar que el panel esté configurado
        CardInfoPanelPrefabSetup panel = FindObjectOfType<CardInfoPanelPrefabSetup>();
        if (panel == null)
        {
            integrationSuccessful = false;
            issues += "- Panel de información no encontrado\n";
        }
        
        // Verificar que las cartas existentes funcionen
        CardDisplay[] existingCards = FindObjectsOfType<CardDisplay>();
        if (existingCards.Length == 0)
        {
            if (showDebugLogs)
                Debug.Log("CardInfoSystemIntegration: No se encontraron cartas en la escena.");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log($"CardInfoSystemIntegration: Se encontraron {existingCards.Length} cartas en la escena.");
        }
        
        if (integrationSuccessful)
        {
            if (showDebugLogs)
                Debug.Log("CardInfoSystemIntegration: Integración verificada exitosamente.");
        }
        else
        {
            Debug.LogError($"CardInfoSystemIntegration: Problemas de integración detectados:\n{issues}");
        }
    }
    
    /// <summary>
    /// Método público para probar el sistema manualmente
    /// </summary>
    [ContextMenu("Test Card Info System")]
    public void TestCardInfoSystem()
    {
        if (cardInfoManager != null)
        {
            // Crear datos de prueba
            CardData testCard = ScriptableObject.CreateInstance<CardData>();
            testCard.cardName = "Carta de Prueba";
            testCard.description = "Esta es una carta de prueba para verificar el sistema.";
            testCard.cardType = CardData.CardType.Attack;
            testCard.baseValue = 10f;
            
            // Mostrar la información de prueba
            cardInfoManager.ShowCardInfo(testCard);
            
            if (showDebugLogs)
                Debug.Log("CardInfoSystemIntegration: Mostrando carta de prueba...");
        }
        else
        {
            Debug.LogError("CardInfoSystemIntegration: No se puede probar el sistema - CardInfoManager no encontrado.");
        }
    }
    
    /// <summary>
    /// Método para limpiar el sistema (útil para testing)
    /// </summary>
    [ContextMenu("Cleanup Card Info System")]
    public void CleanupCardInfoSystem()
    {
        if (cardInfoManager != null)
        {
            cardInfoManager.HideCardInfo();
            if (showDebugLogs)
                Debug.Log("CardInfoSystemIntegration: Sistema limpiado.");
        }
    }
}
