using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardInfoPanelSetup : MonoBehaviour
{
    [Header("Panel Configuration")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image cardIconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Button closeButton;
    
    [Header("Styling")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color titleColor = new Color(1f, 0.8f, 0.2f, 1f);
    
    void Awake()
    {
        SetupPanel();
    }
    
    private void SetupPanel()
    {
        // Configurar el CanvasGroup si no está asignado
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Configurar colores del panel
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
        
        // Configurar colores del texto
        if (titleText != null)
        {
            titleText.color = titleColor;
        }
        
        if (descriptionText != null)
        {
            descriptionText.color = textColor;
        }
        
        if (statsText != null)
        {
            statsText.color = textColor;
        }
        
        // Configurar botón de cerrar
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => {
                if (CardInfoManager.Instance != null)
                {
                    CardInfoManager.Instance.HideCardInfo();
                }
            });
        }
        
        // Asegurar que el panel esté desactivado al inicio
        gameObject.SetActive(false);
    }
    
    void Start()
    {
        // Verificar que el CardInfoManager esté configurado correctamente
        if (CardInfoManager.Instance == null)
        {
            Debug.LogError("CardInfoPanelSetup: CardInfoManager no encontrado en la escena!");
        }
    }
}
