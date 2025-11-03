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
    public List<CardData> allCards = new List<CardData>();
    private List<CardData> availableDeck = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    public List<CardData> currentHand { get; private set; } = new List<CardData>();
    
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

        currentTurn = TurnState.PlayerTurn;

        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(true);
        }

        // Robar 4 cartas al inicio del primer turno
        DrawCardsForNewTurn();
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

    public void StartTargetSelection(CardData card, CardDisplay display)
    {
        if (AreInteractionsBlocked() || waitingForDialogue) return;

        if (selectedCardDisplay != null && selectedCardDisplay != display)
        {
            selectedCardDisplay.SetSelected(false);
        }

        selectedCard = card;
        selectedCardDisplay = display;
        currentTurn = TurnState.SelectingTarget;
        selectedCardDisplay?.SetSelected(true);

        // Todas las cartas potenciadoras (y las nuevas) tienen como objetivo válido al jugador
        if (card.cardType == CardData.CardType.Booster)
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

        // Asegurarse de que las cartas sigan siendo interactuables
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(true);
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
    /// Deshace las cartas potenciadoras jugadas este turno, devolviéndolas a la mano
    /// </summary>
    public void UndoPlayedBoosterCards()
    {
        Debug.Log($"[GameManager] Deshaciendo {playedCardsThisTurn.Count} cartas jugadas...");

        // Devolver solo las cartas potenciadoras a la mano
        List<CardData> cardsToReturn = new List<CardData>();
        
        foreach (CardData card in playedCardsThisTurn)
        {
            if (card != null && card.cardType == CardData.CardType.Booster)
            {
                if (!currentHand.Contains(card))
                {
                    currentHand.Add(card);
                    cardsToReturn.Add(card);
                }
            }
        }

        // Limpiar la lista de cartas jugadas
        playedCardsThisTurn.Clear();

        // Limpiar efectos acumulados
        if (PlayerTurnEffects.Instance != null)
        {
            PlayerTurnEffects.Instance.ClearEffects();
        }

        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }

        Debug.Log($"[GameManager] {cardsToReturn.Count} cartas devueltas a la mano");
    }

    private void ExecuteCardAction(CardData card)
    {
        Debug.Log($"[GameManager] ExecuteCardAction called with card: {card.cardName}, type: {card.cardType}");

        // Las cartas antiguas (Attack, Block, Heal) NO deberían ejecutarse directamente
        // porque ahora se usan los botones de acción. Pero mantenemos compatibilidad.
        // IMPORTANTE: Solo las cartas Booster se pueden jugar arrastrándolas.
        // Las cartas Attack, Block, Heal deben eliminarse del mazo o ignorarse.

        switch (card.cardType)
        {
            case CardData.CardType.Attack:
                Debug.LogWarning("[GameManager] Carta de Ataque jugada directamente. Debería usarse el botón de acción.");
                // No ejecutar automáticamente, estas cartas ya no se usan así
                break;
            case CardData.CardType.Block:
                Debug.LogWarning("[GameManager] Carta de Bloqueo jugada directamente. Debería usarse el botón de acción.");
                // No ejecutar automáticamente, estas cartas ya no se usan así
                break;
            case CardData.CardType.Heal:
                Debug.LogWarning("[GameManager] Carta de Curación jugada directamente. Debería usarse el botón de acción.");
                // No ejecutar automáticamente, estas cartas ya no se usan así
                break;
            case CardData.CardType.Booster:
                Debug.Log("[GameManager] Executing Booster card");
                Card_Booster(card);
                break;
        }
    }

    /// <summary>
    /// Ejecuta una carta potenciadora (Booster)
    /// </summary>
    private void Card_Booster(CardData card)
    {
        Debug.Log($"[GameManager] Card_Booster called with: {card.cardName}, effect: {card.boosterEffectType}");

        // Agregar el efecto a los efectos acumulados
        if (PlayerTurnEffects.Instance != null)
        {
            PlayerTurnEffects.Instance.AddEffect(card);
            Debug.Log($"[GameManager] Efecto agregado: {card.boosterEffectType}");
        }

        // Reproducir sonido de carta
        PlayAttackCardSound(); // Usar sonido de ataque como placeholder

        // Actualizar display de efectos
        if (EffectsDisplayUI.Instance != null)
        {
            EffectsDisplayUI.Instance.RefreshDisplay();
        }

        // IMPORTANTE: Las cartas potenciadoras NO terminan el turno
        // El jugador puede seguir jugando más cartas hasta presionar un botón de acción
        Debug.Log("[GameManager] Carta potenciadora jugada. El turno continúa, puedes jugar más cartas.");
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
                PlayAttackCardSound();
            }
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurnAfterDialogue());

        Debug.Log($"Calling PlayCardAnimation on playerController: {playerController != null}");
        playerController.PlayCardAnimation(card, ApplyDamage, CompleteTurn);
    }

    public void Card_Block(CardData card)
    {
        float blockValue = (card.baseValue + card.individualBaseValueUpgrade) * blockMultiplier;
        float reductionMultiplier = 1f - (blockValue / 100f);

        void ApplyBlock()
        {
            if (enemyController != null)
            {
                enemyController.ApplyAttackReduction(reductionMultiplier);
            }

            if (playerController != null)
            {
                playerController.ActivateBlock(reductionMultiplier);
            }
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurnAfterDialogue());

        playerController.PlayCardAnimation(card, ApplyBlock, CompleteTurn);
    }

    public void Card_Heal(CardData card)
    {
        float finalHeal = (card.baseValue + card.individualBaseValueUpgrade) * healMultiplier;

        void ApplyHeal()
        {
            if (playerHealth != null)
            {
                playerHealth.Heal((int)finalHeal);
                PlayHealCardSound();
            }
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurnAfterDialogue());

        playerController.PlayCardAnimation(card, ApplyHeal, CompleteTurn);
    }

    // ►►► MÉTODO MODIFICADO PARA ESPERAR DIÁLOGOS ◄◄◄
    public IEnumerator EndPlayerTurn()
    {
        Debug.Log("[GameManager] EndPlayerTurn iniciado...");

        // Limpiar efectos del turno (se consumen al ejecutar acciones)
        if (PlayerTurnEffects.Instance != null)
        {
            PlayerTurnEffects.Instance.ClearEffects();
        }

        // Descartar todas las cartas de la mano
        DiscardAllHandCards();

        // Limpiar cartas jugadas del turno
        playedCardsThisTurn.Clear();

        currentTurn = TurnState.EnemyTurn;
        
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(false);
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

        Debug.Log("[GameManager] Turno del jugador finalizado, iniciando turno del enemigo");
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
        Debug.Log("[GameManager] Iniciando turno del jugador...");

        if (enemyController != null)
        {
            enemyController.ResetAttackMultiplier();
        }

        if (playerController != null)
        {
            playerController.DeactivateBlock();
        }

        // Limpiar efectos del turno anterior
        if (PlayerTurnEffects.Instance != null)
        {
            PlayerTurnEffects.Instance.ClearEffects();
        }

        // Limpiar cartas jugadas del turno anterior
        playedCardsThisTurn.Clear();

        // Descartar todas las cartas de la mano del turno anterior
        DiscardAllHandCards();

        // Robar 4 cartas nuevas para este turno
        DrawCardsForNewTurn();

        currentTurn = TurnState.PlayerTurn;
        
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(true);
        }

        // Actualizar visibilidad de botones de acción
        if (ActionButtonsController.Instance != null)
        {
            ActionButtonsController.Instance.UpdateButtonsVisibility();
        }

        Debug.Log($"[GameManager] Turno del jugador iniciado. Cartas en mano: {currentHand.Count}");
    }

    /// <summary>
    /// Roba 4 cartas para el inicio de un nuevo turno del jugador
    /// </summary>
    private void DrawCardsForNewTurn()
    {
        Debug.Log("[GameManager] Robando 4 cartas para nuevo turno...");

        // Si el mazo disponible está vacío, barajar el descarte
        if (availableDeck.Count == 0)
        {
            Debug.Log("[GameManager] Mazo vacío, barajando descartes...");
            availableDeck.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }

        // Robar 4 cartas
        int drawCount = Mathf.Min(4, availableDeck.Count);
        List<CardData> drawnCards = new List<CardData>();

        for (int i = 0; i < drawCount; i++)
        {
            if (availableDeck.Count == 0)
            {
                // Si se acaban las cartas, barajar descartes
                availableDeck.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck();
            }

            if (availableDeck.Count > 0)
            {
                CardData card = availableDeck[0];
                availableDeck.RemoveAt(0);
                drawnCards.Add(card);
                currentHand.Add(card);
                Debug.Log($"[GameManager] Robada carta: {card.cardName}");
            }
        }

        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }

        Debug.Log($"[GameManager] Robo completado. Cartas en mano: {currentHand.Count}");
    }

    /// <summary>
    /// Descarta todas las cartas de la mano al final del turno
    /// </summary>
    public void DiscardAllHandCards()
    {
        Debug.Log($"[GameManager] Descartando {currentHand.Count} cartas de la mano...");

        foreach (CardData card in currentHand)
        {
            if (card != null)
            {
                discardPile.Add(card);
            }
        }

        currentHand.Clear();

        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }

        Debug.Log($"[GameManager] Descarte completado. Cartas en pila de descarte: {discardPile.Count}");
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

        if (playerController != null && playerController.HasBlockActive())
        {
            PlayBlockHitSound();
        }
    }

    public void OnEnemyTurnEnd()
    {
        if (playerController != null && playerController.HasBlockActive())
        {
            Debug.Log("Turno del enemigo terminado. El bloqueo del jugador se desactivará automáticamente.");
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
        Debug.Log($"Interacciones bloqueadas: {blocked}");
    }

    public bool AreInteractionsBlocked()
    {
        return interactionsBlocked || waitingForDialogue;
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
}