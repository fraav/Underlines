using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

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

    private CardData selectedCard;
    private bool playerIsValidTarget;
    private bool enemyIsValidTarget;
    private const string PlayerHealthKey = "PlayerCurrentHealth";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeGame()
    {
        LoadCardUpgrades();
        InitializeDeck();
        InitializeHealthSystems();
        currentTurn = TurnState.PlayerTurn;
        
        if (playerHealth != null) playerHealth.OnDeath.AddListener(OnPlayerDeath);
        if (enemyHealth != null) enemyHealth.OnDeath.AddListener(OnEnemyDefeated);
    }

    void InitializeHealthSystems()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth == null) playerHealth = player.AddComponent<HealthSystem>();
            playerHealth.SetMaxHealth(100);
            playerHealth.LoadHealth(PlayerHealthKey, 100);
            playerHealth.OnHealthChanged.AddListener((health) => {
                playerHealth.SaveHealth(PlayerHealthKey);
            });
        }

        if (enemyController != null)
        {
            enemyHealth = enemyController.GetComponent<HealthSystem>();
            if (enemyHealth == null) enemyHealth = enemyController.gameObject.AddComponent<HealthSystem>();
            enemyHealth.SetMaxHealth(100);
        }
    }

    public void StartTargetSelection(CardData card)
    {
        selectedCard = card;
        currentTurn = TurnState.SelectingTarget;
        
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
        
        if (playerController != null) playerController.SetHighlight(playerIsValidTarget);
        if (enemyController != null) enemyController.SetHighlight(enemyIsValidTarget);
    }

    public void SelectTarget(GameObject target)
    {
        if (currentTurn != TurnState.SelectingTarget || selectedCard == null) return;

        bool isValid = (target.CompareTag("Player") && playerIsValidTarget) || 
                      (target.CompareTag("Enemy") && enemyIsValidTarget);

        if (!isValid) return;

        if (playerController != null) playerController.SetHighlight(false);
        if (enemyController != null) enemyController.SetHighlight(false);

        // Remove card from hand before executing action
        RemoveCardFromHand(selectedCard);

        // Execute card action
        ExecuteCardAction(selectedCard);

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
            if (enemyHealth != null) enemyHealth.TakeDamage((int)finalDamage);
        }

        void CompleteTurn()
        {
            StartCoroutine(EndPlayerTurn());
        }

        playerController.PlayCardAnimation(card, ApplyDamage, CompleteTurn);
    }

    public void Card_Block(CardData card)
    {
        float finalBlock = (card.baseValue + card.individualBaseValueUpgrade) * blockMultiplier;
        StartCoroutine(EndPlayerTurn());
    }

    public void Card_Heal(CardData card)
    {
        float finalHeal = (card.baseValue + card.individualBaseValueUpgrade) * healMultiplier;
        if (playerHealth != null) playerHealth.Heal((int)finalHeal);
        StartCoroutine(EndPlayerTurn());
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
        currentTurn = TurnState.PlayerTurn;
        HandManager.Instance.SetInteractable(true);
    }

    void InitializeDeck()
    {
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogError("No cards assigned in allCards!");
            return;
        }

        availableDeck.Clear();
        availableDeck.AddRange(allCards);
        ShuffleDeck();
        DrawNewHand();
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

        if (availableDeck == null || availableDeck.Count == 0)
        {
            Debug.LogWarning("No cards available in deck! Reshuffling...");
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

    public void OnEnemyDefeated()
    {
        Debug.Log("Enemy defeated! Victory.");
    }

    private void OnPlayerDeath()
    {
        Debug.Log("Player defeated! Game Over.");
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
        InitializeDeck();
        
        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(100);
            PlayerPrefs.DeleteKey(PlayerHealthKey);
        }
    }
}