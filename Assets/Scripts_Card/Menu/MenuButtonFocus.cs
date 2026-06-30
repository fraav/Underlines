using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Colocar en cada botón del menú 3D (el mismo GameObject que tiene el componente Button).
/// Al hacer click, pide a la cámara que se acerque al "focusPoint" asignado.
/// La acción real del botón (cargar escena, abrir panel, etc.) se sigue
/// configurando normalmente en el onClick del Button en el Inspector,
/// pero solo se ejecuta una vez que la cámara ya está enfocada en él
/// (ver explicación de uso más abajo).
/// </summary>
[RequireComponent(typeof(Button))]
public class MenuButtonFocus : MonoBehaviour, IPointerClickHandler
{
    [Header("Punto de foco")]
    [Tooltip("Transform vacío que define posición y rotación de cámara al enfocar este botón.")]
    public Transform focusPoint;

    public GameObject gameobject;

    [Header("Comportamiento")]
    [Tooltip("Si está activo, el primer click enfoca la cámara y el botón no ejecuta su acción todavía.")]
    public bool requireFocusBeforeAction = true;

    private Button _button;
    private bool _isThisButtonFocused;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        MenuClickAwayHandler.OnReturnedToOverview += HandleReturnedToOverview;
    }

    private void OnDisable()
    {
        MenuClickAwayHandler.OnReturnedToOverview -= HandleReturnedToOverview;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var cam = MenuCameraController.Instance;
        if (cam == null || focusPoint == null) return;

        // Si ya está enfocado en este botón, no hace falta volver a mover la cámara;
        // dejamos pasar el click para que el Button dispare su onClick normalmente.
        if (_isThisButtonFocused) return;

        if (requireFocusBeforeAction)
        {
            // Si la cámara está en otro foco, primero la regresamos y luego enfocamos este.
            cam.FocusOn(focusPoint);
            gameobject.SetActive(this);
            MenuClickAwayHandler.Instance?.SetActiveButton(this);
            _isThisButtonFocused = true;

            // Evita que el click "de foco" dispare también la acción del botón
            // este mismo frame, dejando la primera interacción solo para el zoom.
            eventData.Use();
        }
    }

    private void HandleReturnedToOverview()
    {
        _isThisButtonFocused = false;
   
    }

    /// <summary>
    /// Llamar manualmente si querés resetear el estado de foco sin pasar por el handler global.
    /// </summary>
    public void ResetFocusState()
    {
        _isThisButtonFocused = false;
        gameobject.SetActive(false);
    }
}
