using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum TurnState { PlayerTurn, EnemyTurn, SelectingTarget, WaitingForDialogue }
    public TurnState currentTurn { get; private set; }

    [Header("Game Settings")]
    public float damageMultiplier = 1.0f;
    public float blockMultiplier = 1.0f;
    public float healMultiplier = 1.0f;

    [Header("Card System")]
    public const int CardsDrawPerTurn = 2;
    public const int ObjectCardsDrawPerTurn = 2;

    public List<CardData> allCards = new List<CardData>();
    private List<CardData> availableDeck = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    public List<CardData> currentHand { get; private set; } = new List<CardData>();

    [Header("Object Card System (carta/objeto)")]
    public List<ObjectCardData> allObjectCards = new List<ObjectCardData>();
    private List<ObjectCardData> availableObjectDeck = new List<ObjectCardData>();
    private List<ObjectCardData> objectDiscardPile = new List<ObjectCardData>();
    public List<ObjectCardData> currentObjectHand { get; private set; } = new List<ObjectCardData>();
    
    // Rastreo de cartas jugadas en el turno actual (para deshacer)
    private List<CardData> playedCardsThisTurn = new List<CardData>();

    [Header("References")]
    public EnemyController enemyController;
    public PlayerController playerController;
    public HealthSystem playerHealth;
    public HealthSystem enemyHealth;

    [Header("Target Selection Sprites")]
    [SerializeField] private GameObject playerTargetSprite;
    [SerializeField] private GameObject enemyTargetSprite;

    [Header("Scene Settings")]
    public bool isBattleScene = false;

    [Header("Interaction Control")]
    private bool interactionsBlocked = false;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip attackCardSound;
    public AudioClip blockCardSound;
    public AudioClip healCardSound;
    public AudioClip playerHurtSound;
    public AudioClip enemyHurtSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;
    public AudioClip enemyAttackSound;
    public AudioClip enemyHealSound;

    [Header("Transition Settings")]
    public float resultDisplayTime = 2f;

    [Header("Dialogue System")]
    public DialogueSystem dialogueSystem;

    private bool isTransitioning = false;
    private bool playerIsValidTarget;
    private bool enemyIsValidTarget;
    private const string PlayerHealthKey = "PlayerCurrentHealth";

    public const int MaxEnergyPerTurn = 8;
    private int currentEnergy;
    public int CurrentEnergy => currentEnergy;

    /// <summary>currentEnergy, MaxEnergyPerTurn</summary>
    public event System.Action<int, int> OnPlayerEnergyChanged;

    private void NotifyPlayerEnergyChanged()
    {
        OnPlayerEnergyChanged?.Invoke(currentEnergy, MaxEnergyPerTurn);
    }

    private CardData selectedCard;
    private CardDisplay selectedCardDisplay;
    public CardData SelectedCard => selectedCard;
    public CardDisplay SelectedCardDisplay => selectedCardDisplay;

    // ►►► NUEVAS VARIABLES PARA CONTROL DE DIÁLOGOS DURANTE TURNOS ◄◄◄
    private bool waitingForDialogue = false;
    private System.Action afterDialogueCallback;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        if (!scene.name.Contains("Battle"))
        {
            CleanBattleReferences();
        }

        FindSceneReferences();
        CheckIfBattleScene();
        InitializeGameForScene();
    }

    public void CleanBattleReferences()
    {
        enemyController = null;
        enemyHealth = null;
        selectedCard = null;
        selectedCardDisplay = null;
        currentTurn = TurnState.PlayerTurn;
        waitingForDialogue = false;
        afterDialogueCallback = null;

        currentHand.Clear();
        availableDeck.Clear();
        discardPile.Clear();
        playedCardsThisTurn.Clear();

        currentObjectHand.Clear();
        availableObjectDeck.Clear();
        objectDiscardPile.Clear();

        // Limpiar efectos del jugador
        if (PlayerTurnEffects.Instance != null)
        {
            PlayerTurnEffects.Instance.ClearEffects();
        }
    }

    private void CheckIfBattleScene()
    {
        isBattleScene = GameObject.FindGameObjectWithTag("Enemy") != null;
        Debug.Log($"Is battle scene? {isBattleScene}");
    }

    void InitializeGame()
    {
        LoadCardUpgrades();
        LoadPurchasedCards();
    }

    /// <summary>
    /// Carga las cartas compradas que deben estar en el deck permanente
    /// </summary>
    private void LoadPurchasedCards()
    {
        // Las cartas compradas ya están en allCards, pero podemos verificar si hay cartas guardadas
        // que necesiten ser añadidas al deck
        Debug.Log("[GameManager] Cartas en el deck permanente: " + allCards.Count);
    }

    /// <summary>
    /// Añade una carta al deck permanente del jugador
    /// </summary>
    public void AddCardToPermanentDeck(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("[GameManager] Intento de añadir carta nula al deck");
            return;
        }

        if (!allCards.Contains(card))
        {
            allCards.Add(card);
            Debug.Log($"[GameManager] ✓ Carta {card.cardName} añadida al deck permanente");
            
            // Guardar el estado
            PlayerPrefs.SetInt($"CardInDeck_{card.cardName}", 1);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning($"[GameManager] La carta {card.cardName} ya está en el deck permanente");
        }
    }

    /// <summary>
    /// Añade una carta/objeto al mazo permanente (tienda u otros sistemas).
    /// </summary>
    public void AddCardToPermanentObjectDeck(ObjectCardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("[GameManager] Intento de añadir carta/objeto nula al mazo");
            return;
        }

        if (!allObjectCards.Contains(card))
        {
            allObjectCards.Add(card);
            Debug.Log($"[GameManager] ✓ Carta/objeto {card.cardName} añadida al mazo permanente");
            PlayerPrefs.SetInt($"ObjectCardInDeck_{card.cardName}", 1);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning($"[GameManager] La carta/objeto {card.cardName} ya está en el mazo");
        }
    }

    private void FindSceneReferences()
    {
        // Buscar y asignar referencias del jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            playerHealth = player.GetComponent<HealthSystem>();

            if (playerHealth == null)
            {
                playerHealth = player.AddComponent<HealthSystem>();
            }

            playerHealth.OnTakeDamage.AddListener((damage) => {
                PlayPlayerDamageSound();
            });
        }

        // Buscar y asignar referencias del enemigo
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null)
        {
            enemyController = enemy.GetComponent<EnemyController>();
            enemyHealth = enemy.GetComponent<HealthSystem>();

            if (enemyHealth == null)
            {
                enemyHealth = enemy.AddComponent<HealthSystem>();
            }

            enemyHealth.OnTakeDamage.AddListener((damage) => {
                PlayEnemyDamageSound();
            });

            if (enemyController != null)
            {
                enemyController.OnEnemyAttack.AddListener(() => PlayEnemyAttackSound());
                enemyController.OnEnemyHeal.AddListener(() => PlayEnemyHealSound());
            }
        }

        // Buscar el sistema de diálogos
        if (dialogueSystem == null)
        {
            dialogueSystem = FindObjectOfType<DialogueSystem>();
        }

        // Buscar y asignar sprites de selección de objetivo
        FindTargetSprites();
    }

    private void FindTargetSprites()
    {
        // Buscar todos los objetos en la escena que podrían ser sprites de objetivo
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);

        foreach (GameObject obj in allObjects)
        {
            // Usar nombres específicos para identificar los sprites de objetivo
            if (obj.name == "PlayerTargetSprite" || obj.name == "PlayerTarget")
            {
                playerTargetSprite = obj;
                playerTargetSprite.SetActive(false);
                Debug.Log("Player target sprite found: " + obj.name);
            }
            else if (obj.name == "EnemyTargetSprite" || obj.name == "EnemyTarget")
            {
                enemyTargetSprite = obj;
                enemyTargetSprite.SetActive(false);
                Debug.Log("Enemy target sprite found: " + obj.name);
            }
        }

        // Si no se encontraron por nombre, buscar por tag
        if (playerTargetSprite == null || enemyTargetSprite == null)
        {
            GameObject[] targetObjects = GameObject.FindGameObjectsWithTag("TargetSprite");
            foreach (GameObject targetObj in targetObjects)
            {
                if (targetObj.transform.parent != null && targetObj.transform.parent.CompareTag("Player") && playerTargetSprite == null)
                {
                    playerTargetSprite = targetObj;
                    playerTargetSprite.SetActive(false);
                }
                else if (targetObj.transform.parent != null && targetObj.transform.parent.CompareTag("Enemy") && enemyTargetSprite == null)
                {
                    enemyTargetSprite = targetObj;
                    enemyTargetSprite.SetActive(false);
                }
            }
        }

        // Verificación final
        if (playerTargetSprite == null) Debug.LogWarning("Player target sprite not found in scene");
        if (enemyTargetSprite == null) Debug.LogWarning("Enemy target sprite not found in scene");
    }

    private void InitializeGameForScene()
    {
        if (isBattleScene)
        {
            currentTurn = TurnState.PlayerTurn;

            if (playerHealth != null)
            {
                playerHealth.SetMaxHealth(100);
                playerHealth.LoadHealth(PlayerHealthKey, 100);
                playerHealth.OnHealthChanged.AddListener((health) => {
                    playerHealth.SaveHealth(PlayerHealthKey);
                });
                playerHealth.OnDeath.AddListener(OnPlayerDeath);
            }

            if (enemyHealth != null)
            {
                enemyHealth.SetMaxHealth(100);
                enemyHealth.OnDeath.AddListener(OnEnemyDefeated);
            }

            StartCoroutine(InitializeBattle());
        }
    }

    private IEnumerator InitializeBattle()
    {
        yield return new WaitForEndOfFrame();
        ResetCardSystemForNewBattle();
    }

    private void ResetCardSystemForNewBattle()
    {
        currentHand.Clear();
        availableDeck.Clear();
        discardPile.Clear();
        playedCardsThisTurn.Clear();
        availableDeck.AddRange(allCards);
        ShuffleDeck();

        currentObjectHand.Clear();
        availableObjectDeck.Clear();
        objectDiscardPile.Clear();
        availableObjectDeck.AddRange(allObjectCards);
        ShuffleObjectDeck();

        currentEnergy = MaxEnergyPerTurn;

        // CRÍTICO: Desbloquear interacciones antes de establecer el turno
        SetInteractionBlocked(false);
        waitingForDialogue = false;

        // CRÍTICO: Establecer el turno ANTES de robar cartas
        currentTurn = TurnState.PlayerTurn;

        DrawCardsForNewTurn();

        // CRÍTICO: Forzar actualización del estado después de robar cartas
        if (HandManager.Instance != null)
        {
            HandManager.Instance.UpdateInteractableState();
        }

        NotifyPlayerEnergyChanged();
    }

    // ►►► MÉTODO PARA ESPERAR DIÁLOGO ◄◄◄
    private IEnumerator WaitForDialogue(System.Action callback)
    {
        waitingForDialogue = true;
        currentTurn = TurnState.WaitingForDialogue;

        // Bloquear interacciones durante el diálogo
        SetInteractionBlocked(true);
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(false);
        }

        // Esperar a que termine el diálogo
        yield return new WaitWhile(() => dialogueSystem != null && dialogueSystem.IsDialogueActive);

        // Restaurar estado
        waitingForDialogue = false;
        SetInteractionBlocked(false);

        // Ejecutar callback después del diálogo
        callback?.Invoke();
    }

    public void OnEnemyDefeated()
    {
        if (isTransitioning) return;

        Debug.Log("Enemy defeated! Victory.");
        PlayVictorySound();

        CancelSelection();
        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }

        StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        isTransitioning = true;

        // Solo mostrar resultado y esperar
        yield return new WaitForSeconds(resultDisplayTime);

        // Aquí se manejaría la victoria desde otro script
        Debug.Log("Victoria completada - Manejar desde otro script");

        isTransitioning = false;
    }

    private void OnPlayerDeath()
    {
        if (isTransitioning) return;

        Debug.Log("Player defeated! Game Over.");
        PlayDefeatSound();
        StartCoroutine(DefeatRoutine());
    }

    private IEnumerator DefeatRoutine()
    {
        isTransitioning = true;

        // Solo mostrar resultado y esperar
        yield return new WaitForSeconds(resultDisplayTime);

        // Aquí se manejaría la derrota desde otro script
        Debug.Log("Derrota completada - Manejar desde otro script");

        isTransitioning = false;
    }

    public void CancelSelection()
    {
        if (currentTurn == TurnState.SelectingTarget)
        {
            if (playerTargetSprite != null) playerTargetSprite.SetActive(false);
            if (enemyTargetSprite != null) enemyTargetSprite.SetActive(false);

            playerController?.SetHighlight(false);
            enemyController?.SetHighlight(false);

            if (selectedCardDisplay != null)
            {
                selectedCardDisplay.SetSelected(false);
                selectedCardDisplay = null;
            }

            currentTurn = TurnState.PlayerTurn;
            selectedCard = null;
        }
    }

    public int GetPlayEnergyCost(CardData card)
    {
        if (card == null) return 0;
        return Mathf.Max(0, card.playEnergyCost);
    }

    public bool CanAffordEnergy(CardData card)
    {
        return card != null && currentEnergy >= GetPlayEnergyCost(card);
    }

    public int GetObjectPlayEnergyCost(ObjectCardData card)
    {
        if (card == null) return 0;
        return Mathf.Max(0, card.playEnergyCost);
    }

    public bool CanAffordObjectEnergy(ObjectCardData card)
    {
        return card != null && currentEnergy >= GetObjectPlayEnergyCost(card);
    }

    private void SpendEnergyForCard(CardData card)
    {
        int cost = GetPlayEnergyCost(card);
        currentEnergy = Mathf.Max(0, currentEnergy - cost);
        RefreshHandInteractableState();
        NotifyPlayerEnergyChanged();
    }

    private void SpendEnergyForObjectCard(ObjectCardData card)
    {
        int cost = GetObjectPlayEnergyCost(card);
        currentEnergy = Mathf.Max(0, currentEnergy - cost);
        RefreshHandInteractableState();
        NotifyPlayerEnergyChanged();
    }

    private void RefreshHandInteractableState()
    {
        if (HandManager.Instance != null)
            HandManager.Instance.UpdateInteractableState();
    }

    public void RefundEnergy(int amount)
    {
        if (amount <= 0) return;
        currentEnergy = Mathf.Min(MaxEnergyPerTurn, currentEnergy + amount);
        if (HandManager.Instance != null)
        {
            HandManager.Instance.UpdateInteractableState();
        }

        NotifyPlayerEnergyChanged();
    }

    public bool StartTargetSelection(CardData card, CardDisplay display)
    {
        if (AreInteractionsBlocked() || waitingForDialogue) return false;
        if (!CanAffordEnergy(card)) return false;

        if (selectedCardDisplay != null && selectedCardDisplay != display)
        {
            selectedCardDisplay.SetSelected(false);
        }

        selectedCard = card;
        selectedCardDisplay = display;
        currentTurn = TurnState.SelectingTarget;
        selectedCardDisplay?.SetSelected(true);

        // Objetivo jugador: booster, bloqueo, cura o carta con blockBonusType
        if (card.cardType == CardData.CardType.Booster || card.ShouldExecuteAsBlockCard())
        {
            playerIsValidTarget = true;
            enemyIsValidTarget = false;
        }
        else
        {
            // Mantener compatibilidad con cartas antiguas si es necesario
            switch (card.cardType)
            {
                case CardData.CardType.Attack:
                    playerIsValidTarget = false;
                    enemyIsValidTarget = true;
                    break;
                case CardData.CardType.Block:
                case CardData.CardType.Heal:
                    playerIsValidTarget = true;
                    enemyIsValidTarget = false;
                    break;
            }
        }

        if (playerTargetSprite != null) playerTargetSprite.SetActive(playerIsValidTarget);
        if (enemyTargetSprite != null) enemyTargetSprite.SetActive(enemyIsValidTarget);

        playerController?.SetHighlight(playerIsValidTarget);
        enemyController?.SetHighlight(enemyIsValidTarget);
        return true;
    }

    public void SelectTarget(GameObject target)
    {
        if (AreInteractionsBlocked() || waitingForDialogue) return;

        Debug.Log($"SelectTarget called with target: {target?.name}");
        Debug.Log($"Current turn: {currentTurn}, Selected card: {selectedCard?.cardName}");

        if (currentTurn != TurnState.SelectingTarget || selectedCard == null)
        {
            Debug.Log("SelectTarget failed: Invalid turn state or no selected card");
            return;
        }

        bool isValid = (target.CompareTag("Player") && playerIsValidTarget) ||
                      (target.CompareTag("Enemy") && enemyIsValidTarget);

        Debug.Log($"Target validation - Player: {target.CompareTag("Player")}, Enemy: {target.CompareTag("Enemy")}");
        Debug.Log($"Valid targets - Player: {playerIsValidTarget}, Enemy: {enemyIsValidTarget}");
        Debug.Log($"Is valid: {isValid}");

        if (!isValid)
        {
            Debug.Log("SelectTarget failed: Invalid target");
            return;
        }

        Debug.Log("SelectTarget: Valid target, executing card action");

        if (playerTargetSprite != null) playerTargetSprite.SetActive(false);
        if (enemyTargetSprite != null) enemyTargetSprite.SetActive(false);

        playerController?.SetHighlight(false);
        enemyController?.SetHighlight(false);
        
        // Guardar referencia de la carta antes de ejecutar
        CardData cardToExecute = selectedCard;

        SpendEnergyForCard(cardToExecute);

        RemoveCardFromHand(selectedCard);

        // Limpiar selección antes de ejecutar para permitir nuevas selecciones
        if (selectedCardDisplay != null)
        {
            selectedCardDisplay.SetSelected(false);
            selectedCardDisplay = null;
        }

        selectedCard = null;
        currentTurn = TurnState.PlayerTurn;

        // Ejecutar la acción de la carta
        // Para cartas Booster, esto NO termina el turno
        ExecuteCardAction(cardToExecute);

        if (HandManager.Instance != null)
        {
            HandManager.Instance.UpdateInteractableState();
        }

        Debug.Log("[GameManager] Carta ejecutada. Estado del turno: PlayerTurn. Puedes seguir jugando cartas.");
    }

    private void RemoveCardFromHand(CardData card)
    {
        if (currentHand.Contains(card))
        {
            currentHand.Remove(card);
            // Agregar a la lista de cartas jugadas este turno (para poder deshacer)
            playedCardsThisTurn.Add(card);
            
            Debug.Log($"[GameManager] Carta removida de la mano: {card.cardName}. Cartas jugadas este turno: {playedCardsThisTurn.Count}");
            
            HandManager.Instance.RefreshHand();
        }
    }

    /// <summary>
    /// Deshace todas las cartas potenciadoras jugadas este turno, devolviéndolas a la mano
    /// </summary>
    public void UndoPlayedBoosterCards()
    {
        if (currentTurn != TurnState.PlayerTurn)
        {
            Debug.LogWarning("[GameManager] No se puede deshacer: no es el turno del jugador");
            return;
        }

        Debug.Log($"[GameManager] ===== DESHACIENDO TODAS LAS ACCIONES DE POTENCIACIÓN DEL TURNO =====");
        Debug.Log($"[GameManager] Cartas jugadas este turno: {playedCardsThisTurn.Count}");

        int refundEnergy = 0;
        foreach (CardData card in playedCardsThisTurn)
        {
            if (card != null && card.cardType == CardData.CardType.Booster)
            {
                refundEnergy += GetPlayEnergyCost(card);
            }
        }

        // Devolver TODAS las cartas potenciadoras jugadas este turno a la mano
        List<CardData> cardsToReturn = new List<CardData>();
        
        foreach (CardData card in playedCardsThisTurn)
        {
            if (card != null && card.cardType == CardData.CardType.Booster)
            {
                // Verificar que la carta no esté ya en la mano
                if (!currentHand.Contains(card))
                {
                    currentHand.Add(card);
                    cardsToReturn.Add(card);
                    Debug.Log($"[GameManager] ✓ Carta devuelta a la mano: {card.cardName} ({card.boosterEffectType})");
                }
                else
                {
                    Debug.LogWarning($"[GameManager] La carta {card.cardName} ya está en la mano, omitiendo");
                }
            }
        }

        // Limpiar la lista de cartas jugadas este turno
        int cardsCleared = playedCardsThisTurn.Count;
        playedCardsThisTurn.Clear();
        Debug.Log($"[GameManager] Lista de cartas jugadas limpiada ({cardsCleared} cartas)");

        if (refundEnergy > 0)
        {
            RefundEnergy(refundEnergy);
        }

        RefreshHandInteractableState();

        // Limpiar TODOS los efectos acumulados del turno
        if (PlayerTurnEffects.Instance != null)
        {
            int effectsCleared = PlayerTurnEffects.Instance.GetActiveEffects().Count;
            PlayerTurnEffects.Instance.ClearEffects();
            Debug.Log($"[GameManager] Efectos acumulados limpiados ({effectsCleared} efectos)");
        }

        // Actualizar la mano visualmente
        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
            Debug.Log("[GameManager] Mano actualizada visualmente");
        }

        // Actualizar el display de efectos
        if (EffectsDisplayUI.Instance != null)
        {
            EffectsDisplayUI.Instance.RefreshDisplay();
        }

        Debug.Log($"[GameManager] ===== DESHACER COMPLETADO: {cardsToReturn.Count} cartas devueltas a la mano =====");
    }

    private void ExecuteCardAction(CardData card)
    {
        Debug.Log($"[GameManager] ExecuteCardAction called with card: {card.cardName}, type: {card.cardType}");

        // ►►► MODIFICADO: Pasar el target apropiado
        GameObject target = GetAppropriateTarget(card);
        PlayCardEffects(card, target);

        if (card.ShouldExecuteAsBlockCard())
        {
            Card_Block(card);
            return;
        }

        switch (card.cardType)
        {
            case CardData.CardType.Attack:
                Debug.LogWarning("[GameManager] Carta de Ataque jugada directamente. Debería usarse el botón de acción.");
                break;
            case CardData.CardType.Block:
                Card_Block(card);
                break;
            case CardData.CardType.Heal:
                Debug.LogWarning("[GameManager] Carta de Curación jugada directamente. Debería usarse el botón de acción.");
                break;
            case CardData.CardType.Booster:
                Debug.Log("[GameManager] Executing Booster card");
                Card_Booster(card);
                break;
        }
    }

    /// <summary>
    /// Obtiene el target apropiado para la carta basado en su tipo
    /// </summary>
    private GameObject GetAppropriateTarget(CardData card)
    {
        switch (card.cardType)
        {
            case CardData.CardType.Attack:
                return enemyHealth?.gameObject;
            case CardData.CardType.Block:
            case CardData.CardType.Heal:
            case CardData.CardType.Booster:
                return playerHealth?.gameObject;
            default:
                if (card.HasBlockBonusEffect())
                    return playerHealth?.gameObject;
                return null;
        }
    }

    // ►►► NUEVO MÉTODO: Reproduce el sonido específico de una carta y activa su objeto de animación
    public void PlayCardEffects(CardData card, GameObject target = null)
    {
        if (card == null) return;
        
        // Reproducir sonido específico de la carta
        PlayCardSound(card);
        
        // Activar objeto de animación (prefab o objeto en escena)
        ActivateCardAnimationObject(card, target);
    }

    // ►►► NUEVO MÉTODO: Reproduce el sonido específico de la carta
    private void PlayCardSound(CardData card)
    {
        if (card.cardSound != null)
        {
            AudioSource.PlayClipAtPoint(card.cardSound, Camera.main.transform.position);
            Debug.Log($"[GameManager] Sonido de carta reproducido: {card.cardName}");
        }
        else
        {
            // Sonido por defecto según el tipo de carta
            switch (card.cardType)
            {
                case CardData.CardType.Attack:
                    PlayAttackCardSound();
                    break;
                case CardData.CardType.Block:
                    PlayBlockCardSound();
                    break;
                case CardData.CardType.Heal:
                    PlayHealCardSound();
                    break;
                case CardData.CardType.Booster:
                    PlayAttackCardSound(); // Usar sonido de ataque como placeholder para booster
                    break;
            }
        }
    }

    // ►►► NUEVO MÉTODO: Activa el objeto de animación de la carta (prefab o objeto en escena)
    private void ActivateCardAnimationObject(CardData card, GameObject target = null)
    {
        // Prioridad: Prefab sobre objeto en escena
        if (card.animationPrefab != null)
        {
            SpawnAnimationPrefab(card, target);
        }
        else if (!string.IsNullOrEmpty(card.nombreObjetoEnEscenaAActivar))
        {
            // Sistema antiguo - activar objeto en escena
            GameObject animationObject = card.GetObjetoAActivar();
            if (animationObject != null)
            {
                StartCoroutine(ActivateObjectForTime(animationObject, card.animationObjectActiveTime));
                Debug.Log($"[GameManager] Objeto en escena activado: {card.nombreObjetoEnEscenaAActivar} por {card.animationObjectActiveTime} segundos");
            }
        }
        else
        {
            Debug.Log($"[GameManager] No hay objeto o prefab configurado para activar en la carta: {card.cardName}");
        }
    }

    // ►►► NUEVO MÉTODO: Instancia y activa un prefab de animación
    private void SpawnAnimationPrefab(CardData card, GameObject target = null)
    {
        if (card.animationPrefab == null)
        {
            Debug.LogWarning($"[GameManager] No hay prefab asignado para la carta: {card.cardName}");
            return;
        }

        // Determinar la posición de spawn
        Vector3 spawnPosition = card.prefabSpawnPosition;
        Transform parent = null;

        if (target != null && card.attachToTarget)
        {
            // Si hay target y está configurado para attach, usar como parent
            parent = target.transform;
            if (spawnPosition == Vector3.zero)
            {
                spawnPosition = Vector3.zero; // Posición local del parent
            }
        }

        // Instanciar el prefab
        GameObject animationInstance = Instantiate(card.animationPrefab, spawnPosition, Quaternion.identity, parent);
        
        if (animationInstance != null)
        {
            // Si no hay parent y la posición es cero, intentar posicionar en el target
            if (parent == null && spawnPosition == Vector3.zero && target != null)
            {
                animationInstance.transform.position = target.transform.position;
            }

            // Configurar para destrucción automática después del tiempo
            StartCoroutine(DestroyAfterTime(animationInstance, card.animationObjectActiveTime));
            
            Debug.Log($"[GameManager] Prefab instanciado: {card.animationPrefab.name} por {card.animationObjectActiveTime} segundos");
        }
        else
        {
            Debug.LogWarning($"[GameManager] No se pudo instanciar el prefab: {card.animationPrefab.name}");
        }
    }

    // ►►► NUEVO MÉTODO: Destruye un objeto después de un tiempo específico
    private IEnumerator DestroyAfterTime(GameObject obj, float time)
    {
        if (obj == null) yield break;
        
        yield return new WaitForSeconds(time);
        
        if (obj != null)
        {
            Destroy(obj);
            Debug.Log($"[GameManager] Prefab destruido: {obj.name}");
        }
    }

    /// <summary>
    /// Ejecuta una carta potenciadora (Booster)
    /// </summary>
    private void Card_Booster(CardData card)
    {
        Debug.Log($"[GameManager] Card_Booster called with: {card.cardName}, effect: {card.boosterEffectType}");

        // Los efectos de sonido y objeto ya se reprodujeron en ExecuteCardAction
        
        // Agregar el efecto a los efectos acumulados
        if (PlayerTurnEffects.Instance != null)
        {
            PlayerTurnEffects.Instance.AddEffect(card);
            Debug.Log($"[GameManager] Efecto agregado: {card.boosterEffectType}");
        }

        // Actualizar display de efectos
        if (EffectsDisplayUI.Instance != null)
        {
            EffectsDisplayUI.Instance.RefreshDisplay();
        }

        // IMPORTANTE: Las cartas potenciadoras NO terminan el turno
        Debug.Log("[GameManager] Carta potenciadora jugada. El turno continúa, puedes jugar más cartas.");
    }

    // ─── Carta/objeto: juego instantáneo (no termina turno) ───

    public bool TryPlayObjectCard(ObjectCardDisplay display)
    {
        if (display == null || display.currentCard == null) return false;
        if (AreInteractionsBlocked() || waitingForDialogue) return false;
        if (currentTurn != TurnState.PlayerTurn) return false;
        if (!CanAffordObjectEnergy(display.currentCard)) return false;

        ObjectCardData card = display.currentCard;
        SpendEnergyForObjectCard(card);
        RemoveObjectCardFromHand(card);
        StartCoroutine(ExecuteObjectCardEffectRoutine(card));

        Debug.Log($"[GameManager] Carta/objeto jugada: {card.cardName}. Turno continúa.");
        return true;
    }

    private void RemoveObjectCardFromHand(ObjectCardData card)
    {
        if (!currentObjectHand.Contains(card)) return;
        currentObjectHand.Remove(card);
        objectDiscardPile.Add(card);

        if (HandManager.Instance != null)
            HandManager.Instance.RefreshObjectHand();
    }

    private IEnumerator ExecuteObjectCardEffectRoutine(ObjectCardData card)
    {
        if (card == null) yield break;

        GameObject target = playerHealth != null ? playerHealth.gameObject : null;
        PlayObjectCardSound(card);

        if (card.UsesAnimationPrefab())
        {
            PlayObjectCardVisual(card, target);

            float actionPoint = card.prefabActionPointTime > 0f
                ? card.prefabActionPointTime
                : (card.customActionPointTime > 0f ? card.customActionPointTime : 0.5f);

            yield return new WaitForSeconds(actionPoint);
            ApplyObjectCardEffectLogic(card);
        }
        else if (playerController != null)
        {
            bool finished = false;
            playerController.PlayObjectCardAnimation(
                card,
                () => ApplyObjectCardEffectLogic(card),
                () => finished = true);

            while (!finished)
                yield return null;
        }
        else
        {
            ApplyObjectCardEffectLogic(card);
        }

        RefreshHandInteractableState();
    }

    private void ApplyObjectCardEffectLogic(ObjectCardData card)
    {
        if (card == null) return;

        switch (card.effectType)
        {
            case ObjectCardData.ObjectEffectType.DoubleNextAction:
                if (PlayerTurnEffects.Instance != null)
                    PlayerTurnEffects.Instance.AddDoubleActionEffect(card.cardName);
                if (EffectsDisplayUI.Instance != null)
                    EffectsDisplayUI.Instance.RefreshDisplay();
                break;

            case ObjectCardData.ObjectEffectType.Heal:
                float finalHeal = (card.baseValue + card.individualBaseValueUpgrade) * healMultiplier;
                if (playerHealth != null)
                    playerHealth.Heal((int)finalHeal);
                break;

            case ObjectCardData.ObjectEffectType.RestoreEnergy:
                RefundEnergy(card.restoreEnergyAmount);
                break;

            case ObjectCardData.ObjectEffectType.DrawCard:
                DrawCardsFromMainDeck(card.drawCardCount);
                break;
        }
    }

    private void ExecuteObjectCardEffect(ObjectCardData card)
    {
        StartCoroutine(ExecuteObjectCardEffectRoutine(card));
    }

    public void PlayObjectCardEffects(ObjectCardData card, GameObject target = null)
    {
        if (card == null) return;
        PlayObjectCardSound(card);
        PlayObjectCardVisual(card, target);
    }

    private void PlayObjectCardSound(ObjectCardData card)
    {
        if (card == null) return;

        if (card.cardSound != null)
            AudioSource.PlayClipAtPoint(card.cardSound, Camera.main.transform.position);
        else
            PlayHealCardSound();
    }

    private void PlayObjectCardVisual(ObjectCardData card, GameObject target = null)
    {
        if (card == null || !card.UsesAnimationPrefab()) return;

        if (card.animationPrefab != null)
            SpawnObjectAnimationPrefab(card, target);
        else if (!string.IsNullOrEmpty(card.nombreObjetoEnEscenaAActivar))
        {
            GameObject animationObject = card.GetObjetoAActivar();
            if (animationObject != null)
                StartCoroutine(ActivateObjectForTime(animationObject, card.animationObjectActiveTime));
        }
    }

    private void SpawnObjectAnimationPrefab(ObjectCardData card, GameObject target)
    {
        if (card.animationPrefab == null) return;

        Vector3 spawnPosition = card.prefabSpawnPosition;
        Transform parent = null;

        if (target != null && card.attachToTarget)
            parent = target.transform;

        GameObject instance = Instantiate(card.animationPrefab, spawnPosition, Quaternion.identity, parent);
        if (instance != null)
        {
            if (parent == null && spawnPosition == Vector3.zero && target != null)
                instance.transform.position = target.transform.position;
            StartCoroutine(DestroyAfterTime(instance, card.animationObjectActiveTime));
        }
    }

    public void UpdateTargetSelection(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            playerIsValidTarget = true;
        }
    }

    public void Card_Attack(CardData card)
    {
        Debug.Log($"Card_Attack called with damage: {card.baseValue}");

        // ►►► MODIFICADO: Pasar el enemigo como target
        PlayCardEffects(card, enemyHealth?.gameObject);

        float finalDamage = (card.baseValue + card.individualBaseValueUpgrade) *
                          damageMultiplier * card.individualDamageMultiplier;

        Debug.Log($"Final damage calculated: {finalDamage}");

        void ApplyDamage()
        {
            Debug.Log($"ApplyDamage called, enemyHealth: {enemyHealth != null}");
            if (enemyHealth != null)
            {
                Debug.Log($"Applying {finalDamage} damage to enemy");
                enemyHealth.TakeDamage((int)finalDamage);
                PlayEnemyDamageSound();
            }
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurnAfterDialogue());

        Debug.Log($"Calling PlayCardAnimation on playerController: {playerController != null}");
        playerController.PlayCardAnimation(card, ApplyDamage, CompleteTurn);
    }

    public void Card_Heal(CardData card)
    {
        // ►►► MODIFICADO: Pasar el jugador como target
        PlayCardEffects(card, playerHealth?.gameObject);

        float finalHeal = (card.baseValue + card.individualBaseValueUpgrade) * healMultiplier;

        void ApplyHeal()
        {
            if (playerHealth != null)
            {
                playerHealth.Heal((int)finalHeal);
            }
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurnAfterDialogue());

        playerController.PlayCardAnimation(card, ApplyHeal, CompleteTurn);
    }

    /// <summary>
    /// Carta de bloqueo jugada al arrastrar al jugador. Activa bloqueo pendiente + animación (no termina turno).
    /// </summary>
    public void Card_Block(CardData card)
    {
        PlayCardEffects(card, playerHealth?.gameObject);

        float upgradedValue = card.baseValue + card.individualBaseValueUpgrade;
        float effectsMult = PlayerTurnEffects.Instance != null
            ? PlayerTurnEffects.Instance.GetBlockMultiplier()
            : 1f;
        float finalBlockValue = upgradedValue * effectsMult * blockMultiplier;
        float reductionMultiplier = 1f - (finalBlockValue / 100f);

        void ApplyBlock()
        {
            if (enemyController != null)
                enemyController.ApplyAttackReduction(reductionMultiplier);

            if (playerController != null)
            {
                playerController.ActivateBlock(
                    reductionMultiplier,
                    card.blockBonusType,
                    card.blockEnergyReward,
                    card.blockCounterDamage);
            }

            PlayBlockCardSound();
        }

        if (playerController != null)
            playerController.PlayCardAnimation(GetBlockAnimationCard(card), ApplyBlock, null);
        else
            ApplyBlock();
    }

    private CardData GetBlockAnimationCard(CardData card)
    {
        if (card.cardType == CardData.CardType.Block)
            return card;

        CardData animCard = ScriptableObject.CreateInstance<CardData>();
        animCard.cardType = CardData.CardType.Block;
        animCard.customAnimationTrigger = card.customAnimationTrigger;
        animCard.customActionPointTime = card.customActionPointTime;
        animCard.customAnimationDuration = card.customAnimationDuration;
        animCard.cardSound = card.cardSound;
        return animCard;
    }

    // ►►► MÉTODO MODIFICADO PARA ESPERAR DIÁLOGOS ◄◄◄
    public IEnumerator EndPlayerTurn()
    {
        Debug.Log("[GameManager] ===== FINALIZANDO TURNO DEL JUGADOR =====");

        // CRÍTICO: Bloquear interacciones al finalizar el turno
        SetInteractionBlocked(true);

        // Limpiar efectos del turno (se consumen al ejecutar acciones)
        if (PlayerTurnEffects.Instance != null)
        {
            PlayerTurnEffects.Instance.ClearEffects();
        }

        // Descartar todas las cartas de la mano
        DiscardAllHandCards();
        DiscardAllObjectHandCards();

        // Limpiar cartas jugadas del turno
        playedCardsThisTurn.Clear();

        // CRÍTICO: Cambiar el turno ANTES de desactivar interacciones
        currentTurn = TurnState.EnemyTurn;
        
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(false);
            Debug.Log("[GameManager] HandManager.SetInteractable(false) llamado");
        }

        // Actualizar visibilidad de botones de acción
        if (ActionButtonsController.Instance != null)
        {
            ActionButtonsController.Instance.UpdateButtonsVisibility();
        }

        yield return new WaitForSeconds(0.5f);
        
        if (enemyController != null)
        {
            enemyController.StartEnemyTurn();
        }

        Debug.Log("[GameManager] ===== TURNO DEL JUGADOR FINALIZADO =====");
    }

    private IEnumerator EndPlayerTurnAfterDialogue()
    {
        // Si hay un diálogo activo, esperar a que termine
        if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
        {
            Debug.Log("Esperando a que termine el diálogo antes de cambiar turno...");
            yield return StartCoroutine(WaitForDialogue(() => {
                StartCoroutine(EndPlayerTurn());
            }));
        }
        else
        {
            yield return StartCoroutine(EndPlayerTurn());
        }
    }

    public void StartPlayerTurn()
    {
        Debug.Log("[GameManager] ===== INICIANDO TURNO DEL JUGADOR =====");

        currentEnergy = MaxEnergyPerTurn;
        NotifyPlayerEnergyChanged();

        // CRÍTICO: Desbloquear interacciones PRIMERO antes de cualquier otra operación
        SetInteractionBlocked(false);
        waitingForDialogue = false;

        if (enemyController != null)
        {
            enemyController.ResetAttackMultiplier();
        }

        if (playerController != null)
        {
            playerController.DeactivateBlock();
            playerController.ClearBlockBonuses();
        }

        // Limpiar efectos del turno anterior
        if (PlayerTurnEffects.Instance != null)
        {
            PlayerTurnEffects.Instance.ClearEffects();
        }

        // Limpiar cartas jugadas del turno anterior
        playedCardsThisTurn.Clear();

        // NOTA: No es necesario descartar aquí porque las cartas ya fueron descartadas
        // en EndPlayerTurn(). La mano debería estar vacía en este punto.
        if (currentHand.Count > 0)
        {
            Debug.LogWarning($"[GameManager] Advertencia: La mano no está vacía al inicio del turno ({currentHand.Count} cartas). Descartando...");
            DiscardAllHandCards();
        }

        if (currentObjectHand.Count > 0)
        {
            Debug.LogWarning($"[GameManager] Mano objeto no vacía ({currentObjectHand.Count}). Descartando...");
            DiscardAllObjectHandCards();
        }

        // CRÍTICO: Establecer el turno ANTES de robar cartas
        // Esto asegura que RefreshHand() vea el turno correcto
        currentTurn = TurnState.PlayerTurn;

        // Robar 2 cartas + 2 carta/objeto (RefreshAllHands dentro de DrawCardsForNewTurn)
        DrawCardsForNewTurn();
        
        // CRÍTICO: Forzar actualización del estado de interacción después de robar cartas
        // RefreshHand ya llama a UpdateInteractableState, pero lo forzamos aquí también para asegurar
        if (HandManager.Instance != null)
        {
            HandManager.Instance.UpdateInteractableState();
            Debug.Log("[GameManager] HandManager.UpdateInteractableState() llamado después de robar cartas");
        }

        // Actualizar visibilidad de botones de acción
        if (ActionButtonsController.Instance != null)
        {
            ActionButtonsController.Instance.UpdateButtonsVisibility();
        }

        // Verificación final del estado
        Debug.Log($"[GameManager] ===== TURNO DEL JUGADOR INICIADO =====");
        Debug.Log($"[GameManager] Cartas en mano: {currentHand.Count}");
        Debug.Log($"[GameManager] Interacciones bloqueadas: {AreInteractionsBlocked()}");
        Debug.Log($"[GameManager] Estado del turno: {currentTurn}");
    }

    /// <summary>
    /// Roba cartas y carta/objeto al inicio del turno del jugador (2 + 2 por defecto).
    /// </summary>
    private void DrawCardsForNewTurn()
    {
        Debug.Log("[GameManager] ===== ROBAR PARA NUEVO TURNO =====");
        DrawCardsFromMainDeck(CardsDrawPerTurn);
        DrawObjectCardsForTurn(ObjectCardsDrawPerTurn);

        if (HandManager.Instance != null)
            HandManager.Instance.RefreshAllHands();

        Debug.Log($"[GameManager] Mano cartas: {currentHand.Count}, Mano objeto: {currentObjectHand.Count}");
    }

    /// <summary>
    /// Roba cartas del mazo principal (p. ej. efecto DrawCard en carta/objeto).
    /// </summary>
    public int DrawCardsFromMainDeck(int count)
    {
        if (count <= 0) return 0;

        int handSizeBefore = currentHand.Count;
        List<CardData> drawnCards = new List<CardData>();
        int drawn = 0;
        for (int i = 0; i < count; i++)
        {
            if (availableDeck.Count == 0)
            {
                RecycleDiscardPile();
                if (availableDeck.Count == 0 && allCards.Count > 0)
                {
                    availableDeck.AddRange(allCards);
                    ShuffleDeck();
                }
            }

            if (availableDeck.Count == 0) break;

            CardData card = availableDeck[0];
            availableDeck.RemoveAt(0);
            currentHand.Add(card);
            drawnCards.Add(card);
            drawn++;
            Debug.Log($"[GameManager] Carta robada (extra): {card.cardName}");
        }

        if (HandManager.Instance != null)
        {
            if (handSizeBefore == 0)
                HandManager.Instance.RefreshHand();
            else
                HandManager.Instance.AddCardsToHand(drawnCards);
        }

        return drawn;
    }

    private void DrawObjectCardsForTurn(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (availableObjectDeck.Count == 0)
            {
                RecycleObjectDiscardPile();
                if (availableObjectDeck.Count == 0 && allObjectCards.Count > 0)
                {
                    availableObjectDeck.AddRange(allObjectCards);
                    ShuffleObjectDeck();
                }
            }

            if (availableObjectDeck.Count == 0) break;

            ObjectCardData card = availableObjectDeck[0];
            availableObjectDeck.RemoveAt(0);
            currentObjectHand.Add(card);
            Debug.Log($"[GameManager] Carta/objeto robada: {card.cardName}");
        }
    }

    /// <summary>
    /// Asegura que el mazo tenga suficientes cartas disponibles (4), reciclando el descarte si es necesario
    /// </summary>
    private void EnsureDeckHasCards(int requiredCards = 4)
    {
        // Si el deck no tiene suficientes cartas y hay cartas en el descarte, reciclar
        if (availableDeck.Count < requiredCards && discardPile.Count > 0)
        {
            Debug.Log($"[GameManager] Mazo principal tiene {availableDeck.Count} cartas, necesitamos {requiredCards}. Reciclando pila de descartes...");
            RecycleDiscardPile();
        }
        else if (availableDeck.Count == 0 && discardPile.Count == 0)
        {
            Debug.LogWarning("[GameManager] ¡ADVERTENCIA: No hay cartas disponibles en el mazo ni en los descartes!");
            // Si no hay cartas en ningún lado, reinicializar el deck desde allCards
            if (allCards.Count > 0)
            {
                Debug.Log("[GameManager] Reinicializando el deck desde allCards...");
                availableDeck.AddRange(allCards);
                ShuffleDeck();
                Debug.Log($"[GameManager] Deck reinicializado con {availableDeck.Count} cartas");
            }
        }
    }

    /// <summary>
    /// Recicla la pila de descartes al mazo principal y la baraja
    /// </summary>
    private void RecycleDiscardPile()
    {
        if (discardPile.Count == 0)
        {
            Debug.LogWarning("[GameManager] No hay cartas en la pila de descartes para reciclar");
            return;
        }

        int cardsToRecycle = discardPile.Count;
        Debug.Log($"[GameManager] Reciclando {cardsToRecycle} cartas de la pila de descartes al mazo principal...");

        // Mover todas las cartas del descarte al mazo
        availableDeck.AddRange(discardPile);
        discardPile.Clear();

        // Barajar el mazo reciclado
        ShuffleDeck();

        Debug.Log($"[GameManager] ✓ Reciclado completado: {availableDeck.Count} cartas ahora en el mazo principal");
    }

    /// <summary>
    /// Descarta todas las cartas de la mano al final del turno
    /// </summary>
    public void DiscardAllHandCards()
    {
        Debug.Log($"[GameManager] Descartando {currentHand.Count} cartas de la mano...");

        // Crear una copia de la lista para evitar problemas al modificar durante la iteración
        List<CardData> cardsToDiscard = new List<CardData>(currentHand);

        foreach (CardData card in cardsToDiscard)
        {
            if (card != null)
            {
                discardPile.Add(card);
                Debug.Log($"[GameManager] Carta descartada: {card.cardName}");
            }
        }

        currentHand.Clear();

        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }

        Debug.Log($"[GameManager] Descarte completado. Cartas en pila de descarte: {discardPile.Count}, Cartas en mano: {currentHand.Count}");
    }

    public void DiscardAllObjectHandCards()
    {
        List<ObjectCardData> toDiscard = new List<ObjectCardData>(currentObjectHand);
        foreach (ObjectCardData card in toDiscard)
        {
            if (card != null)
                objectDiscardPile.Add(card);
        }

        currentObjectHand.Clear();

        if (HandManager.Instance != null)
            HandManager.Instance.RefreshObjectHand();
    }

    private void RecycleObjectDiscardPile()
    {
        if (objectDiscardPile.Count == 0) return;
        availableObjectDeck.AddRange(objectDiscardPile);
        objectDiscardPile.Clear();
        ShuffleObjectDeck();
    }

    public void ShuffleObjectDeck()
    {
        for (int i = availableObjectDeck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            ObjectCardData temp = availableObjectDeck[i];
            availableObjectDeck[i] = availableObjectDeck[randomIndex];
            availableObjectDeck[randomIndex] = temp;
        }
    }

    public void OnEnemyAttackStart()
    {
        if (playerController != null && playerController.HasBlockPending())
        {
            playerController.ActivatePendingBlock();
        }

        if (playerController != null && playerController.HasBlockActive())
        {
            playerController.PlayBlockAnimation();
        }
    }

    public void OnEnemyAttackApplied()
    {
        PlayEnemyAttackSound();

        if (playerController != null)
        {
            if (playerController.HasBlockActive())
                PlayBlockHitSound();

            playerController.ResolveBlockBonusOnEnemyHit();
        }
    }

    public void OnEnemyTurnEnd()
    {
        if (playerController != null && playerController.HasBlockActive())
        {
            Debug.Log("Turno del enemigo terminado. El bloqueo del jugador se desactivará automáticamente.");
        }

        // Cuando termina el turno del enemigo, reactivar los objetos que el sistema de diálogo
        // desactivó (HUD, paneles, etc.). De esta forma, permanecen ocultos durante todo el turno
        // del enemigo y solo reaparecen al final.
        if (dialogueSystem != null)
        {
            dialogueSystem.ReactivateDialogueObjects();
        }
    }

    public void PlayBlockHitSound()
    {
        PlayBlockCardSound();
    }

    public void PlayPlayerDamageAnimation()
    {
        if (playerController != null)
        {
            playerController.PlayDamageAnimation();
            PlayPlayerDamageSound();
        }
    }

    public void PlayEnemyDamageSound()
    {
        PlaySound(enemyHurtSound);
    }

    public void PlayEnemyDamageSoundFromController()
    {
        PlaySound(enemyHurtSound);
    }

    public void PlayEnemyAttackSound()
    {
        PlaySound(enemyAttackSound);
    }

    public void PlayEnemyHealSound()
    {
        PlaySound(enemyHealSound);
    }

    public void PlayPlayerDamageSound()
    {
        PlaySound(playerHurtSound);
    }

    public void PlayVictorySound()
    {
        PlaySound(victorySound);
    }

    public void PlayDefeatSound()
    {
        PlaySound(defeatSound);
    }

    public void PlayAttackCardSound()
    {
        PlaySound(attackCardSound);
    }

    public void PlayBlockCardSound()
    {
        PlaySound(blockCardSound);
    }

    public void PlayHealCardSound()
    {
        PlaySound(healCardSound);
    }

    public void ShuffleDeck()
    {
        for (int i = availableDeck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardData temp = availableDeck[i];
            availableDeck[i] = availableDeck[randomIndex];
            availableDeck[randomIndex] = temp;
        }
    }

    // Método mantenido para compatibilidad, pero ahora DrawCardsForNewTurn es el método principal
    public void DrawNewHand()
    {
        Debug.LogWarning("[GameManager] DrawNewHand llamado (método legacy). Usar DrawCardsForNewTurn en su lugar.");
        DrawCardsForNewTurn();
    }

    void LoadCardUpgrades()
    {
        foreach (CardData card in allCards)
        {
            card.individualBaseValueUpgrade = PlayerPrefs.GetFloat($"{card.cardName}_baseUpgrade", 0f);
            card.individualDamageMultiplier = PlayerPrefs.GetFloat($"{card.cardName}_damageMult", 1.0f);
        }
    }

    public void ResetGameState()
    {
        damageMultiplier = 1.0f;
        blockMultiplier = 1.0f;
        healMultiplier = 1.0f;

        foreach (CardData card in allCards)
        {
            card.individualBaseValueUpgrade = 0f;
            card.individualDamageMultiplier = 1.0f;
            PlayerPrefs.SetFloat($"{card.cardName}_baseUpgrade", 0f);
            PlayerPrefs.SetFloat($"{card.cardName}_damageMult", 1.0f);
        }

        PlayerPrefs.Save();
        ResetCardSystemForNewBattle();

        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(100);
            PlayerPrefs.DeleteKey(PlayerHealthKey);
        }
    }

    private void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    // Métodos para controlar bloqueo de interacciones
    public void SetInteractionBlocked(bool blocked)
    {
        interactionsBlocked = blocked;
        Debug.Log($"[GameManager] SetInteractionBlocked({blocked}) - interactionsBlocked: {interactionsBlocked}, waitingForDialogue: {waitingForDialogue}");
        
        RefreshHandInteractableState();
    }

    public bool AreInteractionsBlocked()
    {
        bool blocked = interactionsBlocked || waitingForDialogue;
        return blocked;
    }

    // ►►► MÉTODO PÚBLICO PARA FORZAR ESPERA DE DIÁLOGO ◄◄◄
    public void WaitForDialogueCompletion(System.Action callback)
    {
        if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
        {
            StartCoroutine(WaitForDialogue(callback));
        }
        else
        {
            callback?.Invoke();
        }
    }

    // ►►► MÉTODO EXISTENTE: Corrutina para activar un objeto por un tiempo específico
    private IEnumerator ActivateObjectForTime(GameObject obj, float activeTime)
    {
        if (obj == null) yield break;
        
        // Activar el objeto
        obj.SetActive(true);
        
        // Esperar el tiempo especificado
        yield return new WaitForSeconds(activeTime);
        
        // Desactivar el objeto
        obj.SetActive(false);
        
        Debug.Log($"[GameManager] Objeto desactivado: {obj.name}");
    }
}