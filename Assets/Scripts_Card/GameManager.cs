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

    public enum TurnState { PlayerTurn, EnemyTurn, SelectingTarget }
    public TurnState currentTurn { get; private set; }

    [Header("Game Settings")]
    public float damageMultiplier = 1.0f;
    public float blockMultiplier = 1.0f;
    public float healMultiplier = 1.0f;

    [Header("Card System")]
    public List<CardData> allCards = new List<CardData>();
    private List<CardData> availableDeck = new List<CardData>();
    public List<CardData> currentHand { get; private set; } = new List<CardData>();

    [Header("References")]
    public EnemyController enemyController;
    public PlayerController playerController;
    public HealthSystem playerHealth;
    public HealthSystem enemyHealth;

    [Header("Scene Settings")]
    public bool isBattleScene = false;

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

    private bool isTransitioning = false;
    private bool playerIsValidTarget;
    private bool enemyIsValidTarget;
    private const string PlayerHealthKey = "PlayerCurrentHealth";
    
    // Campos privados con propiedades públicas para acceso externo
    private CardData selectedCard;
    private CardDisplay selectedCardDisplay;
    public CardData SelectedCard => selectedCard;
    public CardDisplay SelectedCardDisplay => selectedCardDisplay;

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

        // Clear references when leaving battle scene
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

        // Clear current hand
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            playerHealth = player.GetComponent<HealthSystem>();

            if (playerHealth == null)
            {
                playerHealth = player.AddComponent<HealthSystem>();
            }
            
            // Configurar sonidos de daño para el jugador
            playerHealth.OnTakeDamage.AddListener((damage) => {
                PlayPlayerDamageSound();
            });
        }

        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null)
        {
            enemyController = enemy.GetComponent<EnemyController>();
            enemyHealth = enemy.GetComponent<HealthSystem>();

            if (enemyHealth == null)
            {
                enemyHealth = enemy.AddComponent<HealthSystem>();
            }
            
            // Configurar sonidos de daño para el enemigo
            enemyHealth.OnTakeDamage.AddListener((damage) => {
                PlayEnemyDamageSound();
            });
            
            // Configurar sonidos para acciones del enemigo
            if (enemyController != null)
            {
                enemyController.OnEnemyAttack.AddListener(() => PlayEnemyAttackSound());
                enemyController.OnEnemyHeal.AddListener(() => PlayEnemyHealSound());
            }
        }
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
        
        // Fade in al inicio de la batalla
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
        availableDeck.Clear();
        availableDeck.AddRange(allCards);
        ShuffleDeck();
        DrawNewHand();

        currentTurn = TurnState.PlayerTurn;

        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(true);
        }
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
        
        // Fade out usando SceneTransitionManager
        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.StartCoroutine(
                SceneTransitionManager.Instance.FadeOut()
            );
        }
        else
        {
            // Implementación de respaldo
            yield return new WaitForSeconds(1f);
        }
        
        // Cambiar a la escena de mapa

        
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
        
        // Fade out usando SceneTransitionManager
        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.StartCoroutine(
                SceneTransitionManager.Instance.FadeOut()
            );
        }
        else
        {
            // Implementación de respaldo
            yield return new WaitForSeconds(1f);
        }
        
        // Cambiar al menú principal
        SceneManager.LoadScene("MainMenu");
        
        isTransitioning = false;
    }

    public void CancelSelection()
    {
        if (currentTurn == TurnState.SelectingTarget)
        {
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
                playerIsValidTarget = true;
                enemyIsValidTarget = false;
                break;
        }

        playerController?.SetHighlight(playerIsValidTarget);
        enemyController?.SetHighlight(enemyIsValidTarget);
    }

    public void SelectTarget(GameObject target)
    {
        if (currentTurn != TurnState.SelectingTarget || selectedCard == null) return;

        bool isValid = (target.CompareTag("Player") && playerIsValidTarget) ||
                      (target.CompareTag("Enemy") && enemyIsValidTarget);

        if (!isValid) return;

        playerController?.SetHighlight(false);
        enemyController?.SetHighlight(false);
        RemoveCardFromHand(selectedCard);
        ExecuteCardAction(selectedCard);

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
        // Reproducir sonido según el tipo de carta
        switch (card.cardType)
        {
            case CardData.CardType.Attack:
                PlayAttackCardSound();
                Card_Attack(card);
                break;
            case CardData.CardType.Block:
                PlayBlockCardSound();
                Card_Block(card);
                break;
            case CardData.CardType.Heal:
                PlayHealCardSound();
                Card_Heal(card);
                break;
        }
    }

    public void Card_Attack(CardData card)
    {
        float finalDamage = (card.baseValue + card.individualBaseValueUpgrade) *
                          damageMultiplier * card.individualDamageMultiplier;

        void ApplyDamage()
        {
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)finalDamage);
                // Reproducir sonido de daño del enemigo en el momento exacto del impacto
                PlayEnemyDamageSound();
                // Reproducir sonido de ataque en el momento exacto del impacto
                PlayAttackCardSound();
            }
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurn());

        // NO reproducir sonido aquí - se ejecutará en el punto de acción de la animación
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
                Debug.Log($"Bloqueo aplicado. Multiplicador de ataque enemigo reducido a: {reductionMultiplier}");
            }
            
            // Activar el bloqueo del jugador
            if (playerController != null)
            {
                playerController.ActivateBlock(reductionMultiplier);
                Debug.Log($"Bloqueo del jugador activado con multiplicador: {reductionMultiplier}");
            }
            
            // Reproducir sonido de bloqueo en el momento exacto de activación
            PlayBlockCardSound();
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurn());

        // NO reproducir sonido aquí - se ejecutará en el punto de acción de la animación
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
                // Reproducir sonido de curación en el momento exacto de aplicación
                PlayHealCardSound();
            }
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurn());

        // NO reproducir sonido aquí - se ejecutará en el punto de acción de la animación
        playerController.PlayCardAnimation(card, ApplyHeal, CompleteTurn);
    }

    public IEnumerator EndPlayerTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        HandManager.Instance.SetInteractable(false);
        yield return new WaitForSeconds(0.5f);
        enemyController.StartEnemyTurn();
    }

    public void StartPlayerTurn()
    {
        if (enemyController != null)
        {
            enemyController.ResetAttackMultiplier();
        }

        // Desactivar el bloqueo del jugador al iniciar su turno
        // Esto asegura que vuelva a su animación por defecto
        if (playerController != null)
        {
            playerController.DeactivateBlock();
        }

        currentTurn = TurnState.PlayerTurn;
        HandManager.Instance.SetInteractable(true);
    }

    /// <summary>
    /// Se llama cuando el enemigo inicia su animación de ataque
    /// Permite que el bloqueo del jugador se active simultáneamente
    /// </summary>
    public void OnEnemyAttackStart()
    {
        // Activar el bloqueo pendiente del jugador si tiene uno
        if (playerController != null && playerController.HasBlockPending())
        {
            playerController.ActivatePendingBlock();
        }
        
        // Activar la animación de bloqueo del jugador si tiene bloqueo activo
        if (playerController != null && playerController.HasBlockActive())
        {
            playerController.PlayBlockAnimation();
        }
    }
    
    /// <summary>
    /// Se llama cuando el enemigo aplica su ataque (en el punto de acción)
    /// Permite reproducir el sonido de bloqueo en el momento exacto del impacto
    /// </summary>
    public void OnEnemyAttackApplied()
    {
        // Reproducir sonido de ataque del enemigo en el momento exacto del impacto
        PlayEnemyAttackSound();
        
        // Reproducir sonido de bloqueo en el momento exacto del impacto
        if (playerController != null && playerController.HasBlockActive())
        {
            PlayBlockHitSound();
        }
    }
    
    /// <summary>
    /// Se llama cuando el enemigo termina su turno
    /// Permite que el bloqueo del jugador se desactive automáticamente si no fue golpeado
    /// </summary>
    public void OnEnemyTurnEnd()
    {
        // El bloqueo se desactivará automáticamente por el temporizador en PlayerController
        // Solo desactivar inmediatamente si el enemigo no atacó
        if (playerController != null && playerController.HasBlockActive())
        {
            // El bloqueo se mantendrá activo por un tiempo antes de desactivarse automáticamente
            Debug.Log("Turno del enemigo terminado. El bloqueo del jugador se desactivará automáticamente.");
        }
    }

    /// <summary>
    /// Reproduce el sonido de bloqueo del jugador
    /// </summary>
    public void PlayBlockHitSound()
    {
        PlayBlockCardSound();
        Debug.Log("¡Sonido de bloqueo ejecutado en el momento del impacto!");
    }

    /// <summary>
    /// Permite reproducir la animación de recibir daño del jugador
    /// Útil para cuando el jugador recibe daño mientras bloquea
    /// </summary>
    public void PlayPlayerDamageAnimation()
    {
        if (playerController != null)
        {
            playerController.PlayDamageAnimation();
            // Reproducir sonido de daño del jugador usando el método centralizado
            PlayPlayerDamageSound();
        }
    }
    
    /// <summary>
    /// Permite reproducir el sonido de daño del enemigo
    /// Útil para cuando el enemigo recibe daño
    /// </summary>
    public void PlayEnemyDamageSound()
    {
        PlaySound(enemyHurtSound);
    }

    /// <summary>
    /// Permite reproducir el sonido de daño del enemigo desde el EnemyController
    /// </summary>
    public void PlayEnemyDamageSoundFromController()
    {
        PlaySound(enemyHurtSound);
    }

    /// <summary>
    /// Reproduce el sonido de ataque del enemigo
    /// </summary>
    public void PlayEnemyAttackSound()
    {
        PlaySound(enemyAttackSound);
    }

    /// <summary>
    /// Reproduce el sonido de curación del enemigo
    /// </summary>
    public void PlayEnemyHealSound()
    {
        PlaySound(enemyHealSound);
    }

    /// <summary>
    /// Reproduce el sonido de daño del jugador
    /// </summary>
    public void PlayPlayerDamageSound()
    {
        PlaySound(playerHurtSound);
    }

    /// <summary>
    /// Reproduce el sonido de victoria
    /// </summary>
    public void PlayVictorySound()
    {
        PlaySound(victorySound);
    }

    /// <summary>
    /// Reproduce el sonido de derrota
    /// </summary>
    public void PlayDefeatSound()
    {
        PlaySound(defeatSound);
    }

    /// <summary>
    /// Reproduce el sonido de carta de ataque
    /// </summary>
    public void PlayAttackCardSound()
    {
        PlaySound(attackCardSound);
    }

    /// <summary>
    /// Reproduce el sonido de carta de bloqueo
    /// </summary>
    public void PlayBlockCardSound()
    {
        PlaySound(blockCardSound);
    }

    /// <summary>
    /// Reproduce el sonido de carta de curación
    /// </summary>
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
}