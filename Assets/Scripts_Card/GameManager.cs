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

    public enum TurnState { PlayerTurn, EnemyTurn, SelectingTarget, WaitingForDialogue, SelectingAssistance, SelectingTrigger }
    public TurnState currentTurn { get; private set; }

    [Header("Game Settings")]
    public float damageMultiplier = 1.0f;
    public float blockMultiplier = 1.0f;
    public float healMultiplier = 1.0f;

    [Header("Card System")]
    public List<CardData> allCards = new List<CardData>();
    private List<CardData> availableDeck = new List<CardData>();
    public List<CardData> currentHand { get; private set; } = new List<CardData>();
    public List<CardData> selectedAssistanceCards { get; private set; } = new List<CardData>();

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
    
    [Header("Trigger Buttons")]
    public Button attackTriggerButton;
    public Button blockTriggerButton;
    public Button healTriggerButton;
    public GameObject triggerButtonsPanel;

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
    
    // ►►► NUEVAS VARIABLES PARA SISTEMA DE ASISTENCIA ◄◄◄
    private bool assistancePhaseCompleted = false;
    private CardData.CardType selectedTriggerType;

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

        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.StartCoroutine(
                SceneTransitionManager.Instance.FadeIn()
            );
        }
    }

    private void ResetCardSystemForNewBattle()
    {
        currentHand.Clear();
        selectedAssistanceCards.Clear();
        
        availableDeck.Clear();
        availableDeck.AddRange(allCards);
        
        ShuffleDeck();
        
        assistancePhaseCompleted = false;
        currentTurn = TurnState.PlayerTurn;
        
        // Iniciar fase de asistencia
        StartAssistancePhase();

        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(true);
        }
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

        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.StartCoroutine(
                SceneTransitionManager.Instance.FadeOut()
            );
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

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

        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.StartCoroutine(
                SceneTransitionManager.Instance.FadeOut()
            );
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene("MainMenu");

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

        switch (card.cardType)
        {
            case CardData.CardType.Attack:
                playerIsValidTarget = false;
                enemyIsValidTarget = true;
                break;
            case CardData.CardType.Block:
            case CardData.CardType.Heal:
            case CardData.CardType.Assistance:
                playerIsValidTarget = true;
                enemyIsValidTarget = false;
                break;
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
        
        // Si es carta de asistencia, seleccionarla para acumular efectos
        if (selectedCard.cardType == CardData.CardType.Assistance)
        {
            SelectAssistanceCard(selectedCard);
        }
        else
        {
            // Para cartas normales, ejecutar acción inmediatamente
            RemoveCardFromHand(selectedCard);
            ExecuteCardAction(selectedCard);
        }

        if (selectedCardDisplay != null)
        {
            selectedCardDisplay.SetSelected(false);
            selectedCardDisplay = null;
        }

        currentTurn = TurnState.PlayerTurn;
        selectedCard = null;
    }

    private void RemoveCardFromHand(CardData card)
    {
        if (currentHand.Contains(card))
        {
            currentHand.Remove(card);
            availableDeck.Add(card);
            HandManager.Instance.RefreshHand();

            if (currentHand.Count == 0)
            {
                ShuffleDeck();
                DrawNewHand();
            }
        }
    }

    private void ExecuteCardAction(CardData card)
    {
        Debug.Log($"ExecuteCardAction called with card: {card.cardName}, type: {card.cardType}");

        switch (card.cardType)
        {
            case CardData.CardType.Attack:
                Debug.Log("Executing Attack card");
                PlayAttackCardSound();
                Card_Attack(card);
                break;
            case CardData.CardType.Block:
                Debug.Log("Executing Block card");
                PlayBlockCardSound();
                Card_Block(card);
                break;
            case CardData.CardType.Heal:
                Debug.Log("Executing Heal card");
                PlayHealCardSound();
                Card_Heal(card);
                break;
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
        currentTurn = TurnState.EnemyTurn;
        HandManager.Instance.SetInteractable(false);
        
        // Ocultar botones de gatillo
        ShowTriggerButtons(false);
        
        // Descartar cartas de asistencia restantes
        DiscardRemainingAssistanceCards();
        
        yield return new WaitForSeconds(0.5f);
        enemyController.StartEnemyTurn();
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
        if (enemyController != null)
        {
            enemyController.ResetAttackMultiplier();
        }

        if (playerController != null)
        {
            playerController.DeactivateBlock();
        }

        // Iniciar el nuevo sistema de turno del jugador
        currentTurn = TurnState.PlayerTurn;
        StartAssistancePhase();
        
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(true);
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

    public void DrawNewHand()
    {
        currentHand.Clear();

        if (availableDeck.Count == 0)
        {
            availableDeck.AddRange(allCards);
            ShuffleDeck();
        }

        int drawCount = Mathf.Min(4, availableDeck.Count);
        List<CardData> drawnCards = availableDeck.Take(drawCount).ToList();

        foreach (CardData card in drawnCards)
        {
            if (card != null)
            {
                currentHand.Add(card);
                availableDeck.Remove(card);
            }
        }

        if (availableDeck.Count == 0)
        {
            availableDeck.AddRange(allCards);
            ShuffleDeck();
        }

        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }
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
    
    // ►►► NUEVOS MÉTODOS PARA SISTEMA DE ASISTENCIA ◄◄◄
    
    private void StartAssistancePhase()
    {
        currentTurn = TurnState.SelectingAssistance;
        DrawNewHand(); // Usar el método existente para robar 4 cartas
        ShowTriggerButtons(false);
        
        Debug.Log("Iniciando fase de asistencia - Robar 4 cartas de asistencia");
    }
    
    public void SelectAssistanceCard(CardData card)
    {
        if (selectedAssistanceCards.Contains(card))
        {
            selectedAssistanceCards.Remove(card);
            Debug.Log($"Carta de asistencia deseleccionada: {card.cardName}");
        }
        else if (selectedAssistanceCards.Count < 4)
        {
            selectedAssistanceCards.Add(card);
            Debug.Log($"Carta de asistencia seleccionada: {card.cardName} ({selectedAssistanceCards.Count}/4)");
        }
        else
        {
            Debug.Log("Ya tienes 4 cartas de asistencia seleccionadas");
        }
        
        // Actualizar visuales de selección
        UpdateCardSelectionVisuals();
    }
    
    public void ConfirmAssistanceSelection()
    {
        if (currentTurn != TurnState.SelectingAssistance) return;
        
        assistancePhaseCompleted = true;
        currentTurn = TurnState.SelectingTrigger;
        ShowTriggerButtons(true);
        
        Debug.Log($"Fase de asistencia completada con {selectedAssistanceCards.Count} cartas seleccionadas");
    }
    
    private void ShowTriggerButtons(bool show)
    {
        if (triggerButtonsPanel != null)
        {
            triggerButtonsPanel.SetActive(show);
        }
    }
    
    public void SelectTrigger(CardData.CardType triggerType)
    {
        if (currentTurn != TurnState.SelectingTrigger) return;
        
        selectedTriggerType = triggerType;
        ExecuteTriggerAction(triggerType);
    }
    
    private void ExecuteTriggerAction(CardData.CardType triggerType)
    {
        Debug.Log($"Ejecutando gatillo: {triggerType}");
        
        // Aplicar efectos acumulables de las cartas de asistencia
        float totalMultiplier = 1.0f;
        foreach (CardData assistanceCard in selectedAssistanceCards)
        {
            if (assistanceCard.cardType == CardData.CardType.Assistance)
            {
                if (assistanceCard.assistanceType == CardData.AssistanceType.DoubleEffect)
                {
                    totalMultiplier *= assistanceCard.effectMultiplier;
                }
            }
        }
        
        // Crear una carta temporal para ejecutar la acción
        CardData tempCard = ScriptableObject.CreateInstance<CardData>();
        tempCard.cardType = triggerType;
        tempCard.baseValue = 10; // Valor base para el gatillo
        tempCard.cardName = triggerType.ToString() + " Gatillo";
        
        // Aplicar multiplicadores acumulables
        switch (triggerType)
        {
            case CardData.CardType.Attack:
                damageMultiplier *= totalMultiplier;
                break;
            case CardData.CardType.Block:
                blockMultiplier *= totalMultiplier;
                break;
            case CardData.CardType.Heal:
                healMultiplier *= totalMultiplier;
                break;
        }
        
        // Ejecutar la acción correspondiente
        switch (triggerType)
        {
            case CardData.CardType.Attack:
                Card_Attack(tempCard);
                break;
            case CardData.CardType.Block:
                Card_Block(tempCard);
                break;
            case CardData.CardType.Heal:
                Card_Heal(tempCard);
                break;
        }
        
        // Restaurar multiplicadores
        damageMultiplier = 1.0f;
        blockMultiplier = 1.0f;
        healMultiplier = 1.0f;
        
        // Quitar cartas de asistencia seleccionadas de la mano
        foreach (CardData assistanceCard in selectedAssistanceCards)
        {
            if (currentHand.Contains(assistanceCard))
            {
                currentHand.Remove(assistanceCard);
                availableDeck.Add(assistanceCard);
            }
        }
        
        // Limpiar selección y terminar turno
        DiscardRemainingAssistanceCards();
        StartCoroutine(EndPlayerTurnAfterDialogue());
    }
    
    private void DiscardRemainingAssistanceCards()
    {
        // Mover todas las cartas de la mano al descarte (mazo disponible)
        foreach (CardData card in currentHand)
        {
            availableDeck.Add(card);
        }
        
        // Limpiar las listas
        currentHand.Clear();
        selectedAssistanceCards.Clear();
        
        // Actualizar la mano visual
        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }
        
        Debug.Log("Todas las cartas descartadas al final del turno");
    }
    
    // Métodos públicos para los botones de gatillo
    public void OnAttackTriggerClicked()
    {
        SelectTrigger(CardData.CardType.Attack);
    }
    
    public void OnBlockTriggerClicked()
    {
        SelectTrigger(CardData.CardType.Block);
    }
    
    public void OnHealTriggerClicked()
    {
        SelectTrigger(CardData.CardType.Heal);
    }
    
    private void UpdateCardSelectionVisuals()
    {
        if (HandManager.Instance != null)
        {
            foreach (GameObject cardObj in HandManager.Instance.spawnedCards)
            {
                if (cardObj != null)
                {
                    CardDisplay display = cardObj.GetComponent<CardDisplay>();
                    if (display != null && display.currentCard != null)
                    {
                        bool isSelected = selectedAssistanceCards.Contains(display.currentCard);
                        display.SetSelected(isSelected);
                    }
                }
            }
        }
    }
}