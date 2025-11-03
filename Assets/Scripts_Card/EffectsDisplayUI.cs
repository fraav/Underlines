using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Muestra en la UI los efectos acumulados durante el turno del jugador
/// </summary>
public class EffectsDisplayUI : MonoBehaviour
{
    public static EffectsDisplayUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject effectsPanel;
    [SerializeField] private TMP_Text effectsText;
    [SerializeField] private Button undoButton;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetupUI();
        UpdateDisplay();
    }

    void Update()
    {
        // Actualizar el display cada frame durante el turno del jugador
        if (GameManager.Instance != null && 
            GameManager.Instance.currentTurn == GameManager.TurnState.PlayerTurn)
        {
            UpdateDisplay();
        }
    }

    private void SetupUI()
    {
        if (undoButton != null)
        {
            undoButton.onClick.RemoveAllListeners();
            undoButton.onClick.AddListener(OnUndoButtonClicked);
        }
    }

    /// <summary>
    /// Actualiza el texto de efectos acumulados
    /// </summary>
    public void UpdateDisplay()
    {
        if (effectsPanel == null || effectsText == null) return;

        bool isPlayerTurn = GameManager.Instance != null && 
                           GameManager.Instance.currentTurn == GameManager.TurnState.PlayerTurn;

        // Mostrar panel solo durante el turno del jugador
        effectsPanel.SetActive(isPlayerTurn);

        if (!isPlayerTurn) return;

        // Actualizar texto de efectos
        if (PlayerTurnEffects.Instance != null)
        {
            string effectsDescription = PlayerTurnEffects.Instance.GetEffectsDescription();
            effectsText.text = effectsDescription;
        }
        else
        {
            effectsText.text = "Sin efectos activos";
        }

        // Actualizar estado del botón de deshacer
        bool hasEffects = PlayerTurnEffects.Instance != null && 
                         PlayerTurnEffects.Instance.GetActiveEffects().Count > 0;
        
        if (undoButton != null)
        {
            undoButton.interactable = hasEffects;
        }
    }

    private void OnUndoButtonClicked()
    {
        Debug.Log("[EffectsDisplayUI] Botón DESHACER presionado");

        if (GameManager.Instance == null || 
            GameManager.Instance.currentTurn != GameManager.TurnState.PlayerTurn)
        {
            Debug.LogWarning("[EffectsDisplayUI] No se puede deshacer: no es el turno del jugador");
            return;
        }

        // Limpiar efectos acumulados
        if (PlayerTurnEffects.Instance != null)
        {
            PlayerTurnEffects.Instance.ClearEffects();
        }

        // Devolver las cartas arrastradas a la mano
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UndoPlayedBoosterCards();
        }

        UpdateDisplay();
    }

    /// <summary>
    /// Método público para forzar actualización del display
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateDisplay();
    }
}

