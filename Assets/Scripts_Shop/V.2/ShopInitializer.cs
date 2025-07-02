using UnityEngine;

public class ShopInitializer : MonoBehaviour
{
    void Start()
    {
        // Inicializar la tienda cuando se entra en la escena
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.InitializeShop();
        }

        // Actualizar UI de dinero
        if (MoneyUI.Instance != null && EconomyManager.Instance != null)
        {
            MoneyUI.Instance.UpdateMoneyText(EconomyManager.Instance.CurrentMoney);
        }
    }
}