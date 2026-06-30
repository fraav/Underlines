using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class CardDisplay : MonoBehaviour, IPointerDownHandler
{
    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TMP_Text fullDescriptionText;
    [SerializeField] private GameObject selectionIndicator;
    [SerializeField] private float selectedScale = 1.1f;

    public CardData currentCard;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private bool isBeingDiscarded = false;
    
    // Card visual effects reference
    private Card cardVisualEffects;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (selectionIndicator != null) selectionIndicator.SetActive(false);
        
        // Get Card visual effects component
        cardVisualEffects = GetComponent<Card>();
    }

    void Start()
    {
        // Verificación mejorada para todas las escenas de batalla
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Battle"))
        {
            Debug.LogWarning($"Destruyendo CardDisplay en escena incorrecta: {sceneName}");
            Destroy(gameObject);
        }
    }

    public void Initialize(CardData card)
    {
        currentCard = card;
        UpdateCardDisplay();
        
        // CRÍTICO: Establecer el estado de interacción basado en el estado actual del juego
        // Esto asegura que las cartas se creen con el estado correcto desde el inicio
        if (GameManager.Instance != null)
        {
            bool shouldBeInteractable = GameManager.Instance.currentTurn == GameManager.TurnState.PlayerTurn &&
                                      GameManager.Instance.isBattleScene &&
                                      !GameManager.Instance.AreInteractionsBlocked() &&
                                      GameManager.Instance.CanAffordEnergy(card);
            SetInteractableState(shouldBeInteractable);
        }
        else
        {
            // Fallback si GameManager no está disponible
            SetInteractableState(true);
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null && !isBeingDiscarded)
        {
            selectionIndicator.SetActive(selected);
        }

        // Use Card visual effects if available
        if (cardVisualEffects != null)
        {
            cardVisualEffects.SetSelected(selected);
        }
        else
        {
            transform.localScale = selected ? Vector3.one * selectedScale : Vector3.one;
        }
    }

    public void SetInteractableState(bool interactable)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = interactable ? 1f : 0.6f;
            canvasGroup.blocksRaycasts = interactable;
            canvasGroup.interactable = interactable;
            
            Debug.Log($"[CardDisplay] SetInteractableState({interactable}) para carta {currentCard?.cardName} - blocksRaycasts: {canvasGroup.blocksRaycasts}, interactable: {canvasGroup.interactable}");
        }
        else
        {
            Debug.LogWarning($"[CardDisplay] CanvasGroup es null para carta {currentCard?.cardName}");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isBeingDiscarded) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ShowCardInfoPanel();
        }
    }


    private void ShowCardInfoPanel()
    {
        if (currentCard == null) return;

        // Verificar si las interacciones están bloqueadas
        if (GameManager.Instance != null && GameManager.Instance.AreInteractionsBlocked())
            return;

        // Mostrar el panel de información de la carta
        if (CardInfoPanel.Instance != null)
        {
            CardInfoPanel.Instance.ShowCardInfo(currentCard);
        }
        else
        {
            Debug.LogWarning("CardInfoPanel.Instance no encontrado");
        }
    }


    public void DiscardCard()
    {
        if (isBeingDiscarded) return;
        StartCoroutine(DiscardAnimation());
    }

    private IEnumerator DiscardAnimation()
    {
        isBeingDiscarded = true;
        canvasGroup.blocksRaycasts = false;
        if (descriptionPanel != null) descriptionPanel.SetActive(false);

        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void UpdateCardDisplay()
    {
        if (icon != null && currentCard.icon != null) icon.sprite = currentCard.icon;
        if (titleText != null) titleText.text = currentCard.cardName;
        if (descriptionText != null) descriptionText.text = GetShortDescription();
        if (fullDescriptionText != null) fullDescriptionText.text = GetFullDescription();
    }

    private string GetShortDescription()
    {
        if (GameManager.Instance == null) return currentCard.description;

        float upgradedValue = currentCard.baseValue + currentCard.individualBaseValueUpgrade;

        switch (currentCard.cardType)
        {
            case CardData.CardType.Attack:
                float attackValue = upgradedValue * GameManager.Instance.damageMultiplier * currentCard.individualDamageMultiplier;
                return $"Damage: {attackValue:F1}";
            case CardData.CardType.Block:
                float blockValue = upgradedValue * GameManager.Instance.blockMultiplier;
                string blockDesc = $"Reduce: {blockValue:F0}%";
                if (currentCard.blockBonusType == CardData.BlockBonusType.RewardHealOnBlock)
                    blockDesc += $" | +{currentCard.blockHealReward:F0} HP al bloquear";
                else if (currentCard.blockBonusType == CardData.BlockBonusType.CounterDamageOnBlock)
                    blockDesc += $" | {currentCard.blockCounterDamage:F0} daño al bloquear";
                return blockDesc;
            case CardData.CardType.Heal:
                float healValue = upgradedValue * GameManager.Instance.healMultiplier;
                return $"Heal: {healValue:F1}";
            case CardData.CardType.Booster:
                return GetBoosterDescription();
            default:
                return currentCard.description;
        }
    }

    private string GetFullDescription()
    {
        if (currentCard == null) return "";

        float upgradedValue = currentCard.baseValue + currentCard.individualBaseValueUpgrade;

        switch (currentCard.cardType)
        {
            case CardData.CardType.Attack:
                float attackValue = upgradedValue * GameManager.Instance.damageMultiplier * currentCard.individualDamageMultiplier;
                return $"<b>{currentCard.cardName}</b>\n\n" +
                       $"{currentCard.description}\n\n" +
                       $"Damage: <color=#FFD700>{attackValue:F1}</color>";
            case CardData.CardType.Block:
                float blockValue = upgradedValue * GameManager.Instance.blockMultiplier;
                string bonusText = GetBlockBonusDescription();
                return $"<b>{currentCard.cardName}</b>\n\n" +
                       $"{currentCard.description}\n\n" +
                       $"Reduces enemy attack by <color=#FFD700>{blockValue:F0}%</color> " +
                       $"on their next turn" +
                       (string.IsNullOrEmpty(bonusText) ? "" : $"\n<color=#FFD700>{bonusText}</color>");
            case CardData.CardType.Heal:
                float healValue = upgradedValue * GameManager.Instance.healMultiplier;
                return $"<b>{currentCard.cardName}</b>\n\n" +
                       $"{currentCard.description}\n\n" +
                       $"Heal: <color=#FFD700>{healValue:F1}</color>";
            case CardData.CardType.Booster:
                return $"<b>{currentCard.cardName}</b>\n\n" +
                       $"{currentCard.description}\n\n" +
                       $"Efecto: <color=#FFD700>{GetBoosterFullDescription()}</color>";
            default:
                return currentCard.description;
        }
    }

    private string GetBlockBonusDescription()
    {
        if (currentCard == null || currentCard.cardType != CardData.CardType.Block)
            return "";

        switch (currentCard.blockBonusType)
        {
            case CardData.BlockBonusType.RewardHealOnBlock:
                return $"Si bloqueas un ataque: recuperas {currentCard.blockHealReward:F0} de vida";
            case CardData.BlockBonusType.CounterDamageOnBlock:
                return $"Si bloqueas un ataque: infliges {currentCard.blockCounterDamage:F0} de daño al enemigo";
            default:
                return "";
        }
    }

    private string GetBoosterDescription()
    {
        if (currentCard.cardType != CardData.CardType.Booster) return "";
        
        switch (currentCard.boosterEffectType)
        {
            case CardData.BoosterEffectType.DoubleAction:
                return "Duplica acción";
            case CardData.BoosterEffectType.IncreaseDamage:
                return $"Daño +{currentCard.boosterValue * 100}%";
            case CardData.BoosterEffectType.IncreaseBlock:
                return $"Bloqueo +{currentCard.boosterValue * 100}%";
            case CardData.BoosterEffectType.IncreaseHeal:
                return $"Curación +{currentCard.boosterValue * 100}%";
            case CardData.BoosterEffectType.Heal:
                return $"Curación instantánea";
            default:
                return currentCard.description;
        }
    }

    private string GetBoosterFullDescription()
    {
        if (currentCard.cardType != CardData.CardType.Booster) return "";
        
        switch (currentCard.boosterEffectType)
        {
            case CardData.BoosterEffectType.DoubleAction:
                return "Duplica la próxima acción ejecutada";
            case CardData.BoosterEffectType.IncreaseDamage:
                return $"Aumenta el daño en {currentCard.boosterValue * 100}%";
            case CardData.BoosterEffectType.IncreaseBlock:
                return $"Aumenta el bloqueo en {currentCard.boosterValue * 100}%";
            case CardData.BoosterEffectType.IncreaseHeal:
                return $"Aumenta la curación en {currentCard.boosterValue * 100}%";
            case CardData.BoosterEffectType.Heal:
            {
                float healValue = (currentCard.baseValue + currentCard.individualBaseValueUpgrade) *
                    (GameManager.Instance != null ? GameManager.Instance.healMultiplier : 1f);
                return $"Restaura {healValue:F0} de vida al jugarla";
            }
            default:
                return "Efecto desconocido";
        }
    }
}