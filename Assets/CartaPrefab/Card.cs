using UnityEngine;
using System.Collections.Generic;
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
        

        // Posición - CRÍTICO: NO tocar la posición durante el arrastre
        // Solo ajustar la posición cuando la carta NO está siendo arrastrada y está lejos de su posición inicial
        if (!arrastrando && rect != null)
        {
            // Asegurar que la posición inicial esté sincronizada solo si es necesario
            if (posicionInicial == Vector2.zero && rect.anchoredPosition != Vector2.zero)
            {
                posicionInicial = rect.anchoredPosition;
            }
            
            // Solo hacer Lerp si la carta está significativamente lejos de su posición inicial
            // Esto evita que se fuerce constantemente la posición y permite el arrastre
            float distanceToInitial = Vector2.Distance(rect.anchoredPosition, posicionInicial);
            if (distanceToInitial > 5f) // Solo si está a más de 5 pixels de distancia (aumentado para evitar interferencias)
            {
                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, posicionInicial, Time.deltaTime * velocidadAnimacion);
            }
        }
        // Durante el arrastre, NO tocar rect.anchoredPosition aquí - OnDrag lo maneja completamente
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
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // CRÍTICO: Verificar si las interacciones están bloqueadas PRIMERO
        if (GameManager.Instance != null && GameManager.Instance.AreInteractionsBlocked())
        {
            Debug.LogWarning($"[Card] OnBeginDrag bloqueado - AreInteractionsBlocked: true");
            return;
        }

        // Get CardData from CardDisplay
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

        // Asegurar que tenemos las referencias necesarias
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

        if (rect == null || canvasRect == null)
        {
            Debug.LogError("[Card] No se pueden obtener referencias necesarias para el arrastre");
            return;
        }

        // IMPORTANTE: Sincronizar la posición inicial con la posición actual antes de comenzar el arrastre
        // Esto asegura que después de refrescar la mano, la posición inicial esté correcta
        if (posicionInicial == Vector2.zero || Vector2.Distance(posicionInicial, rect.anchoredPosition) > 0.1f)
        {
            posicionInicial = rect.anchoredPosition;
        }

        // CRÍTICO: Establecer arrastrando = true ANTES de cualquier otra operación
        // Esto previene que Update() interfiera con la posición
        arrastrando = true;
        seleccionada = true;
        if (outline != null)
            outline.enabled = true;

        // Obtener la posición del mouse en coordenadas del canvas
        Vector2 posicionMouse;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out posicionMouse))
        {
            Debug.LogWarning("[Card] No se pudo convertir la posición del mouse a coordenadas del canvas");
            arrastrando = false; // Resetear el estado si falla
            return;
        }
    
        // Calcular el offset del mouse con respecto al centro de la carta
        // Usar la posición actual del rect, no la posición inicial guardada
        offsetDrag = posicionMouse - rect.anchoredPosition;
        posicionMouseAnterior = posicionMouse;
        
        tiempoIdle = 0f;
        tiempoSinMovimiento = 0f;

        // Start target selection
        GameManager.Instance.StartTargetSelection(cardDisplay.currentCard, cardDisplay);
    }


public void OnDrag(PointerEventData eventData)
{
    // CRÍTICO: Verificar que estamos arrastrando antes de continuar
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

    // CRÍTICO: Aplicar la posición DIRECTAMENTE sin ninguna interferencia
    // Esta es la ÚNICA función que debe modificar rect.anchoredPosition durante el arrastre
    Vector2 nuevaPosicion = posicionCanvas - offsetDrag;
    rect.anchoredPosition = nuevaPosicion;

    Vector2 delta = posicionCanvas - posicionMouseAnterior;
    posicionMouseAnterior = posicionCanvas;

    // Calcular balanceo basado en el movimiento horizontal
    anguloBalanceo -= delta.x * sensibilidadBalanceo;
    anguloBalanceo = Mathf.Clamp(anguloBalanceo, -maxBalanceo, maxBalanceo);

    // Detectar movimiento reciente
    if (Mathf.Abs(delta.x) > 0.01f)
    {
        movimientoReciente = true;
        tiempoSinMovimiento = 0f;
    }
    
    // Check for valid target using 3D detection (sin logs)
    CheckValidTarget3D(eventData.position);
}

    public void OnEndDrag(PointerEventData eventData)
    {
        // CRÍTICO: NO cambiar arrastrando = false hasta después de verificar el target
        // Esto previene que Update() interfiera antes de que se complete la acción
        
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
                // Solo cambiar arrastrando después de confirmar que hay un target válido
                arrastrando = false;
                GameManager.Instance.SelectTarget(target);
                return;
            }
        }
        
        // Si no hay target válido, asegurar que la posición inicial esté actualizada
        if (rect != null && posicionInicial != Vector2.zero)
        {
            // La carta regresará a su posición inicial en el Update
        }
        else if (rect != null)
        {
            // Si no hay posición inicial guardada, usar la posición actual
            posicionInicial = rect.anchoredPosition;
        }
        
        // CRÍTICO: Cambiar arrastrando = false al final, después de todas las operaciones
        arrastrando = false;
        
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
        
        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            if (hitObject.CompareTag("Player") || hitObject.CompareTag("Enemy"))
            {
                return hitObject;
            }
        }
        
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
        // Convertir Vector3 a Vector2 para mantener consistencia
        Vector2 newPos2D = new Vector2(newPosition.x, newPosition.y);
        UpdateOriginalPosition(newPos2D);
    }

    /// <summary>
    /// Actualiza la posición original de la carta (sobrecarga para Vector2)
    /// </summary>
    public void UpdateOriginalPosition(Vector2 newPosition)
    {
        // CRÍTICO: NO actualizar la posición original si la carta está siendo arrastrada
        // Esto previene que se sobrescriba la posición durante el arrastre
        if (arrastrando) return;
        
        if (rect == null)
        {
            rect = GetComponent<RectTransform>();
        }

        // Actualizar la posición inicial
        posicionInicial = newPosition;
        
        // IMPORTANTE: NO forzar la posición del RectTransform aquí
        // Solo actualizar la posición inicial guardada
    }

    public bool IsBeingDragged()
    {
        return arrastrando;
    }

}