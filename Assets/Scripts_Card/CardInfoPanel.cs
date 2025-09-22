using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardInfoPanel : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject panelObject;
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardDescriptionText;
    [SerializeField] private TMP_Text cardEffectText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backgroundCloseButton; // Para cerrar al hacer click fuera del panel

    [Header("Panel Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve scaleAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isOpen = false;
    private Coroutine animationCoroutine;
    private CanvasGroup canvasGroup;

    public static CardInfoPanel Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetupButtons();
        // Inicializar panel como cerrado
        if (panelObject != null)
            panelObject.SetActive(false);
    }

    void Start()
    {
        // Asegurar que el panel esté cerrado al inicio
        if (panelObject != null)
            panelObject.SetActive(false);
    }

    private void SetupButtons()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (backgroundCloseButton != null)
            backgroundCloseButton.onClick.AddListener(ClosePanel);
    }

    public void ShowCardInfo(CardData cardData)
    {
        if (cardData == null) return;

        Debug.Log($"Mostrando información de carta: {cardData.cardName}");

        // Actualizar información de la carta
        UpdateCardInfo(cardData);

        // Mostrar panel
        ShowPanel();

        // Bloquear interacciones
        BlockAllInteractions();
    }

    private void UpdateCardInfo(CardData cardData)
    {
        // Actualizar imagen
        if (cardImage != null && cardData.icon != null)
            cardImage.sprite = cardData.icon;

        // Actualizar nombre
        if (cardNameText != null)
            cardNameText.text = cardData.cardName;

        // Actualizar descripción
        if (cardDescriptionText != null)
            cardDescriptionText.text = cardData.description;

        // Actualizar efecto con valores calculados
        if (cardEffectText != null)
            cardEffectText.text = GetCardEffectText(cardData);
    }

    private string GetCardEffectText(CardData cardData)
    {
        if (GameManager.Instance == null) return "Efecto no disponible";

        float upgradedValue = cardData.baseValue + cardData.individualBaseValueUpgrade;

        switch (cardData.cardType)
        {
            case CardData.CardType.Attack:
                float attackValue = upgradedValue * GameManager.Instance.damageMultiplier * cardData.individualDamageMultiplier;
                return $"<color=#FF4444><b>ATAQUE</b></color>\n\n" +
                       $"Inflige <color=#FFD700>{attackValue:F1}</color> de daño al enemigo.\n\n" +
                       $"Valor base: {upgradedValue:F1}\n" +
                       $"Multiplicador de daño: {GameManager.Instance.damageMultiplier:F1}x\n" +
                       $"Multiplicador individual: {cardData.individualDamageMultiplier:F1}x";

            case CardData.CardType.Block:
                float blockValue = upgradedValue * GameManager.Instance.blockMultiplier;
                return $"<color=#4444FF><b>DEFENSA</b></color>\n\n" +
                       $"Reduce el próximo ataque del enemigo en <color=#FFD700>{blockValue:F0}%</color>.\n\n" +
                       $"Valor base: {upgradedValue:F1}\n" +
                       $"Multiplicador de defensa: {GameManager.Instance.blockMultiplier:F1}x";

            case CardData.CardType.Heal:
                float healValue = upgradedValue * GameManager.Instance.healMultiplier;
                return $"<color=#44FF44><b>CURACIÓN</b></color>\n\n" +
                       $"Restaura <color=#FFD700>{healValue:F1}</color> de vida.\n\n" +
                       $"Valor base: {upgradedValue:F1}\n" +
                       $"Multiplicador de curación: {GameManager.Instance.healMultiplier:F1}x";

            default:
                return "Tipo de carta desconocido";
        }
    }

    public void ShowPanel()
    {
        if (isOpen) return;

        isOpen = true;
        if (panelObject != null)
            panelObject.SetActive(true);

        // Animar apertura
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimatePanel(true));
    }

    public void ClosePanel()
    {
        if (!isOpen) return;

        Debug.Log("Cerrando panel de información de carta");

        isOpen = false;

        // Animar cierre
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimatePanel(false));
    }

    private System.Collections.IEnumerator AnimatePanel(bool show)
    {
        float startTime = Time.time;
        Vector3 startScale = show ? Vector3.zero : Vector3.one;
        Vector3 targetScale = show ? Vector3.one : Vector3.zero;
        float startAlpha = show ? 0f : 1f;
        float targetAlpha = show ? 1f : 0f;

        while (Time.time - startTime < animationDuration)
        {
            float progress = (Time.time - startTime) / animationDuration;
            float curveProgress = scaleAnimationCurve.Evaluate(progress);

            if (panelObject != null)
            {
                panelObject.transform.localScale = Vector3.Lerp(startScale, targetScale, curveProgress);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            }

            yield return null;
        }

        // Asegurar valores finales
        if (panelObject != null)
        {
            panelObject.transform.localScale = targetScale;
            if (!show)
                panelObject.SetActive(false);
        }

        if (canvasGroup != null)
            canvasGroup.alpha = targetAlpha;

        // Desbloquear interacciones si se está cerrando
        if (!show)
        {
            UnblockAllInteractions();
        }
    }

    private void BlockAllInteractions()
    {
        Debug.Log("Bloqueando todas las interacciones");

        // Bloquear interacciones de cartas
        if (HandManager.Instance != null)
            HandManager.Instance.SetInteractable(false);

        // Bloquear interacciones del GameManager
        if (GameManager.Instance != null)
        {
            // Guardar el estado actual del turno
            GameManager.Instance.SetInteractionBlocked(true);
        }

        // Bloquear raycast del canvas del panel
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }

    private void UnblockAllInteractions()
    {
        Debug.Log("Desbloqueando todas las interacciones");

        // Desbloquear interacciones de cartas
        if (HandManager.Instance != null)
            HandManager.Instance.SetInteractable(true);

        // Desbloquear interacciones del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetInteractionBlocked(false);
        }

        // Desbloquear raycast del canvas del panel
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
