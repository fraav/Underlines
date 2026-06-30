using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 escalaOriginal;

    private bool seleccionada = false;
    private bool arrastrando = false;

    private RectTransform rect;
    private RectTransform canvasRect;
    private Vector2 posicionInicial;
    private bool posicionInicialValida;
    private Canvas canvas;

    private Vector2 offsetDrag;
    private Transform dragOriginalParent;
    private int dragOriginalSiblingIndex;

    [Tooltip("Velocidad de retorno a la posición de la mano al cancelar el arrastre.")]
    public float velocidadAnimacion = 15f;

    private Outline outline;
    public Color colorOutlineBase = Color.yellow;
    public Color colorOutlinePulso = Color.red;
    public float velocidadPulso = 4f;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        escalaOriginal = transform.localScale;
        posicionInicial = rect.anchoredPosition;
        posicionInicialValida = true;

        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.effectColor = colorOutlineBase;
        outline.effectDistance = new Vector2(3, 3);
        outline.enabled = false;
    }

    private void Update()
    {
        if (seleccionada && outline != null)
        {
            float pulso = (Mathf.Sin(Time.time * velocidadPulso) + 1f) * 0.5f;
            outline.effectColor = Color.Lerp(colorOutlineBase, colorOutlinePulso, pulso);
        }

        transform.localScale = escalaOriginal;
        transform.localRotation = Quaternion.identity;

        if (!arrastrando && rect != null && posicionInicialValida)
        {
            float distanceToInitial = Vector2.Distance(rect.anchoredPosition, posicionInicial);
            if (distanceToInitial > 5f)
            {
                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, posicionInicial, Time.deltaTime * velocidadAnimacion);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance != null && GameManager.Instance.AreInteractionsBlocked())
            return;

        seleccionada = !seleccionada;
        if (outline != null)
            outline.enabled = seleccionada;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.Instance != null && GameManager.Instance.AreInteractionsBlocked())
        {
            Debug.LogWarning("[Card] OnBeginDrag bloqueado - AreInteractionsBlocked: true");
            return;
        }

        CardDisplay cardDisplay = GetComponent<CardDisplay>();
        if (cardDisplay == null)
        {
            Debug.LogWarning("[Card] OnBeginDrag - CardDisplay es null");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[Card] OnBeginDrag - GameManager.Instance es null");
            return;
        }

        if (GameManager.Instance.currentTurn != GameManager.TurnState.PlayerTurn)
        {
            Debug.LogWarning($"[Card] OnBeginDrag - No es turno del jugador. Turno actual: {GameManager.Instance.currentTurn}");
            return;
        }

        if (!GameManager.Instance.CanAffordEnergy(cardDisplay.currentCard))
        {
            Debug.LogWarning("[Card] OnBeginDrag - Energía insuficiente para esta carta.");
            return;
        }

        if (rect == null)
        {
            rect = GetComponent<RectTransform>();
        }

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvasRect = canvas.GetComponent<RectTransform>();
            }
        }

        if (rect == null || canvasRect == null || canvas == null)
        {
            Debug.LogError("[Card] No se pueden obtener referencias necesarias para el arrastre");
            return;
        }

        posicionInicial = rect.anchoredPosition;
        posicionInicialValida = true;

        if (!GameManager.Instance.StartTargetSelection(cardDisplay.currentCard, cardDisplay))
        {
            return;
        }

        dragOriginalParent = transform.parent;
        dragOriginalSiblingIndex = transform.GetSiblingIndex();
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        arrastrando = true;
        seleccionada = true;
        if (outline != null)
            outline.enabled = true;

        Vector2 posicionMouse;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out posicionMouse))
        {
            Debug.LogWarning("[Card] No se pudo convertir la posición del mouse a coordenadas del canvas");
            RestoreToHandParent();
            GameManager.Instance.CancelSelection();
            return;
        }

        offsetDrag = posicionMouse - rect.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!arrastrando) return;

        if (canvas == null || rect == null || canvasRect == null)
        {
            return;
        }

        Vector2 posicionCanvas;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out posicionCanvas))
        {
            return;
        }

        rect.anchoredPosition = posicionCanvas - offsetDrag;

        CheckValidTarget3D(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        seleccionada = false;
        if (outline != null)
            outline.enabled = false;

        GameObject target = GetTarget3D(eventData.position);
        CardDisplay cardDisplay = GetComponent<CardDisplay>();

        if (target != null && cardDisplay != null && cardDisplay.currentCard != null)
        {
            bool isValidTarget = false;
            bool isPlayerTarget = target.CompareTag("Player");
            bool isEnemyTarget = target.CompareTag("Enemy");

            switch (cardDisplay.currentCard.cardType)
            {
                case CardData.CardType.Attack:
                    isValidTarget = isEnemyTarget;
                    break;
                case CardData.CardType.Block:
                case CardData.CardType.Heal:
                case CardData.CardType.Booster:
                    isValidTarget = isPlayerTarget;
                    break;
            }

            if (!isValidTarget && cardDisplay.currentCard.ShouldExecuteAsBlockCard())
                isValidTarget = isPlayerTarget;

            if (isValidTarget)
            {
                ReparentToHandContainer();
                arrastrando = false;
                GameManager.Instance.SelectTarget(target);
                return;
            }
        }

        RestoreToHandParent();
        GameManager.Instance.CancelSelection();
    }

    private void ReparentToHandContainer()
    {
        if (dragOriginalParent == null) return;

        transform.SetParent(dragOriginalParent, true);
        transform.SetSiblingIndex(dragOriginalSiblingIndex);
    }

    private void RestoreToHandParent()
    {
        arrastrando = false;
        ReparentToHandContainer();

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = 1f;
    }

    private void CheckValidTarget3D(Vector2 screenPosition)
    {
        GameObject target = GetTarget3D(screenPosition);
        CardDisplay cardDisplay = GetComponent<CardDisplay>();

        bool isValid = false;
        if (target != null && cardDisplay != null && cardDisplay.currentCard != null)
        {
            bool isPlayerTarget = target.CompareTag("Player");
            bool isEnemyTarget = target.CompareTag("Enemy");

            switch (cardDisplay.currentCard.cardType)
            {
                case CardData.CardType.Attack:
                    isValid = isEnemyTarget;
                    break;
                case CardData.CardType.Block:
                case CardData.CardType.Heal:
                case CardData.CardType.Booster:
                    isValid = isPlayerTarget;
                    break;
            }

            if (!isValid && cardDisplay.currentCard.ShouldExecuteAsBlockCard())
                isValid = isPlayerTarget;
        }

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = isValid ? 1f : 0.7f;
        }
    }

    private GameObject GetTarget3D(Vector2 screenPosition)
    {
        Camera camera = Camera.main;
        if (camera == null) camera = FindObjectOfType<Camera>();

        if (camera == null) return null;

        Ray ray = camera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("Player") || hitObject.CompareTag("Enemy"))
            {
                return hitObject;
            }
        }

        return null;
    }

    public void SetSelected(bool selected)
    {
        seleccionada = selected;
        if (outline != null)
            outline.enabled = selected;
    }

    public void SetHover(bool hover) { }

    public void UpdateOriginalPosition(Vector3 newPosition)
    {
        Vector2 newPos2D = new Vector2(newPosition.x, newPosition.y);
        UpdateOriginalPosition(newPos2D);
    }

    public void UpdateOriginalPosition(Vector2 newPosition)
    {
        if (arrastrando) return;

        if (rect == null)
        {
            rect = GetComponent<RectTransform>();
        }

        posicionInicial = newPosition;
        posicionInicialValida = true;
    }

    public bool IsBeingDragged()
    {
        return arrastrando;
    }
}
