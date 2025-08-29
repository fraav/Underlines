using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardInfoManager : MonoBehaviour
{
    public static CardInfoManager Instance { get; private set; }

    [Header("Panel References")]
    [SerializeField] private GameObject cardInfoPanel;
    [SerializeField] private Image cardIcon;
    [SerializeField] private TMP_Text cardTitle;
    [SerializeField] private TMP_Text cardDescription;
    [SerializeField] private TMP_Text cardStats;
    
    [Header("Panel Settings")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    
    private bool isPanelActive = false;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePanel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePanel()
    {
        if (cardInfoPanel != null)
        {
            cardInfoPanel.SetActive(false);
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = cardInfoPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = cardInfoPanel.AddComponent<CanvasGroup>();
                }
            }
            panelCanvasGroup.alpha = 0f;
        }
    }

    public void ShowCardInfo(CardData cardData)
    {
        if (cardData == null || isPanelActive) return;

        // Actualizar la información de la carta
        if (cardIcon != null && cardData.icon != null)
        {
            cardIcon.sprite = cardData.icon;
        }
        
        if (cardTitle != null)
        {
            cardTitle.text = cardData.cardName;
        }
        
        if (cardDescription != null)
        {
            cardDescription.text = cardData.description;
        }
        
        if (cardStats != null)
        {
            cardStats.text = GetCardStatsText(cardData);
        }

        // Mostrar el panel con animación
        ShowPanel();
    }

    public void HideCardInfo()
    {
        if (!isPanelActive) return;
        HidePanel();
    }

    private void ShowPanel()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        cardInfoPanel.SetActive(true);
        fadeCoroutine = StartCoroutine(FadePanel(0f, 1f, fadeInDuration));
        isPanelActive = true;
        
        // Bloquear interacciones del juego
        SetGameInteractions(false);
    }

    private void HidePanel()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadePanel(1f, 0f, fadeOutDuration));
        isPanelActive = false;
        
        // Restaurar interacciones del juego
        SetGameInteractions(true);
    }

    private System.Collections.IEnumerator FadePanel(float startAlpha, float targetAlpha, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        panelCanvasGroup.alpha = targetAlpha;
        
        if (targetAlpha == 0f)
        {
            cardInfoPanel.SetActive(false);
        }
    }

    private string GetCardStatsText(CardData cardData)
    {
        if (GameManager.Instance == null) return "";

        float upgradedValue = cardData.baseValue + cardData.individualBaseValueUpgrade;

        switch (cardData.cardType)
        {
            case CardData.CardType.Attack:
                float attackValue = upgradedValue * GameManager.Instance.damageMultiplier * cardData.individualDamageMultiplier;
                return $"<color=#FF6B6B>Attack Damage:</color> <color=#FFD700>{attackValue:F1}</color>";
                
            case CardData.CardType.Block:
                float blockValue = upgradedValue * GameManager.Instance.blockMultiplier;
                return $"<color=#4ECDC4>Block Reduction:</color> <color=#FFD700>{blockValue:F0}%</color>";
                
            case CardData.CardType.Heal:
                float healValue = upgradedValue * GameManager.Instance.healMultiplier;
                return $"<color=#45B7D1>Healing:</color> <color=#FFD700>{healValue:F1}</color>";
                
            default:
                return "";
        }
    }

    private void SetGameInteractions(bool enabled)
    {
        // Aquí puedes agregar lógica para bloquear/desbloquear interacciones del juego
        // Por ejemplo, deshabilitar el input del jugador, etc.
        if (GameManager.Instance != null)
        {
            // Puedes agregar un estado en GameManager para controlar si el juego está pausado
            // GameManager.Instance.SetGamePaused(!enabled);
        }
    }

    public bool IsPanelActive()
    {
        return isPanelActive;
    }

    // Método para cerrar el panel con Escape o click derecho
    void Update()
    {
        if (isPanelActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                HideCardInfo();
            }
        }
    }
}
