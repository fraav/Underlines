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
            CleanBattleReferences();

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
                playerHealth = player.AddComponent<HealthSystem>();
            playerHealth.OnTakeDamage.AddListener(dmg => PlaySound(playerHurtSound));
        }

        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null)
        {
            enemyController = enemy.GetComponent<EnemyController>();
            enemyHealth = enemy.GetComponent<HealthSystem>();
            if (enemyHealth == null)
                enemyHealth = enemy.AddComponent<HealthSystem>();
            enemyHealth.OnTakeDamage.AddListener(dmg => PlaySound(enemyHurtSound));

            if (enemyController != null)
            {
                enemyController.OnEnemyAttack.AddListener(() => PlaySound(enemyAttackSound));
                enemyController.OnEnemyHeal.AddListener(() => PlaySound(enemyHealSound));
            }
        }
    }

    private void InitializeGameForScene()
    {
        if (!isBattleScene) return;

        currentTurn = TurnState.PlayerTurn;

        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(100);
            playerHealth.LoadHealth(PlayerHealthKey, 100);
            playerHealth.OnHealthChanged.AddListener(h => playerHealth.SaveHealth(PlayerHealthKey));
            playerHealth.OnDeath.AddListener(OnPlayerDeath);
        }

        if (enemyHealth != null)
            enemyHealth.OnDeath.AddListener(OnEnemyDefeated);

        StartCoroutine(InitializeBattle());
    }

    private IEnumerator InitializeBattle()
    {
        yield return new WaitForEndOfFrame();
        ResetCardSystemForNewBattle();

        // Fade in al inicio de la batalla
        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeIn();
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
            HandManager.Instance.SetInteractable(true);
    }

    public void OnEnemyDefeated()
    {
        if (isTransitioning) return;

        Debug.Log("Enemy defeated! Victory.");
        PlaySound(victorySound);
        CancelSelection();
        HandManager.Instance?.RefreshHand();
        StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        string nextScene = SceneManager.GetActiveScene().name == "BattleScene" ? "Shop" : "Credits";

        // Fade out → cargar → fade in
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeToScene(nextScene);
            yield break;
        }
        else
        {
            SceneManager.LoadScene(nextScene);
            yield break;
        }
    }

    private void OnPlayerDeath()
    {
        if (isTransitioning) return;

        Debug.Log("Player defeated! Game Over.");
        PlaySound(defeatSound);
        StartCoroutine(DefeatRoutine());
    }

    private IEnumerator DefeatRoutine()
    {
        isTransitioning = true;

        // Fade out usando FadeManager
        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeOut();
        else
            yield return new WaitForSeconds(1f);

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
            selectedCardDisplay.SetSelected(false);

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
        switch (card.cardType)
        {
            case CardData.CardType.Attack:
                PlaySound(attackCardSound);
                Card_Attack(card);
                break;
            case CardData.CardType.Block:
                PlaySound(blockCardSound);
                Card_Block(card);
                break;
            case CardData.CardType.Heal:
                PlaySound(healCardSound);
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
            enemyHealth?.TakeDamage((int)finalDamage);
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurn());

        playerController.PlayCardAnimation(card, ApplyDamage, CompleteTurn);
    }

    public void Card_Block(CardData card)
    {
        float blockValue = (card.baseValue + card.individualBaseValueUpgrade) * blockMultiplier;
        float reductionMultiplier = 1f - (blockValue / 100f);

        void ApplyBlock()
        {
            enemyController?.ApplyAttackReduction(reductionMultiplier);
            Debug.Log($"Bloqueo aplicado. Multiplicador de ataque enemigo reducido a: {reductionMultiplier}");
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurn());

        playerController.PlayCardAnimation(card, ApplyBlock, CompleteTurn);
    }

    public void Card_Heal(CardData card)
    {
        float finalHeal = (card.baseValue + card.individualBaseValueUpgrade) * healMultiplier;

        void ApplyHeal()
        {
            playerHealth?.Heal((int)finalHeal);
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurn());

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
        enemyController?.ResetAttackMultiplier();
        currentTurn = TurnState.PlayerTurn;
        HandManager.Instance.SetInteractable(true);
    }

    public void ShuffleDeck()
    {
        for (int i = availableDeck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            var temp = availableDeck[i];
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
        var drawnCards = availableDeck.Take(drawCount).ToList();

        foreach (var card in drawnCards)
        {
            currentHand.Add(card);
            availableDeck.Remove(card);
        }

        if (availableDeck.Count == 0)
        {
            availableDeck.AddRange(allCards);
            ShuffleDeck();
        }

        HandManager.Instance?.RefreshHand();
    }

    void LoadCardUpgrades()
    {
        foreach (var card in allCards)
        {
            card.individualBaseValueUpgrade = PlayerPrefs.GetFloat($"{card.cardName}_baseUpgrade", 0f);
            card.individualDamageMultiplier = PlayerPrefs.GetFloat($"{card.cardName}_damageMult", 1f);
        }
    }

    public void ResetGameState()
    {
        damageMultiplier = 1f;
        blockMultiplier = 1f;
        healMultiplier = 1f;

        foreach (var card in allCards)
        {
            card.individualBaseValueUpgrade = 0f;
            card.individualDamageMultiplier = 1f;
            PlayerPrefs.SetFloat($"{card.cardName}_baseUpgrade", 0f);
            PlayerPrefs.SetFloat($"{card.cardName}_damageMult", 1f);
        }

        PlayerPrefs.Save();
        ResetCardSystemForNewBattle();
        
        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(100);
            PlayerPrefs.DeleteKey(PlayerHealthKey);
        }
    }
    
    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volume);
    }
}
