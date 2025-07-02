using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardPurchaseShopItem : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardCostText;
    [SerializeField] private Button purchaseButton;

    private CardData cardData;
    private int cost;

    void Start()
    {
        purchaseButton.onClick.AddListener(TryPurchase);
    }

    public void SetCard(CardData card)
    {
        cardData = card;
        UpdateUI();
    }

    public void SetCost(int newCost)
    {
        cost = newCost;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (cardData == null) return;

        // Usar shopIcon si está disponible, de lo contrario usar icon
        if (cardData.shopIcon != null)
        {
            cardImage.sprite = cardData.shopIcon;
        }
        else
        {
            cardImage.sprite = cardData.icon;
        }

        cardNameText.text = cardData.cardName;
        cardCostText.text = $"{cost}G";
    }

    private void TryPurchase()
    {
        if (EconomyManager.Instance != null && EconomyManager.Instance.TryPurchaseItem(cost))
        {
            DeliverItem();
        }
        else
        {
            Debug.LogWarning("EconomyManager no encontrado o fondos insuficientes");
        }
    }

    public void DeliverItem()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.AddCardToDeck(cardData);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("ShopManager no encontrado!");
        }
    }
}