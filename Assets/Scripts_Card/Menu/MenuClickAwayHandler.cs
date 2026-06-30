using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Detecta clicks fuera del botón actualmente enfocado y le pide a la cámara
/// que vuelva a la posición original. Colocar en un único GameObject de la escena
/// (por ejemplo "MenuManager").
/// </summary>
public class MenuClickAwayHandler : MonoBehaviour
{
    public static MenuClickAwayHandler Instance { get; private set; }

    /// <summary>Se dispara cuando la cámara vuelve al overview por click afuera.</summary>
    public static event Action OnReturnedToOverview;

    private MenuButtonFocus _activeButton;

    private void Awake()
    {
        Instance = this;
    }

    public void SetActiveButton(MenuButtonFocus button)
    {
        _activeButton = button;
    }

    private void Update()
    {
        if (_activeButton == null) return;
        if (MenuCameraController.Instance == null) return;
        if (!MenuCameraController.Instance.IsFocused) return;
        if (MenuCameraController.Instance.IsTransitioning) return;

        bool clicked = Input.GetMouseButtonDown(0);
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // Si usás el nuevo Input System exclusivamente, reemplazar la línea de arriba por:
        // bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#endif
        if (!clicked) return;

        // ¿El click cayó sobre algún elemento de UI (botón, panel, etc.)?
        bool clickedOnUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (!clickedOnUI)
        {
            ReturnToOverview();
            return;
        }

        // Si clickeó UI pero no es el botón activo (por ejemplo otro botón visible
        // detrás, o un panel distinto), también consideramos que es "click afuera"
        // salvo que sea el propio botón enfocado.
        var pressed = EventSystem.current.currentSelectedGameObject;
        if (pressed != null && pressed == _activeButton.gameObject)
        {
            // Es un click legítimo sobre el botón enfocado: dejamos que su
            // propio onClick se ejecute, no hacemos nada acá.
            return;
        }
    }

    private void ReturnToOverview()
    {
        MenuCameraController.Instance.ReturnToOverview();
        _activeButton?.ResetFocusState();
        _activeButton = null;
        OnReturnedToOverview?.Invoke();
    }
}
