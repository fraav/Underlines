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
    
    [Header("Descripción Emergente")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TMP_Text fullDescriptionText;

    [Header("Configuración")]
    [SerializeField] private CanvasGroup canvasGroup;

    private CardData currentCard;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Coroutine moveCoroutine;
    private bool isBeingDiscarded = false;

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
        
        // Ocultar panel de descripción al inicio
        if (descriptionPanel != null) 
        {
            descriptionPanel.SetActive(false);
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

        if (fullDescriptionText != null)
        {
            fullDescriptionText.text = GetFullDescription(card);
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void SetInteractableState(bool interactable)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = interactable ? 1f : 0.6f;
            canvasGroup.blocksRaycasts = interactable;
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

    public void OnPointerDown(PointerEventData eventData)
    {
        // Clic izquierdo: jugar carta
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            PlayCard();
        }
        // Clic derecho: mostrar/ocultar descripción
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ToggleDescription();
        }
    }

    private void ToggleDescription()
    {
        if (descriptionPanel == null || isBeingDiscarded) return;
        
        // Alternar visibilidad del panel
        bool isActive = !descriptionPanel.activeSelf;
        descriptionPanel.SetActive(isActive);
        
        // Si estamos activando, traer al frente
        if (isActive)
        {
            transform.SetAsLastSibling();
        }
    }
    
    private string GetFullDescription(CardData card)
    {
        if (GameManager.Instance == null) return card.description;
        
        float upgradedBase = card.baseValue + card.individualBaseValueUpgrade;
        float finalValue = 0f;
        string valueType = "";
        string typeName = "";

        switch (card.cardType)
        {
            case CardData.CardType.Attack:
                finalValue = upgradedBase * GameManager.Instance.damageMultiplier * card.individualDamageMultiplier;
                valueType = "Daño";
                typeName = "Ataque";
                break;
            case CardData.CardType.Block:
                finalValue = upgradedBase * GameManager.Instance.blockMultiplier;
                valueType = "Bloqueo";
                typeName = "Defensa";
                break;
            case CardData.CardType.Heal:
                finalValue = upgradedBase * GameManager.Instance.healMultiplier;
                valueType = "Curación";
                typeName = "Curación";
                break;
            default:
                return card.description;
        }

        return $"<b>{card.cardName}</b>\n\n" +
               $"{card.description}\n\n" +
               $"{valueType}: <color=#FFD700>{finalValue.ToString("F1")}</color>\n" +
               $"Valor Base: {upgradedBase}\n" +
               $"<i>Tipo: {typeName}</i>";
    }

    public void PlayCard()
    {
        // Prevenir múltiples llamadas durante la animación
        if (isBeingDiscarded) return;

        // Solo permitir jugar durante el turno del jugador
        if (GameManager.Instance.currentTurn != GameManager.TurnState.PlayerTurn)
        {
            Debug.LogWarning("¡No es el turno del jugador!");
            return;
        }

        if (currentCard == null || GameManager.Instance == null) return;

        // Deshabilitar interacción con todas las cartas
        HandManager.Instance.SetInteractable(false);

        // Iniciar animación de descarte de la carta
        StartCoroutine(AnimateDiscard());

        // Ejecutar la lógica de la carta
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
    }

    public void MoveToFanPosition(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        // Detener cualquier movimiento en curso
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        // Solo mover si el objeto está activo
        if (gameObject.activeInHierarchy)
        {
            moveCoroutine = StartCoroutine(SmoothMove(targetPosition, targetRotation, duration));
        }
        else
        {
            // Si está inactivo, establecer posición directamente
            rectTransform.localPosition = targetPosition;
            rectTransform.localRotation = targetRotation;
            originalPosition = targetPosition;
            originalRotation = targetRotation;
        }
    }

    private IEnumerator SmoothMove(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        float elapsed = 0f;
        Vector3 startPosition = rectTransform.localPosition;
        Quaternion startRotation = rectTransform.localRotation;

        while (elapsed < duration)
        {
            // Verificar si el objeto sigue activo
            if (!gameObject.activeInHierarchy) yield break;

            float t = elapsed / duration;
            rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            rectTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Verificar nuevamente antes de la posición final
        if (gameObject.activeInHierarchy)
        {
            rectTransform.localPosition = targetPosition;
            rectTransform.localRotation = targetRotation;
            originalPosition = targetPosition;
            originalRotation = targetRotation;
        }
    }

    IEnumerator AnimateDiscard()
    {
        isBeingDiscarded = true;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
        
        // Ocultar descripción si está visible
        if (descriptionPanel != null && descriptionPanel.activeSelf)
        {
            descriptionPanel.SetActive(false);
        }

        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            // Salir si el objeto se desactiva
            if (!gameObject.activeInHierarchy) yield break;

            float progress = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Destruir solo si sigue activo
        if (gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
        }
    }
}