using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUpgradeShopItem : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text upgradeCostText;
    [SerializeField] private Button upgradeButton;

    [Header("Configuración Mejora")]
    [SerializeField] private float baseValueUpgradeAmount = 5f;
    [SerializeField] private float damageMultiplierUpgradeAmount = 0.1f;

    private CardData cardData;
    private int cost;

    void Start()
    {
        upgradeButton.onClick.AddListener(TryPurchaseUpgrade);
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
        upgradeCostText.text = $"{cost}G";
    }

    private void TryPurchaseUpgrade()
    {
        if (EconomyManager.Instance != null && EconomyManager.Instance.TryPurchaseItem(cost))
        {
            DeliverUpgrade();
        }
        else
        {
            Debug.LogWarning("EconomyManager no encontrado o fondos insuficientes");
        }
    }

    private void DeliverUpgrade()
    {
        // Aplicar mejoras
        cardData.individualBaseValueUpgrade += baseValueUpgradeAmount;
        cardData.individualDamageMultiplier += damageMultiplierUpgradeAmount;

        // Guardar mejoras
        SaveCardUpgrades(cardData);

        // Actualizar UI en juego
        if (HandManager.Instance != null)
        {
            HandManager.Instance.RefreshHand();
        }

        Destroy(gameObject);
    }

    private void SaveCardUpgrades(CardData card)
    {
        PlayerPrefs.SetFloat($"{card.cardName}_baseUpgrade", card.individualBaseValueUpgrade);
        PlayerPrefs.SetFloat($"{card.cardName}_damageMult", card.individualDamageMultiplier);
        PlayerPrefs.Save();
    }
}