using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Item de tienda para comprar cartas que se añaden permanentemente al deck
/// </summary>
public class CardPurchaseShopItemV2 : ShopItem
{
    [Header("Configuración de Carta")]
    [SerializeField] private CardData cardToPurchase;
    [SerializeField] private bool unlimitedPurchases = false;

    [Header("UI References")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardDescriptionText;
    [SerializeField] private TMP_Text priceText;

    void Start()
    {
        UpdateUI();
    }

    protected override void DeliverItem()
    {
        if (cardToPurchase == null)
        {
            Debug.LogError("[CardPurchaseShopItemV2] CardToPurchase no asignado");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[CardPurchaseShopItemV2] GameManager.Instance no encontrado");
            return;
        }

        Debug.Log($"[CardPurchaseShopItemV2] ===== COMPRANDO CARTA =====");
        Debug.Log($"[CardPurchaseShopItemV2] Carta: {cardToPurchase.cardName}");
        Debug.Log($"[CardPurchaseShopItemV2] Tipo: {cardToPurchase.cardType}");

        // Añadir la carta al deck permanente del jugador
        if (!GameManager.Instance.allCards.Contains(cardToPurchase))
        {
            GameManager.Instance.AddCardToPermanentDeck(cardToPurchase);
            Debug.Log($"[CardPurchaseShopItemV2] ✓ Carta {cardToPurchase.cardName} añadida al deck permanente");
            Debug.Log($"[CardPurchaseShopItemV2] Total de cartas en deck: {GameManager.Instance.allCards.Count}");
        }
        else
        {
            Debug.LogWarning($"[CardPurchaseShopItemV2] La carta {cardToPurchase.cardName} ya está en el deck");
        }

        // Guardar el estado del deck
        SaveCardToDeck();

        // Si no es compra ilimitada, desactivar el item
        if (!unlimitedPurchases)
        {
            gameObject.SetActive(false);
            Debug.Log($"[CardPurchaseShopItemV2] Item desactivado (compra única)");
        }
        else
        {
            UpdateUI();
            Debug.Log($"[CardPurchaseShopItemV2] Item actualizado (compra ilimitada)");
        }

        Debug.Log($"[CardPurchaseShopItemV2] ===== COMPRA COMPLETADA =====");
    }

    private void UpdateUI()
    {
        if (cardToPurchase == null) return;

        // Actualizar imagen de la carta
        if (cardImage != null)
        {
            if (cardToPurchase.shopIcon != null)
            {
                cardImage.sprite = cardToPurchase.shopIcon;
            }
            else if (cardToPurchase.icon != null)
            {
                cardImage.sprite = cardToPurchase.icon;
            }
        }

        // Actualizar nombre
        if (cardNameText != null)
        {
            cardNameText.text = cardToPurchase.cardName;
        }

        // Actualizar descripción
        if (cardDescriptionText != null)
        {
            cardDescriptionText.text = GetCardDescription();
        }

        // Actualizar precio
        if (priceText != null)
        {
            // El precio se obtiene de la clase base ShopItem
            // Necesitamos acceder al itemPrice, pero está privado en ShopItem
            // Por ahora, el precio se mostrará desde el inspector o desde otro sistema
        }
    }

    private string GetCardDescription()
    {
        if (cardToPurchase == null) return "";

        switch (cardToPurchase.cardType)
        {
            case CardData.CardType.Booster:
                return GetBoosterDescription();
            case CardData.CardType.Attack:
                return $"Ataque: {cardToPurchase.baseValue}";
            case CardData.CardType.Block:
                return $"Bloqueo: {cardToPurchase.baseValue}%";
            case CardData.CardType.Heal:
                return $"Curación: {cardToPurchase.baseValue}";
            default:
                return string.IsNullOrEmpty(cardToPurchase.description) ? "Sin descripción" : cardToPurchase.description;
        }
    }

    private string GetBoosterDescription()
    {
        if (cardToPurchase.cardType != CardData.CardType.Booster) return "";

        switch (cardToPurchase.boosterEffectType)
        {
            case CardData.BoosterEffectType.DoubleAction:
                return "Duplica la próxima acción";
            case CardData.BoosterEffectType.IncreaseDamage:
                return $"Aumenta el daño en {cardToPurchase.boosterValue * 100}%";
            case CardData.BoosterEffectType.IncreaseBlock:
                return $"Aumenta el bloqueo en {cardToPurchase.boosterValue * 100}%";
            case CardData.BoosterEffectType.IncreaseHeal:
                return $"Aumenta la curación en {cardToPurchase.boosterValue * 100}%";
            case CardData.BoosterEffectType.Heal:
                return $"Curación instantánea: {cardToPurchase.baseValue} HP";
            default:
                return cardToPurchase.description;
        }
    }

    private void SaveCardToDeck()
    {
        // Guardar que esta carta está en el deck
        string key = $"CardInDeck_{cardToPurchase.cardName}";
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        Debug.Log($"[CardPurchaseShopItemV2] Carta guardada en el deck: {key}");
    }

    /// <summary>
    /// Configura la carta que se puede comprar
    /// </summary>
    public void SetCard(CardData card)
    {
        cardToPurchase = card;
        UpdateUI();
    }
}

