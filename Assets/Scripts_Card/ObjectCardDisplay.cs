using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class ObjectCardDisplay : MonoBehaviour, IPointerDownHandler
{
    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject selectionIndicator;

    public ObjectCardData currentCard;
    private CanvasGroup canvasGroup;
    private ObjectCard objectCardVisual;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        objectCardVisual = GetComponent<ObjectCard>();
        if (selectionIndicator != null) selectionIndicator.SetActive(false);
    }

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Battle"))
            Destroy(gameObject);
    }

    public void Initialize(ObjectCardData card)
    {
        currentCard = card;
        UpdateDisplay();

        if (GameManager.Instance != null)
        {
            bool interactable = GameManager.Instance.currentTurn == GameManager.TurnState.PlayerTurn &&
                                GameManager.Instance.isBattleScene &&
                                !GameManager.Instance.AreInteractionsBlocked() &&
                                GameManager.Instance.CanAffordObjectEnergy(card);
            SetInteractableState(interactable);
        }
        else
        {
            SetInteractableState(true);
        }
    }

    public void SetInteractableState(bool interactable)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = interactable ? 1f : 0.6f;
        canvasGroup.blocksRaycasts = interactable;
        canvasGroup.interactable = interactable;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Solo clic derecho (info). El efecto se activa arrastrando al Player (ObjectCard).
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (GameManager.Instance != null && GameManager.Instance.AreInteractionsBlocked()) return;
        if (currentCard == null) return;
        Debug.Log($"[ObjectCardDisplay] {currentCard.cardName}: {currentCard.description}");
    }

    private void UpdateDisplay()
    {
        if (currentCard == null) return;
        if (icon != null && currentCard.icon != null) icon.sprite = currentCard.icon;
        if (titleText != null) titleText.text = currentCard.cardName;
        if (descriptionText != null) descriptionText.text = GetShortDescription();
    }

    private string GetShortDescription()
    {
        if (currentCard == null) return "";
        switch (currentCard.effectType)
        {
            case ObjectCardData.ObjectEffectType.DoubleNextAction:
                return "Duplica acción";
            case ObjectCardData.ObjectEffectType.Heal:
                float heal = (currentCard.baseValue + currentCard.individualBaseValueUpgrade) *
                    (GameManager.Instance != null ? GameManager.Instance.healMultiplier : 1f);
                return $"Curación: {heal:F0}";
            case ObjectCardData.ObjectEffectType.RestoreEnergy:
                return $"+{currentCard.restoreEnergyAmount} energía";
            case ObjectCardData.ObjectEffectType.DrawCard:
                return $"Roba {currentCard.drawCardCount}";
            default:
                return currentCard.description;
        }
    }
}
