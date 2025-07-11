using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

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

    private CardData currentCard;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Coroutine moveCoroutine;
    private bool isBeingDiscarded = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (selectionIndicator != null) selectionIndicator.SetActive(false);
    }

    public void Initialize(CardData card)
    {
        currentCard = card;
        UpdateCardDisplay();
        SetInteractableState(true);
    }

    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null && !isBeingDiscarded)
        {
            selectionIndicator.SetActive(selected);
        }

        transform.localScale = selected ? Vector3.one * selectedScale : Vector3.one;
    }

    public void SetInteractableState(bool interactable)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = interactable ? 1f : 0.6f;
            canvasGroup.blocksRaycasts = interactable;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isBeingDiscarded) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (GameManager.Instance.currentTurn == GameManager.TurnState.SelectingTarget &&
                GameManager.Instance.selectedCard == currentCard)
            {
                GameManager.Instance.CancelSelection();
                return;
            }

            SelectCard();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ToggleDescription();
        }
    }

    private void SelectCard()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.currentTurn != GameManager.TurnState.PlayerTurn)
        {
            return;
        }

        GameManager.Instance.StartTargetSelection(currentCard, this);
    }

    private void ToggleDescription()
    {
        if (descriptionPanel == null) return;
        descriptionPanel.SetActive(!descriptionPanel.activeSelf);
        if (descriptionPanel.activeSelf) transform.SetAsLastSibling();
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

    public void MoveToFanPosition(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(SmoothMove(targetPosition, targetRotation, duration));
    }

    private IEnumerator SmoothMove(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        Vector3 startPosition = rectTransform.localPosition;
        Quaternion startRotation = rectTransform.localRotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            rectTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.localPosition = targetPosition;
        rectTransform.localRotation = targetRotation;
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
        float finalValue = 0f;

        switch (currentCard.cardType)
        {
            case CardData.CardType.Attack:
                finalValue = upgradedValue * GameManager.Instance.damageMultiplier * currentCard.individualDamageMultiplier;
                return $"Damage: {finalValue:F1}";
            case CardData.CardType.Block:
                finalValue = upgradedValue * GameManager.Instance.blockMultiplier;
                // Mostrar como porcentaje de reducción
                return $"Reduce: {finalValue:F0}%";
            case CardData.CardType.Heal:
                finalValue = upgradedValue * GameManager.Instance.healMultiplier;
                return $"Heal: {finalValue:F1}";
            default:
                return currentCard.description;
        }
    }

    private string GetFullDescription()
    {
        if (currentCard == null) return "";

        float upgradedValue = currentCard.baseValue + currentCard.individualBaseValueUpgrade;
        float finalValue = 0f;

        switch (currentCard.cardType)
        {
            case CardData.CardType.Attack:
                finalValue = upgradedValue * GameManager.Instance.damageMultiplier * currentCard.individualDamageMultiplier;
                return $"<b>{currentCard.cardName}</b>\n\n" +
                       $"{currentCard.description}\n\n" +
                       $"Damage: <color=#FFD700>{finalValue:F1}</color>";
            case CardData.CardType.Block:
                finalValue = upgradedValue * GameManager.Instance.blockMultiplier;
                // Descripción mejorada para bloqueo
                return $"<b>{currentCard.cardName}</b>\n\n" +
                       $"{currentCard.description}\n\n" +
                       $"Reduces enemy attack by <color=#FFD700>{finalValue:F0}%</color> " +
                       $"on their next turn";
            case CardData.CardType.Heal:
                finalValue = upgradedValue * GameManager.Instance.healMultiplier;
                return $"<b>{currentCard.cardName}</b>\n\n" +
                       $"{currentCard.description}\n\n" +
                       $"Heal: <color=#FFD700>{finalValue:F1}</color>";
            default:
                return currentCard.description;
        }
    }
}