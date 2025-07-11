using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

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

    public CardData selectedCard { get; private set; }
    public CardDisplay selectedCardDisplay { get; private set; }

    private bool playerIsValidTarget;
    private bool enemyIsValidTarget;
    private const string PlayerHealthKey = "PlayerCurrentHealth";

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
        Debug.Log("Enemy defeated! Victory.");
        CancelSelection();
        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("Player defeated! Game Over.");
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
        switch (card.cardType)
        {
            case CardData.CardType.Attack:
                Card_Attack(card);
                break;
            case CardData.CardType.Block:
                Card_Block(card);
                break;
            case CardData.CardType.Heal:
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
            }
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
            if (enemyController != null)
            {
                enemyController.ApplyAttackReduction(reductionMultiplier);
                Debug.Log($"Bloqueo aplicado. Multiplicador de ataque enemigo reducido a: {reductionMultiplier}");
            }
        }

        void CompleteTurn() => StartCoroutine(EndPlayerTurn());

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
                /// Debug.Log($"Curación aplicada. Salud actual: {playerHealth.currentHealth}");
            }
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
        if (enemyController != null)
        {
            enemyController.ResetAttackMultiplier();
        }

        currentTurn = TurnState.PlayerTurn;
        HandManager.Instance.SetInteractable(true);
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
}