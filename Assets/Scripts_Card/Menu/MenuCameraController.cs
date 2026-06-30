using System.Collections;
using UnityEngine;

/// <summary>
/// Controla el movimiento de la cámara entre la vista general del menú
/// y el acercamiento (zoom) hacia un botón específico.
/// Colocar este script en la cámara principal del menú.
/// </summary>
public class MenuCameraController : MonoBehaviour
{
    public static MenuCameraController Instance { get; private set; }

    [Header("Posición original de la cámara")]
    [Tooltip("Si se deja vacío, se toma automáticamente la posición/rotación inicial de la cámara al arrancar.")]
    public Transform overviewPoint;

    [Header("Movimiento")]
    [Tooltip("Duración en segundos del recorrido de ida y vuelta.")]
    public float transitionDuration = 0.8f;

    [Tooltip("Curva de animación para suavizar el movimiento.")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public bool IsFocused { get; private set; }
    public bool IsTransitioning { get; private set; }

    private Transform _currentTarget;
    private Vector3 _overviewPos;
    private Quaternion _overviewRot;
    private Coroutine _moveRoutine;

    private void Awake()
    {
        Instance = this;

        if (overviewPoint != null)
        {
            _overviewPos = overviewPoint.position;
            _overviewRot = overviewPoint.rotation;
        }
        else
        {
            _overviewPos = transform.position;
            _overviewRot = transform.rotation;
        }
    }

    /// <summary>
    /// Mueve la cámara hacia el punto de foco indicado por el botón.
    /// </summary>
    public void FocusOn(Transform focusPoint)
    {
        if (IsTransitioning) return;

        _currentTarget = focusPoint;
        IsFocused = true;

        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveCamera(focusPoint.position, focusPoint.rotation));
    }

    /// <summary>
    /// Devuelve la cámara a la posición original (overview).
    /// </summary>
    public void ReturnToOverview()
    {
        if (IsTransitioning || !IsFocused) return;

        _currentTarget = null;
        IsFocused = false;

        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveCamera(_overviewPos, _overviewRot));
    }

    private IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot)
    {
        IsTransitioning = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float curved = easeCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPos, targetPos, curved);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, curved);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        IsTransitioning = false;
    }
}
