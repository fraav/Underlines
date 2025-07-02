using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [SerializeField] private Transform shopItemsContainer;
    [SerializeField] private GameObject cardPurchasePrefab;
    [SerializeField] private GameObject cardUpgradePrefab;

    [Header("Configuración Tienda")]
    [SerializeField] private int itemsToShow = 3;
    [SerializeField] private int maxCardsToOffer = 5;
    [SerializeField] private int upgradeCost = 50;
    [SerializeField] private int newCardCost = 100;

    [Header("Lista Maestra de Cartas")]
    [SerializeField] private List<CardData> allPossibleCards = new List<CardData>();

    private List<CardData> availableCards = new List<CardData>();
    private List<CardData> cardsInDeck = new List<CardData>();
    private List<GameObject> currentShopItems = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeShop()
    {
        // Obtener cartas del jugador
        cardsInDeck = GameManager.Instance.allCards;

        // Determinar cartas disponibles para compra
        availableCards = allPossibleCards
            .Where(card => !cardsInDeck.Contains(card))
            .Take(maxCardsToOffer)
            .ToList();

        GenerateShopItems();
    }

    private void GenerateShopItems()
    {
        // Limpiar tienda anterior
        ClearShop();

        // Generar items de mejora para cartas en el deck
        GenerateUpgradeItems();

        // Generar items para comprar nuevas cartas
        GeneratePurchaseItems();
    }

    private void GenerateUpgradeItems()
    {
        // Seleccionar cartas aleatorias del deck para mejorar
        var upgradeCandidates = cardsInDeck
            .OrderBy(x => Random.value)
            .Take(itemsToShow / 2)
            .ToList();

        foreach (var card in upgradeCandidates)
        {
            GameObject item = Instantiate(cardUpgradePrefab, shopItemsContainer);
            CardUpgradeShopItem upgradeItem = item.GetComponent<CardUpgradeShopItem>();

            if (upgradeItem != null)
            {
                upgradeItem.SetCard(card);
                upgradeItem.SetCost(upgradeCost);
                currentShopItems.Add(item);
            }
            else
            {
                Debug.LogError("Componente CardUpgradeShopItem no encontrado en el prefab");
            }
        }
    }

    private void GeneratePurchaseItems()
    {
        // Seleccionar cartas aleatorias disponibles para comprar
        var purchaseCandidates = availableCards
            .OrderBy(x => Random.value)
            .Take(itemsToShow - (itemsToShow / 2))
            .ToList();

        foreach (var card in purchaseCandidates)
        {
            GameObject item = Instantiate(cardPurchasePrefab, shopItemsContainer);
            CardPurchaseShopItem purchaseItem = item.GetComponent<CardPurchaseShopItem>();

            if (purchaseItem != null)
            {
                purchaseItem.SetCard(card);
                purchaseItem.SetCost(newCardCost);
                currentShopItems.Add(item);
            }
            else
            {
                Debug.LogError("Componente CardPurchaseShopItem no encontrado en el prefab");
            }
        }
    }

    private void ClearShop()
    {
        foreach (var item in currentShopItems)
        {
            Destroy(item);
        }
        currentShopItems.Clear();
    }

    public void AddCardToDeck(CardData newCard)
    {
        if (!GameManager.Instance.allCards.Contains(newCard))
        {
            GameManager.Instance.allCards.Add(newCard);
        }

        // Guardar el estado de la carta
        PlayerPrefs.SetFloat($"{newCard.cardName}_baseUpgrade", newCard.individualBaseValueUpgrade);
        PlayerPrefs.SetFloat($"{newCard.cardName}_damageMult", newCard.individualDamageMultiplier);
        PlayerPrefs.Save();

        // Actualizar la tienda
        InitializeShop();
    }
}