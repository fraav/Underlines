using UnityEngine;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private int itemPrice;
    [SerializeField] private string itemID;
    [SerializeField] private bool consumable = true; // Si es consumible, se desactiva despu�s de comprar

    public void AttemptPurchase()
    {
        if (EconomyManager.Instance.TryPurchaseItem(itemPrice))
        {
            Debug.Log($"Compra exitosa: {itemID}");

            // Entregar el �tem (l�gica espec�fica en clases hijas)
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
        // Implementaci�n espec�fica en clases hijas
    }
}