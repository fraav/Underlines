using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Controla los botones de acciones básicas del jugador (Ataque, Bloqueo, Curación)
/// </summary>
public class ActionButtonsController : MonoBehaviour
{
    public static ActionButtonsController Instance;

    [Header("Action Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button blockButton;
    [SerializeField] private Button healButton;

    [Header("Action Values")]
    [SerializeField] private int baseAttackValue = 10;
    [SerializeField] private int baseBlockValue = 20; // Porcentaje de reducción
    [SerializeField] private int baseHealValue = 15;

    [Header("UI References")]
    [SerializeField] private GameObject buttonsContainer;

    private bool isProcessingAction = false;

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
        SetupButtons();
        UpdateButtonsVisibility();
        LoadUpgrades();
    }

    /// <summary>
    /// Carga las mejoras guardadas de los botones de acción
    /// </summary>
    private void LoadUpgrades()
    {
        baseAttackValue += (int)PlayerPrefs.GetFloat("ActionButton_Attack_Upgrade", 0f);
        baseBlockValue += (int)PlayerPrefs.GetFloat("ActionButton_Block_Upgrade", 0f);
        baseHealValue += (int)PlayerPrefs.GetFloat("ActionButton_Heal_Upgrade", 0f);

        Debug.Log($"[ActionButtonsController] Mejoras cargadas - Ataque: {baseAttackValue}, Bloqueo: {baseBlockValue}, Curación: {baseHealValue}");
    }

    /// <summary>
    /// Mejora el valor base de ataque
    /// </summary>
    public void UpgradeBaseAttackValue(float amount)
    {
        baseAttackValue += (int)amount;
        Debug.Log($"[ActionButtonsController] Valor base de ataque mejorado: {baseAttackValue}");
    }

    /// <summary>
    /// Mejora el valor base de bloqueo
    /// </summary>
    public void UpgradeBaseBlockValue(float amount)
    {
        baseBlockValue += (int)amount;
        Debug.Log($"[ActionButtonsController] Valor base de bloqueo mejorado: {baseBlockValue}");
    }

    /// <summary>
    /// Mejora el valor base de curación
    /// </summary>
    public void UpgradeBaseHealValue(float amount)
    {
        baseHealValue += (int)amount;
        Debug.Log($"[ActionButtonsController] Valor base de curación mejorado: {baseHealValue}");
    }

    /// <summary>
    /// Obtiene el valor base de ataque
    /// </summary>
    public float GetBaseAttackValue()
    {
        return baseAttackValue;
    }

    /// <summary>
    /// Obtiene el valor base de bloqueo
    /// </summary>
    public float GetBaseBlockValue()
    {
        return baseBlockValue;
    }

    /// <summary>
    /// Obtiene el valor base de curación
    /// </summary>
    public float GetBaseHealValue()
    {
        return baseHealValue;
    }

    private void SetupButtons()
    {
        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(OnAttackButtonClicked);
        }

        if (blockButton != null)
        {
            blockButton.onClick.RemoveAllListeners();
            blockButton.onClick.AddListener(OnBlockButtonClicked);
        }

        if (healButton != null)
        {
            healButton.onClick.RemoveAllListeners();
            healButton.onClick.AddListener(OnHealButtonClicked);
        }
    }

    /// <summary>
    /// Actualiza la visibilidad de los botones según el estado del turno
    /// </summary>
    public void UpdateButtonsVisibility()
    {
        bool isPlayerTurn = GameManager.Instance != null && 
                           GameManager.Instance.currentTurn == GameManager.TurnState.PlayerTurn;
        
        if (buttonsContainer != null)
        {
            buttonsContainer.SetActive(isPlayerTurn);
        }

        bool buttonsEnabled = isPlayerTurn && !isProcessingAction;
        SetButtonsInteractable(buttonsEnabled);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (attackButton != null) attackButton.interactable = interactable;
        if (blockButton != null) blockButton.interactable = interactable;
        if (healButton != null) healButton.interactable = interactable;
    }

    private void OnAttackButtonClicked()
    {
        if (isProcessingAction || GameManager.Instance == null) return;
        if (GameManager.Instance.currentTurn != GameManager.TurnState.PlayerTurn) return;

        Debug.Log("[ActionButtonsController] Botón de ATAQUE presionado");
        StartCoroutine(ExecuteAttackAction());
    }

    private void OnBlockButtonClicked()
    {
        if (isProcessingAction || GameManager.Instance == null) return;
        if (GameManager.Instance.currentTurn != GameManager.TurnState.PlayerTurn) return;

        Debug.Log("[ActionButtonsController] Botón de BLOQUEO presionado");
        StartCoroutine(ExecuteBlockAction());
    }

    private void OnHealButtonClicked()
    {
        if (isProcessingAction || GameManager.Instance == null) return;
        if (GameManager.Instance.currentTurn != GameManager.TurnState.PlayerTurn) return;

        Debug.Log("[ActionButtonsController] Botón de CURACIÓN presionado");
        StartCoroutine(ExecuteHealAction());
    }

