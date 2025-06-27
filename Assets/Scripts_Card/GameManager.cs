using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Variables mejorables
    public float damageMultiplier = 1.0f;
    public float blockMultiplier = 1.0f;
    public float healMultiplier = 1.0f;

    // Sistema de cartas
    public List<CardData> allCards = new List<CardData>();
    private List<CardData> availableDeck = new List<CardData>();
    public List<CardData> currentHand { get; private set; } = new List<CardData>();


    private const float InitialDamageMultiplier = 1.0f;
    private const float InitialBlockMultiplier = 1.0f;
    private const float InitialHealMultiplier = 1.0f;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCardUpgrades();
            InitializeDeck();
        }
        else
        {
            Destroy(gameObject);
        }
    }

        public void ResetGameState()
    {
        // Resetear multiplicadores globales
        damageMultiplier = InitialDamageMultiplier;
        blockMultiplier = InitialBlockMultiplier;
        healMultiplier = InitialHealMultiplier;

        // Resetear mejoras individuales de cartas
        foreach (CardData card in allCards)
        {
            card.individualBaseValueUpgrade = 0f;
            card.individualDamageMultiplier = 1.0f;
            
            // Guardar los valores reseteados
            PlayerPrefs.SetFloat($"{card.cardName}_baseUpgrade", 0f);
            PlayerPrefs.SetFloat($"{card.cardName}_damageMult", 1.0f);
        }

        PlayerPrefs.Save();

        // Reiniciar el mazo
        InitializeDeck();
    }

    void LoadCardUpgrades()
    {
        foreach (CardData card in allCards)
        {
            card.individualBaseValueUpgrade = PlayerPrefs.GetFloat($"{card.cardName}_baseUpgrade", 0f);
            card.individualDamageMultiplier = PlayerPrefs.GetFloat($"{card.cardName}_damageMult", 1.0f);
        }
    }

    void InitializeDeck()
    {
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogError("No hay cartas asignadas en allCards!");
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
            Debug.LogWarning("No hay cartas disponibles en el mazo! Reciclando...");
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
            else
            {
                Debug.LogWarning("Se encontr� carta nula en el mazo!");
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
        else
        {
            Debug.Log("HandManager.Instance a�n no est� disponible");
        }
    }

    public void PlayCard(CardData card)
    {
        if (card == null || !currentHand.Contains(card))
        {
            Debug.LogWarning("Intento de jugar carta inv�lida");
            return;
        }

        currentHand.Remove(card);
        availableDeck.Add(card);

        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }

        if (currentHand.Count == 0)
        {
            ShuffleDeck();
            DrawNewHand();
        }
    }

    // Funciones de cartas (modificadas para usar mejoras individuales)
    public void Card_Attack(CardData card)
    {
        float finalDamage = (card.baseValue + card.individualBaseValueUpgrade) *
                          damageMultiplier * card.individualDamageMultiplier;
        Debug.Log($"Atacando por {finalDamage} puntos");
    }

    public void Card_Block(CardData card)
    {
        float finalBlock = (card.baseValue + card.individualBaseValueUpgrade) * blockMultiplier;
        Debug.Log($"Bloqueando {finalBlock} puntos");
    }

    public void Card_Heal(CardData card)
    {
        float finalHeal = (card.baseValue + card.individualBaseValueUpgrade) * healMultiplier;
        Debug.Log($"Curando {finalHeal} puntos");
    }

    // Funciones para tienda (mejoras globales)
    public void UpgradeDamage(float upgrade)
    {
        damageMultiplier += upgrade;
        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }
    }

    public void UpgradeBlock(float upgrade)
    {
        blockMultiplier += upgrade;
        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }
    }

    public void UpgradeHeal(float upgrade)
    {
        healMultiplier += upgrade;
        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }
    }
}