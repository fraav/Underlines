using UnityEngine;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private int itemPrice;
    [SerializeField] private string itemID;
    [SerializeField] private bool consumable = true; // Si es consumible, se desactiva después de comprar

    public void AttemptPurchase()
    {
        if (EconomyManager.Instance.TryPurchaseItem(itemPrice))
        {
            Debug.Log($"Compra exitosa: {itemID}");

            // Entregar el ítem (lógica específica en clases hijas)
            DeliverItem();

            if (consumable)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("Dinero insuficiente");
            // Feedback visual/sonoro
        }
    }

    // Virtual para permitir override en clases hijas
    protected virtual void DeliverItem()
    {
        // Implementación específica en clases hijas
    }
}