    private IEnumerator ExecuteAttackAction()
    {
        isProcessingAction = true;
        SetButtonsInteractable(false);
        
        // Bloquear interacciones con cartas durante la ejecución de la acción
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetInteractionBlocked(true);
        }
        
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(false);
        }
        
        Debug.Log("[ActionButtonsController] Iniciando ejecución de ataque...");

        // Obtener efectos acumulados
        int doubleActionCount = 0;
        float damageMultiplier = 1.0f;

        if (PlayerTurnEffects.Instance != null)
        {
            doubleActionCount = PlayerTurnEffects.Instance.GetDoubleActionCount();
            damageMultiplier = PlayerTurnEffects.Instance.GetDamageMultiplier();
        }

        Debug.Log($"[ActionButtonsController] Efectos: Duplicaciones={doubleActionCount}, Multiplicador de daño={damageMultiplier}");

        // Calcular daño base
        float finalDamage = baseAttackValue * damageMultiplier;

        Debug.Log($"[ActionButtonsController] Daño calculado: {finalDamage} (base: {baseAttackValue})");

        // Ejecutar ataque (con duplicaciones si las hay)
        int attackCount = 1 + doubleActionCount;
        
        for (int i = 0; i < attackCount; i++)
        {
            Debug.Log($"[ActionButtonsController] Ejecutando ataque {i + 1} de {attackCount}");
            
            yield return StartCoroutine(PerformAttackAnimation(finalDamage));
            
            // Pequeña pausa entre ataques si hay duplicación
            if (i < attackCount - 1)
            {
                yield return new WaitForSeconds(0.3f);
            }
        }

        Debug.Log("[ActionButtonsController] Todos los ataques completados");
        
        // Consumir efectos de duplicación
        if (PlayerTurnEffects.Instance != null && doubleActionCount > 0)
        {
            // Los efectos se consumirán al final del turno
        }

        yield return StartCoroutine(EndPlayerTurnAfterActions());
    }

    private IEnumerator ExecuteBlockAction()
    {
        isProcessingAction = true;
        SetButtonsInteractable(false);
        
        // Bloquear interacciones con cartas durante la ejecución de la acción
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetInteractionBlocked(true);
        }
        
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(false);
        }
        
        Debug.Log("[ActionButtonsController] Iniciando ejecución de bloqueo...");

        // Obtener efectos acumulados
        // IMPORTANTE: El bloqueo NO se duplica con DoubleAction, solo se potencia con IncreaseBlock
        float blockMultiplier = 1.0f;

        if (PlayerTurnEffects.Instance != null)
        {
            blockMultiplier = PlayerTurnEffects.Instance.GetBlockMultiplier();
            Debug.Log($"[ActionButtonsController] Efectos de bloqueo: Multiplicador={blockMultiplier:F2}x (DoubleAction NO afecta bloqueo)");
        }

        float finalBlockValue = baseBlockValue * blockMultiplier;
        float reductionMultiplier = 1f - (finalBlockValue / 100f);

        Debug.Log($"[ActionButtonsController] Bloqueo calculado: {finalBlockValue}% (Reducción: {reductionMultiplier})");
        Debug.Log($"[ActionButtonsController] NOTA: El bloqueo se ejecuta UNA SOLA VEZ (no se duplica con DoubleAction)");

        // El bloqueo siempre se ejecuta una sola vez, independientemente de DoubleAction
        yield return StartCoroutine(PerformBlockAnimation(reductionMultiplier));

        yield return StartCoroutine(EndPlayerTurnAfterActions());
    }

    private IEnumerator ExecuteHealAction()
    {
        isProcessingAction = true;
        SetButtonsInteractable(false);
        
        // Bloquear interacciones con cartas durante la ejecución de la acción
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetInteractionBlocked(true);
        }
        
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(false);
        }
        
        Debug.Log("[ActionButtonsController] Iniciando ejecución de curación...");

        // Obtener efectos acumulados
        int doubleActionCount = 0;
        float healMultiplier = 1.0f;

        if (PlayerTurnEffects.Instance != null)
        {
            doubleActionCount = PlayerTurnEffects.Instance.GetDoubleActionCount();
            healMultiplier = PlayerTurnEffects.Instance.GetHealMultiplier();
        }

        float finalHeal = baseHealValue * healMultiplier;

        Debug.Log($"[ActionButtonsController] Curación calculada: {finalHeal} (base: {baseHealValue})");

        int healCount = 1 + doubleActionCount;
        
        for (int i = 0; i < healCount; i++)
        {
            Debug.Log($"[ActionButtonsController] Ejecutando curación {i + 1} de {healCount}");
            
            yield return StartCoroutine(PerformHealAnimation(finalHeal));
            
            if (i < healCount - 1)
            {
                yield return new WaitForSeconds(0.3f);
            }
        }

        yield return StartCoroutine(EndPlayerTurnAfterActions());
    }

    private IEnumerator PerformAttackAnimation(float damage)
    {
        // Crear un CardData temporal para la animación
        CardData tempCard = ScriptableObject.CreateInstance<CardData>();
        tempCard.cardType = CardData.CardType.Attack;
        tempCard.baseValue = damage;
        tempCard.cardName = "Ataque Básico";

        void ApplyDamage()
        {
            if (GameManager.Instance != null && GameManager.Instance.enemyHealth != null)
            {
                Debug.Log($"[ActionButtonsController] Aplicando {damage} de daño al enemigo");
                GameManager.Instance.enemyHealth.TakeDamage((int)damage);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayEnemyDamageSound();
                    GameManager.Instance.PlayAttackCardSound();
                }
            }
        }

        void OnComplete() { }

        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            yield return StartCoroutine(PlayCardAnimationCoroutine(tempCard, ApplyDamage, OnComplete));
        }
        else
        {
            ApplyDamage();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator PerformBlockAnimation(float reductionMultiplier)
    {
        CardData tempCard = ScriptableObject.CreateInstance<CardData>();
        tempCard.cardType = CardData.CardType.Block;
        tempCard.baseValue = baseBlockValue;
        tempCard.cardName = "Bloqueo Básico";

        void ApplyBlock()
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.enemyController != null)
                {
                    GameManager.Instance.enemyController.ApplyAttackReduction(reductionMultiplier);
                }

                if (GameManager.Instance.playerController != null)
                {
                    GameManager.Instance.playerController.ActivateBlock(reductionMultiplier);
                }
            }
        }

        void OnComplete() { }

        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            yield return StartCoroutine(PlayCardAnimationCoroutine(tempCard, ApplyBlock, OnComplete));
        }
        else
        {
            ApplyBlock();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator PerformHealAnimation(float healAmount)
    {
        CardData tempCard = ScriptableObject.CreateInstance<CardData>();
        tempCard.cardType = CardData.CardType.Heal;
        tempCard.baseValue = healAmount;
        tempCard.cardName = "Curación Básica";

        void ApplyHeal()
        {
            if (GameManager.Instance != null && GameManager.Instance.playerHealth != null)
            {
                Debug.Log($"[ActionButtonsController] Aplicando {healAmount} de curación al jugador");
                GameManager.Instance.playerHealth.Heal((int)healAmount);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayHealCardSound();
                }
            }
        }

        void OnComplete() { }

        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            yield return StartCoroutine(PlayCardAnimationCoroutine(tempCard, ApplyHeal, OnComplete));
        }
        else
        {
            ApplyHeal();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator PlayCardAnimationCoroutine(CardData card, System.Action onAction, System.Action onComplete)
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            bool animationComplete = false;
            
            // Crear callback que marca la animación como completa
            System.Action completeCallback = () => { 
                animationComplete = true;
            };

            // Calcular duración aproximada de la animación basada en el tipo de carta
            float estimatedDuration = 1.0f; // Duración por defecto
            
            // Buscar animación para este tipo de carta si existe
            CardAnimation[] animations = GameManager.Instance.playerController.GetCardAnimations();
            if (animations != null)
            {
                foreach (var anim in animations)
                {
                    if (anim != null && anim.cardType == card.cardType)
                    {
                        estimatedDuration = anim.animationDuration;
                        break;
                    }
                }
            }

            // Ejecutar la animación y esperar a que complete
            GameManager.Instance.playerController.PlayCardAnimation(card, onAction, completeCallback);

            // Esperar a que se complete la animación (con timeout de seguridad)
            float timeout = estimatedDuration + 2.0f; // Timeout de seguridad
            float elapsed = 0f;
            
            while (!animationComplete && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!animationComplete)
            {
                Debug.LogWarning($"[ActionButtonsController] Timeout esperando animación de {card.cardType}");
            }
            
            // Pequeña pausa adicional para asegurar que todo termine
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            onAction?.Invoke();
            yield return new WaitForSeconds(0.5f);
        }

        onComplete?.Invoke();
    }

    private IEnumerator EndPlayerTurnAfterActions()
    {
        Debug.Log("[ActionButtonsController] Esperando a que terminen todos los efectos y animaciones...");
        
        // Esperar un momento adicional para asegurar que todas las animaciones terminen
        yield return new WaitForSeconds(0.5f);

        // Esperar diálogos si los hay
        if (GameManager.Instance != null && GameManager.Instance.dialogueSystem != null && 
            GameManager.Instance.dialogueSystem.IsDialogueActive)
        {
            Debug.Log("[ActionButtonsController] Esperando a que termine el diálogo...");
            yield return new WaitWhile(() => GameManager.Instance.dialogueSystem.IsDialogueActive);
        }

        Debug.Log("[ActionButtonsController] Finalizando turno del jugador...");

        // CRÍTICO: Asegurar que las interacciones estén bloqueadas antes de finalizar
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetInteractionBlocked(true);
        }

        // Consumir todos los efectos acumulados
        if (PlayerTurnEffects.Instance != null)
        {
            PlayerTurnEffects.Instance.ClearEffects();
        }

        // Descartar todas las cartas de la mano
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DiscardAllHandCards();
        }

        // Cambiar al turno del enemigo
        if (GameManager.Instance != null)
        {
            yield return StartCoroutine(GameManager.Instance.EndPlayerTurn());
        }

        isProcessingAction = false;
    }
}

