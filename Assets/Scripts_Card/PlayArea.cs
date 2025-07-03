using UnityEngine;
using UnityEngine.EventSystems;

public class PlayArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerExit(PointerEventData eventData) { }

    public bool IsPointInside(Vector2 screenPoint)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Camera eventCamera = null;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            eventCamera = canvas.worldCamera;
            if (eventCamera == null)
                eventCamera = Camera.main;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform, 
            screenPoint, 
            eventCamera
        );
    }
}