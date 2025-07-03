using UnityEngine;

public class CardUpgradeShopItem : ShopItem
{
    [Header("Configuración de Mejora")]
    [SerializeField] private CardData cardToUpgrade;
    [SerializeField] private float baseValueUpgradeAmount = 5f;
    [SerializeField] private float damageMultiplierUpgradeAmount = 0.1f;
    [SerializeField] private bool unlimitedPurchases = false;

    protected override void DeliverItem()
    {
        if (cardToUpgrade != null)
        {
            // Aplicar mejoras
            cardToUpgrade.individualBaseValueUpgrade += baseValueUpgradeAmount;
            cardToUpgrade.individualDamageMultiplier += damageMultiplierUpgradeAmount;

            // Guardar mejoras
            SaveCardUpgrades(cardToUpgrade);

            // Actualizar UI
            if (HandManager.Instance != null)
            {
                HandManager.Instance.RefreshHand();
            }
        }
        else
        {
            Debug.LogError("CardToUpgrade no asignado en CardUpgradeShopItem");
        }
    }

    private void SaveCardUpgrades(CardData card)
    {
        PlayerPrefs.SetFloat($"{card.cardName}_baseUpgrade", card.individualBaseValueUpgrade);
        PlayerPrefs.SetFloat($"{card.cardName}_damageMult", card.individualDamageMultiplier);
        PlayerPrefs.Save();
    }
}