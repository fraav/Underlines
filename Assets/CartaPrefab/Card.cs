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
    private Vector2 posicionInicial;
    private Canvas canvas;

    private Vector2 offsetDrag;

    // Balanceo
    private float anguloBalanceo = 0f;
    private Vector2 posicionMouseAnterior;
    public float sensibilidadBalanceo = 1.0f; // Aumentado para mayor sensibilidad
    public float maxBalanceo = 20f; // Aumentado para mayor rango
    public float amortiguacionBalanceo = 5f; // Reducido para balanceo más persistente

    // Balanceo idle
    public float oscilacionIdle = 3f; // Aumentado para oscilación más visible
    public float frecuenciaIdle = 1.5f; // Reducido para oscilación más lenta
    private float tiempoIdle = 0f;
    private bool movimientoReciente = false;
    private float tiempoSinMovimiento = 0f;
    public float tiempoParaIdle = 0.5f; // Aumentado para activar idle más rápido

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
            // Durante el arrastre, usar el balanceo
            float anguloFinal = anguloBalanceo;
            
            // Si no hay movimiento reciente, agregar oscilación idle
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
        
        // Aplicar rotación
        Quaternion nuevaRotacion = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotacionObjetivo), Time.deltaTime * velocidadAnimacion);
        transform.rotation = nuevaRotacion;
        

        // Posición
        if (!arrastrando)
        {
            rect.anchoredPosition = Vector3.Lerp(rect.anchoredPosition, posicionInicial, Time.deltaTime * velocidadAnimacion);
        }
        else
        {
            // Durante el arrastre, amortiguar el balanceo hacia cero
            anguloBalanceo = Mathf.Lerp(anguloBalanceo, 0f, Time.deltaTime * amortiguacionBalanceo);

            // Actualizar tiempo sin movimiento
            if (!movimientoReciente)
                tiempoSinMovimiento += Time.deltaTime;
            else
                tiempoSinMovimiento = 0f;

            // Resetear movimiento reciente para el siguiente frame
            movimientoReciente = false;
            
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => sobreMouse = true;
    public void OnPointerExit(PointerEventData eventData) => sobreMouse = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Verificar si las interacciones están bloqueadas
        if (GameManager.Instance != null && GameManager.Instance.AreInteractionsBlocked())
            return;

        seleccionada = !seleccionada;
        if (outline != null)
            outline.enabled = seleccionada; // Mostrar u ocultar el outline
        Debug.Log("Carta seleccionada: " + name);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
        // Verificar si las interacciones están bloqueadas
        if (GameManager.Instance != null && GameManager.Instance.AreInteractionsBlocked())
            return;

        // Get CardData from CardDisplay
        CardDisplay cardDisplay = GetComponent<CardDisplay>();
        if (cardDisplay == null || GameManager.Instance == null || 
            GameManager.Instance.currentTurn != GameManager.TurnState.PlayerTurn)
            return;

        Debug.Log("OnBeginDrag called on Card");
        
        arrastrando = true;
        seleccionada = true;
        if (outline != null)
            outline.enabled = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 posicionInicialAlterna);
    
        // Calcular el offset del mouse con respecto al centro del objeto
        offsetDrag = posicionInicialAlterna - rect.anchoredPosition;
        posicionMouseAnterior = posicionInicialAlterna;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out posicionMouseAnterior);
        
        
        
        tiempoIdle = 0f;
        tiempoSinMovimiento = 0f;

        // Start target selection
        GameManager.Instance.StartTargetSelection(cardDisplay.currentCard, cardDisplay);
    }


public void OnDrag(PointerEventData eventData)
{
    if (canvas == null) return;

    Vector2 posicionCanvas;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out posicionCanvas);

    // Aplicar la posición manteniendo el offset inicial
    rect.anchoredPosition = posicionCanvas - offsetDrag;

    Vector2 delta = posicionCanvas - posicionMouseAnterior;
    posicionMouseAnterior = posicionCanvas;

    // Calcular balanceo basado en el movimiento horizontal
    anguloBalanceo -= delta.x * sensibilidadBalanceo;
    anguloBalanceo = Mathf.Clamp(anguloBalanceo, -maxBalanceo, maxBalanceo);

    // Detectar movimiento reciente
    if (Mathf.Abs(delta.x) > 0.01f)
    {
        movimientoReciente = true;
        tiempoSinMovimiento = 0f; // Resetear el tiempo sin movimiento
    }
    
    // Check for valid target using 3D detection
    CheckValidTarget3D(eventData.position);
}

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag called on Card");
        
        arrastrando = false;
        seleccionada = false;
        if (outline != null)
            outline.enabled = false;

        // Check for valid target using 3D detection
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
            
            if (isValidTarget)
            {
                Debug.Log($"Card dropped on valid target: {target.name}");
                GameManager.Instance.SelectTarget(target);
                return;
            }
        }
        
        Debug.Log("Card dropped on invalid target, returning to hand");
        GameManager.Instance.CancelSelection();
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
        }
        
        // Visual feedback
        if (isValid)
        {
            GetComponent<CanvasGroup>().alpha = 1f;
        }
        else
        {
            GetComponent<CanvasGroup>().alpha = 0.7f;
        }
    }

    private GameObject GetTarget3D(Vector2 screenPosition)
    {
        // Convert screen position to world ray
        Camera camera = Camera.main;
        if (camera == null) camera = FindObjectOfType<Camera>();
        
        if (camera == null) return null;
        
        Ray ray = camera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        Debug.Log($"Raycasting from screen position: {screenPosition}");
        
        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            Debug.Log($"Hit object: {hitObject.name}, Tag: {hitObject.tag}");
            
            if (hitObject.CompareTag("Player") || hitObject.CompareTag("Enemy"))
            {
                Debug.Log($"Found valid target: {hitObject.name}");
                return hitObject;
            }
        }
        
        Debug.Log("No valid target found");
        return null;
    }

    // Métodos públicos para controlar efectos visuales desde CardDisplay
    public void SetSelected(bool selected)
    {
        seleccionada = selected;
        if (outline != null)
            outline.enabled = selected;
    }

    public void SetHover(bool hover)
    {
        sobreMouse = hover;
    }

    public void UpdateOriginalPosition(Vector3 newPosition)
    {
        print("CALLED UPDATE ORIGINAL POSITION");
        posicionInicial = newPosition;
    }

    public bool IsBeingDragged()
    {
        return arrastrando;
    }

}