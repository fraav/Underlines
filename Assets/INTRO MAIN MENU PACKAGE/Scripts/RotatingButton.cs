using UnityEngine;
using UnityEngine.EventSystems;

public class RotatingButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float rotationSpeed = 90f;     // grados por segundo en eje Y
    public float scaleUpSize = 1.2f;      // escala al agrandar
    public float scaleSpeed = 5f;         // velocidad del cambio de escala

    private bool isHovering = false;
    private bool isRotating = true;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private Quaternion originalRotation;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        originalRotation = transform.rotation;
    }

    void Update()
    {
        // Rotación continua en eje Y
        if (isRotating)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }

        // Escalado suave
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isRotating = false;
        transform.rotation = originalRotation; // Reinicia rotación
        targetScale = originalScale * scaleUpSize;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isRotating = true;
        targetScale = originalScale;
    }
}
