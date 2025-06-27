using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class CardDisplay : MonoBehaviour, IPointerDownHandler
{
    [Header("Referencias Obligatorias")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button playButton;

    [Header("Configuración")]
    [SerializeField] private CanvasGroup canvasGroup;

    private CardData currentCard;

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
        PlayCard();
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
}