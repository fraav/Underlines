using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Arrastre alineado con Card.cs: mismas coordenadas (canvas), sin reparent.
/// ignoreLayout evita que el Vertical Layout Group mueva la carta en reposo.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ObjectCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 escalaOriginal;
    private bool arrastrando;
    private bool seleccionada;

    private RectTransform rect;
    private RectTransform canvasRect;
    private Canvas canvas;
    private Vector2 offsetDrag;
    private Vector2 posicionInicial;

    private LayoutElement layoutElement;

    private Outline outline;
    public Color colorOutlineBase = Color.yellow;
    public Color colorOutlinePulso = Color.green;
    public float velocidadPulso = 4f;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<LayoutElement>();
    }

    private void Start()
    {
        escalaOriginal = transform.localScale;

        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            canvasRect = canvas.transform as RectTransform;

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

        if (arrastrando)
            return;

        transform.localScale = escalaOriginal;
        transform.localRotation = Quaternion.identity;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.Instance != null && GameManager.Instance.AreInteractionsBlocked())
            return;

        ObjectCardDisplay display = GetComponent<ObjectCardDisplay>();
        if (display == null || display.currentCard == null)
            return;

        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.currentTurn != GameManager.TurnState.PlayerTurn)
            return;

        if (!GameManager.Instance.CanAffordObjectEnergy(display.currentCard))
            return;

        EnsureCanvasReferences();

        if (rect == null || canvasRect == null)
            return;

        layoutElement.ignoreLayout = true;
        transform.SetAsLastSibling();

        posicionInicial = rect.anchoredPosition;

        arrastrando = true;
        seleccionada = true;
        if (outline != null)
            outline.enabled = true;

        Camera eventCamera = GetEventCamera(eventData);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventCamera, out Vector2 posicionMouse))
        {
            EndDragCancelled();
            return;
        }

        offsetDrag = posicionMouse - rect.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!arrastrando || canvasRect == null || rect == null)
            return;

        Camera eventCamera = GetEventCamera(eventData);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventCamera, out Vector2 posicionCanvas))
        {
            return;
        }

        rect.anchoredPosition = posicionCanvas - offsetDrag;
        CheckValidTarget(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        seleccionada = false;
        if (outline != null)
            outline.enabled = false;

        ResetDragAlpha();

        GameObject target = GetTarget3D(eventData.position);
        ObjectCardDisplay display = GetComponent<ObjectCardDisplay>();

        if (target != null && display != null && display.currentCard != null &&
            target.CompareTag("Player"))
        {
            arrastrando = false;
            layoutElement.ignoreLayout = false;

            if (GameManager.Instance != null)
                GameManager.Instance.TryPlayObjectCard(display);

            return;
        }

        EndDragCancelled();
    }

    private void EndDragCancelled()
    {
        arrastrando = false;
        layoutElement.ignoreLayout = false;

        if (rect != null)
            rect.anchoredPosition = posicionInicial;

        seleccionada = false;
        if (outline != null)
            outline.enabled = false;

        ResetDragAlpha();
        HandManager.Instance?.RebuildObjectHandLayout();
    }

    private void EnsureCanvasReferences()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                canvasRect = canvas.transform as RectTransform;
        }
    }

    private Camera GetEventCamera(PointerEventData eventData)
    {
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return eventData.pressEventCamera;
    }

    private void CheckValidTarget(Vector2 screenPosition)
    {
        GameObject target = GetTarget3D(screenPosition);
        bool isValid = target != null && target.CompareTag("Player");

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = isValid ? 1f : 0.7f;
    }

    private void ResetDragAlpha()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = 1f;
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

    public bool IsBeingDragged() => arrastrando;

    public void UpdateOriginalPosition(Vector2 newPosition)
    {
        if (arrastrando) return;
        posicionInicial = newPosition;
    }
}
