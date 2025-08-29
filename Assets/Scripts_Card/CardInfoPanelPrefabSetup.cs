using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardInfoPanelPrefabSetup : MonoBehaviour
{
    [Header("Panel Components")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image cardIconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backgroundCloseButton;
    
    [Header("CardInfoManager Reference")]
    [SerializeField] private CardInfoManager cardInfoManager;
    
    void Awake()
    {
        SetupReferences();
        ConfigurePanel();
    }
    
    private void SetupReferences()
    {
        // Buscar componentes si no están asignados
        if (mainPanel == null)
            mainPanel = transform.Find("MainPanel")?.gameObject;
            
        if (backgroundImage == null)
            backgroundImage = transform.Find("Background")?.GetComponent<Image>();
            
        if (cardIconImage == null)
            cardIconImage = transform.Find("MainPanel/CardIcon")?.GetComponent<Image>();
            
        if (titleText == null)
            titleText = transform.Find("MainPanel/TitleText")?.GetComponent<TMP_Text>();
            
        if (descriptionText == null)
            descriptionText = transform.Find("MainPanel/DescriptionText")?.GetComponent<TMP_Text>();
            
        if (statsText == null)
            statsText = transform.Find("MainPanel/StatsText")?.GetComponent<TMP_Text>();
            
        if (closeButton == null)
            closeButton = transform.Find("MainPanel/CloseButton")?.GetComponent<Button>();
            
        if (backgroundCloseButton == null)
            backgroundCloseButton = transform.Find("Background")?.GetComponent<Button>();
    }
    
    private void ConfigurePanel()
    {
        // Configurar botón de cerrar principal
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => {
                if (cardInfoManager != null)
                {
                    cardInfoManager.HideCardInfo();
                }
            });
        }
        
        // Configurar botón de cerrar en el fondo (para cerrar haciendo click fuera del panel)
        if (backgroundCloseButton != null)
        {
            backgroundCloseButton.onClick.RemoveAllListeners();
            backgroundCloseButton.onClick.AddListener(() => {
                if (cardInfoManager != null)
                {
                    cardInfoManager.HideCardInfo();
                }
            });
        }
        
        // Configurar colores por defecto
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0, 0, 0, 0.8f);
        }
        
        if (titleText != null)
        {
            titleText.color = new Color(1f, 0.8f, 0.2f, 1f);
        }
        
        if (descriptionText != null)
        {
            descriptionText.color = Color.white;
        }
        
        if (statsText != null)
        {
            statsText.color = Color.white;
        }
    }
    
    void Start()
    {
        // Buscar el CardInfoManager en la escena si no está asignado
        if (cardInfoManager == null)
        {
            cardInfoManager = FindObjectOfType<CardInfoManager>();
        }
        
        // Asignar las referencias al CardInfoManager
        if (cardInfoManager != null)
        {
            // Usar reflexión para asignar las referencias privadas
            var managerType = typeof(CardInfoManager);
            var panelField = managerType.GetField("cardInfoPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var iconField = managerType.GetField("cardIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var titleField = managerType.GetField("cardTitle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descField = managerType.GetField("cardDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var statsField = managerType.GetField("cardStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var canvasGroupField = managerType.GetField("panelCanvasGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (panelField != null) panelField.SetValue(cardInfoManager, mainPanel);
            if (iconField != null) iconField.SetValue(cardInfoManager, cardIconImage);
            if (titleField != null) titleField.SetValue(cardInfoManager, titleText);
            if (descField != null) descField.SetValue(cardInfoManager, descriptionText);
            if (statsField != null) statsField.SetValue(cardInfoManager, statsText);
            if (canvasGroupField != null) canvasGroupField.SetValue(cardInfoManager, mainPanel?.GetComponent<CanvasGroup>());
        }
        else
        {
            Debug.LogError("CardInfoPanelPrefabSetup: No se pudo encontrar CardInfoManager en la escena!");
        }
    }
}
