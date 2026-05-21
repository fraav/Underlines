using UnityEngine;

/// <summary>
/// Compra de carta/objeto: se añade a allObjectCards (mazo separado de allCards).
/// </summary>
public class ObjectCardPurchaseShopItem : ShopItem
{
    [SerializeField] private ObjectCardData objectCardToPurchase;
    [SerializeField] private bool unlimitedPurchases = false;

    protected override void DeliverItem()
    {
        if (objectCardToPurchase == null)
        {
            Debug.LogError("[ObjectCardPurchaseShopItem] objectCardToPurchase no asignado");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[ObjectCardPurchaseShopItem] GameManager.Instance no encontrado");
            return;
        }

        GameManager.Instance.AddCardToPermanentObjectDeck(objectCardToPurchase);

        if (!unlimitedPurchases)
            gameObject.SetActive(false);
    }

    public void SetObjectCard(ObjectCardData card) => objectCardToPurchase = card;
}
