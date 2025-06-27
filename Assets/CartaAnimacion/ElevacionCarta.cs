using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ElevacionCarta : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración de Elevación")]
    public float alturaElevacion = 20f;
    [Header("Configuración de Escalado")]
    public float factorEscala = 1.1f;
    public float suavizado = 10f;

    private Vector3 posicionOriginal;
    private Vector3 posicionObjetivo;
    private Vector3 escalaOriginal;
    
    void Start()
    {
        escalaOriginal = transform.localScale;
        posicionOriginal = transform.localPosition;
        posicionObjetivo = posicionOriginal;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        posicionObjetivo = posicionOriginal + new Vector3(0, alturaElevacion, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        posicionObjetivo = posicionOriginal;
    }

    void Update()
    {
        // Animación de movimiento suavizado
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            posicionObjetivo,
            suavizado * Time.deltaTime
        );
    }
}