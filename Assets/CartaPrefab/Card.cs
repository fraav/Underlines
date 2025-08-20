using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Para usar Outline e Image

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 escalaOriginal;
    private Vector3 escalaHover = new Vector3(1.1f, 1.1f, 1f);
    private Vector3 escalaSeleccionada = new Vector3(1.25f, 1.25f, 1f);

    private bool sobreMouse = false;
    private bool seleccionada = false;
    private bool arrastrando = false;

    public float inclinacionMax = 8f;
    public float velocidadAnimacion = 15f;
    private Vector3 rotacionObjetivo;

    private RectTransform rect;
    private RectTransform canvasRect;
    private Vector3 posicionInicial;
    private Canvas canvas;

    private Vector2 offsetDrag;

    // Balanceo
    private float anguloBalanceo = 0f;
    private Vector2 posicionMouseAnterior;
    public float sensibilidadBalanceo = 0.5f;
    public float maxBalanceo = 12f;
    public float amortiguacionBalanceo = 8f;

    // Balanceo idle
    public float oscilacionIdle = 2f;
    public float frecuenciaIdle = 2f;
    private float tiempoIdle = 0f;
    private bool movimientoReciente = false;
    private float tiempoSinMovimiento = 0f;
    public float tiempoParaIdle = 0.2f;

    // Outline
    private Outline outline;
    public Color colorOutlineBase = Color.yellow;  // Color base del outline
    public Color colorOutlinePulso = Color.red;    // Color hacia el que pulsa
    public float velocidadPulso = 4f;              // Velocidad del efecto pulso

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        escalaOriginal = transform.localScale;
        posicionInicial = rect.anchoredPosition;

        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        // Buscar o crear Outline
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
        // Pulso del outline si está seleccionada
        if (seleccionada && outline != null)
{
    float pulso = (Mathf.Sin(Time.time * velocidadPulso) + 1f) * 0.5f; // 0 a 1
    outline.effectColor = Color.Lerp(colorOutlineBase, colorOutlinePulso, pulso);
}

        if (!arrastrando)
        {
            // Rotación por hover
            if (sobreMouse)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Vector2 posMouse;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect, mousePos, null, out posMouse);

                float normX = Mathf.Clamp(posMouse.x / (rect.rect.width * 0.5f), -1f, 1f);
                float normY = Mathf.Clamp(posMouse.y / (rect.rect.height * 0.5f), -1f, 1f);

                rotacionObjetivo = new Vector3(normY * inclinacionMax, -normX * inclinacionMax, 0f);
            }
            else
            {
                rotacionObjetivo = Vector3.zero;
            }
        }
        else
        {
            float anguloFinal = anguloBalanceo;
            if (!movimientoReciente && tiempoSinMovimiento >= tiempoParaIdle)
            {
                tiempoIdle += Time.deltaTime * frecuenciaIdle;
                anguloFinal += Mathf.Sin(tiempoIdle) * oscilacionIdle;
            }
            rotacionObjetivo = new Vector3(0f, 0f, anguloFinal);
        }

        // Escala dinámica
        Vector3 escalaObjetivo = arrastrando || seleccionada ? escalaSeleccionada :
                                 sobreMouse ? escalaHover :
                                 escalaOriginal;

        transform.localScale = Vector3.Lerp(transform.localScale, escalaObjetivo, Time.deltaTime * velocidadAnimacion);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotacionObjetivo), Time.deltaTime * velocidadAnimacion);

        // Posición
        if (!arrastrando)
        {
            rect.anchoredPosition = Vector3.Lerp(rect.anchoredPosition, posicionInicial, Time.deltaTime * velocidadAnimacion);
        }
        else
        {
            anguloBalanceo = Mathf.Lerp(anguloBalanceo, 0f, Time.deltaTime * amortiguacionBalanceo);

            if (!movimientoReciente)
                tiempoSinMovimiento += Time.deltaTime;
            else
                tiempoSinMovimiento = 0f;

            movimientoReciente = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => sobreMouse = true;
    public void OnPointerExit(PointerEventData eventData) => sobreMouse = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        seleccionada = !seleccionada;
        if (outline != null)
            outline.enabled = seleccionada; // Mostrar u ocultar el outline
        Debug.Log("Carta seleccionada: " + name);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        arrastrando = true;
        seleccionada = true;
        if (outline != null)
            outline.enabled = true;

        Vector2 mouseLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out mouseLocalPos);
        offsetDrag = new Vector2(mouseLocalPos.x, rect.rect.height / 2);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out posicionMouseAnterior);
        tiempoIdle = 0f;
        tiempoSinMovimiento = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Vector2 posicionCanvas;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out posicionCanvas);

        rect.anchoredPosition = posicionCanvas - offsetDrag;

        Vector2 delta = posicionCanvas - posicionMouseAnterior;
        posicionMouseAnterior = posicionCanvas;

        anguloBalanceo -= delta.x * sensibilidadBalanceo;
        anguloBalanceo = Mathf.Clamp(anguloBalanceo, -maxBalanceo, maxBalanceo);

        if (Mathf.Abs(delta.x) > 0.01f)
            movimientoReciente = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        arrastrando = false;
        seleccionada = false;
        if (outline != null)
            outline.enabled = false;
    }
}