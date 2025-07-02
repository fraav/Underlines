using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class CardDisplay : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Referencias Obligatorias")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button playButton;

    [Header("Configuración")]
    [SerializeField] private CanvasGroup canvasGroup;

    private CardData currentCard;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private bool isDragging = false;
    private Coroutine moveCoroutine;

    void Start()
    {
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(CardData card)
    {
        if (card == null)
        {
            Debug.LogError("Se intentó inicializar carta con CardData nulo");
            return;
        }

        currentCard = card;

        if (icon != null && card.icon != null)
        {
            icon.sprite = card.icon;
        }

        if (titleText != null)
        {
            titleText.text = card.cardName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = GetDescription(card);
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(PlayCard);
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    private string GetDescription(CardData card)
    {
        if (GameManager.Instance == null)
        {
            return card.description;
        }

        float upgradedBase = card.baseValue + card.individualBaseValueUpgrade;
        float finalValue = 0f;
        string valueType = "";

        switch (card.cardType)
        {
            case CardData.CardType.Attack:
                finalValue = upgradedBase * GameManager.Instance.damageMultiplier * card.individualDamageMultiplier;
                valueType = "Daño";
                break;
            case CardData.CardType.Block:
                finalValue = upgradedBase * GameManager.Instance.blockMultiplier;
                valueType = "Bloqueo";
                break;
            case CardData.CardType.Heal:
                finalValue = upgradedBase * GameManager.Instance.healMultiplier;
                valueType = "Curación";
                break;
            default:
                return card.description;
        }

        return $"{valueType}: {finalValue.ToString("F1")}\n" +
               $"Base: {upgradedBase}";
    }

    public void PlayCard()
    {
        if (currentCard == null || GameManager.Instance == null) return;

        switch (currentCard.cardType)
        {
            case CardData.CardType.Attack:
                GameManager.Instance.Card_Attack(currentCard);
                break;
            case CardData.CardType.Block:
                GameManager.Instance.Card_Block(currentCard);
                break;
            case CardData.CardType.Heal:
                GameManager.Instance.Card_Heal(currentCard);
                break;
        }

        GameManager.Instance.PlayCard(currentCard);
        StartCoroutine(AnimateDiscard());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Mantenemos vacío pero necesario para la interfaz
    }

    public void MoveToFanPosition(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(SmoothMove(targetPosition, targetRotation, duration));
    }

    private IEnumerator SmoothMove(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        float elapsed = 0f;
        Vector3 startPosition = rectTransform.localPosition;
        Quaternion startRotation = rectTransform.localRotation;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            rectTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.localPosition = targetPosition;
        rectTransform.localRotation = targetRotation;
        originalPosition = targetPosition;
        originalRotation = targetRotation;
    }

    IEnumerator AnimateDiscard()
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        originalPosition = rectTransform.localPosition;
        originalRotation = rectTransform.localRotation;
        originalParent = transform.parent;
        transform.SetParent(transform.root); // Sacamos de la jerarquía de la mano
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out pos
            );
            rectTransform.localPosition = new Vector3(pos.x, pos.y, 0);
            rectTransform.localRotation = Quaternion.identity;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        PlayArea playArea = FindObjectOfType<PlayArea>();
        if (playArea != null && playArea.IsPointInside(eventData.position))
        {
            PlayCard();
        }
        else
        {
            transform.SetParent(originalParent);
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(SmoothMove(originalPosition, originalRotation, 0.2f));
        }
    }
}