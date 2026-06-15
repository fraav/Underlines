using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Arrastre alineado con Card.cs. Durante el drag reparenta al Canvas para coordenadas correctas
/// (el panel objeto tiene anclas distintas al canvas).
/// </summary>
public class ObjectCard : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 escalaOriginal;
    private bool seleccionada;
    private bool arrastrando;

    private RectTransform rect;
    private RectTransform canvasRect;
    private Vector2 posicionInicial;
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

        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();

        outline = GetComponent<Outline>();
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();
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

        if (!arrastrando && rect != null)
        {
            if (posicionInicial == Vector2.zero && rect.anchoredPosition != Vector2.zero)
                posicionInicial = rect.anchoredPosition;

            float distanceToInitial = Vector2.Distance(rect.anchoredPosition, posicionInicial);
            if (distanceToInitial > 5f)
                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, posicionInicial, Time.deltaTime * velocidadAnimacion);
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
            return;

        ObjectCardDisplay cardDisplay = GetComponent<ObjectCardDisplay>();
        if (cardDisplay == null || cardDisplay.currentCard == null || GameManager.Instance == null)
            return;

        if (GameManager.Instance.currentTurn != GameManager.TurnState.PlayerTurn)
            return;

        if (!GameManager.Instance.CanAffordObjectEnergy(cardDisplay.currentCard))
            return;

        EnsureCanvasRefs();
        if (rect == null || canvasRect == null || canvas == null)
            return;

        posicionInicial = rect.anchoredPosition;

        dragOriginalParent = transform.parent;
        dragOriginalSiblingIndex = transform.GetSiblingIndex();
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        arrastrando = true;
        seleccionada = true;
        if (outline != null)
            outline.enabled = true;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 posicionMouse))
        {
            RestoreToHandParent();
            return;
        }

        offsetDrag = posicionMouse - rect.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!arrastrando || canvas == null || rect == null || canvasRect == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 posicionCanvas))
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
        ObjectCardDisplay cardDisplay = GetComponent<ObjectCardDisplay>();

        if (target != null && cardDisplay != null && cardDisplay.currentCard != null &&
            target.CompareTag("Player"))
        {
            arrastrando = false;
            GameManager.Instance.TryPlayObjectCard(cardDisplay);
            return;
        }

        RestoreToHandParent();
    }

    private void RestoreToHandParent()
    {
        arrastrando = false;

        if (dragOriginalParent != null)
        {
            transform.SetParent(dragOriginalParent, false);
            transform.SetSiblingIndex(dragOriginalSiblingIndex);
        }

        if (rect != null)
            rect.anchoredPosition = posicionInicial;

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = 1f;

        HandManager.Instance?.SyncObjectCardPositions();
    }

    private void EnsureCanvasRefs()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                canvasRect = canvas.GetComponent<RectTransform>();
        }
    }

    private void CheckValidTarget3D(Vector2 screenPosition)
    {
        GameObject target = GetTarget3D(screenPosition);
        bool isValid = target != null && target.CompareTag("Player");

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = isValid ? 1f : 0.7f;
    }

    private GameObject GetTarget3D(Vector2 screenPosition)
    {
        Camera camera = Camera.main ?? FindObjectOfType<Camera>();
        if (camera == null) return null;

        Ray ray = camera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Enemy"))
                return hit.collider.gameObject;
        }

        return null;
    }

    public void SetSelected(bool selected)
    {
        seleccionada = selected;
        if (outline != null)
            outline.enabled = selected;
    }

    public void UpdateOriginalPosition(Vector3 newPosition)
    {
        UpdateOriginalPosition(new Vector2(newPosition.x, newPosition.y));
    }

    public void UpdateOriginalPosition(Vector2 newPosition)
    {
        if (arrastrando) return;

        if (rect == null)
            rect = GetComponent<RectTransform>();

        posicionInicial = newPosition;
    }

    public bool IsBeingDragged() => arrastrando;
}
