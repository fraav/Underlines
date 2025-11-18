using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Item de tienda para mejorar los valores base de los botones de acción (Ataque, Bloqueo, Curación)
/// </summary>
public class ActionButtonUpgradeShopItem : ShopItem
{
    [Header("Configuración de Mejora")]
    [SerializeField] private ActionButtonType buttonType;
    [SerializeField] private float baseValueUpgradeAmount = 5f;
    [SerializeField] private bool unlimitedPurchases = true;

    [Header("UI References")]
    [SerializeField] private TMP_Text buttonNameText;
    [SerializeField] private TMP_Text currentValueText;
    [SerializeField] private TMP_Text upgradeAmountText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image buttonIcon;

    public enum ActionButtonType
    {
        Attack,
        Block,
        Heal
    }

    void Start()
    {
        UpdateUI();
    }

    protected override void DeliverItem()
    {
        if (ActionButtonsController.Instance == null)
        {
            Debug.LogError("[ActionButtonUpgradeShopItem] ActionButtonsController.Instance no encontrado");
            return;
        }

        Debug.Log($"[ActionButtonUpgradeShopItem] ===== APLICANDO MEJORA A {buttonType} =====");
        Debug.Log($"[ActionButtonUpgradeShopItem] Cantidad de mejora: +{baseValueUpgradeAmount}");

        // Aplicar mejora según el tipo de botón
        switch (buttonType)
        {
            case ActionButtonType.Attack:
                float oldAttack = ActionButtonsController.Instance.GetBaseAttackValue();
                ActionButtonsController.Instance.UpgradeBaseAttackValue(baseValueUpgradeAmount);
                float newAttack = ActionButtonsController.Instance.GetBaseAttackValue();
                Debug.Log($"[ActionButtonUpgradeShopItem] Ataque: {oldAttack} → {newAttack}");
                break;
            case ActionButtonType.Block:
                float oldBlock = ActionButtonsController.Instance.GetBaseBlockValue();
                ActionButtonsController.Instance.UpgradeBaseBlockValue(baseValueUpgradeAmount);
                float newBlock = ActionButtonsController.Instance.GetBaseBlockValue();
                Debug.Log($"[ActionButtonUpgradeShopItem] Bloqueo: {oldBlock} → {newBlock}");
                break;
            case ActionButtonType.Heal:
                float oldHeal = ActionButtonsController.Instance.GetBaseHealValue();
                ActionButtonsController.Instance.UpgradeBaseHealValue(baseValueUpgradeAmount);
                float newHeal = ActionButtonsController.Instance.GetBaseHealValue();
                Debug.Log($"[ActionButtonUpgradeShopItem] Curación: {oldHeal} → {newHeal}");
                break;
        }

        // Guardar mejoras
        SaveUpgrades();

        // Actualizar UI
        UpdateUI();

        Debug.Log($"[ActionButtonUpgradeShopItem] ===== MEJORA APLICADA EXITOSAMENTE =====");
    }

    private void UpdateUI()
    {
        if (ActionButtonsController.Instance == null) return;

        // Actualizar nombre del botón
        if (buttonNameText != null)
        {
            buttonNameText.text = GetButtonName();
        }

        // Actualizar valor actual
        if (currentValueText != null)
        {
            float currentValue = GetCurrentValue();
            currentValueText.text = $"Valor actual: {currentValue:F0}";
        }

        // Actualizar cantidad de mejora
        if (upgradeAmountText != null)
        {
            upgradeAmountText.text = $"+{baseValueUpgradeAmount:F0}";
        }

        // Actualizar precio
        if (priceText != null && GetComponent<ShopItem>() != null)
        {
            // El precio se maneja en la clase base ShopItem
        }
    }

    private string GetButtonName()
    {
        switch (buttonType)
        {
            case ActionButtonType.Attack:
                return "Ataque";
            case ActionButtonType.Block:
                return "Bloqueo";
            case ActionButtonType.Heal:
                return "Curación";
            default:
                return "Desconocido";
        }
    }

    private float GetCurrentValue()
    {
        if (ActionButtonsController.Instance == null) return 0f;

        switch (buttonType)
        {
            case ActionButtonType.Attack:
                return ActionButtonsController.Instance.GetBaseAttackValue();
            case ActionButtonType.Block:
                return ActionButtonsController.Instance.GetBaseBlockValue();
            case ActionButtonType.Heal:
                return ActionButtonsController.Instance.GetBaseHealValue();
            default:
                return 0f;
        }
    }

    private void SaveUpgrades()
    {
        string key = $"ActionButton_{buttonType}_Upgrade";
        float currentUpgrade = PlayerPrefs.GetFloat(key, 0f);
        currentUpgrade += baseValueUpgradeAmount;
        PlayerPrefs.SetFloat(key, currentUpgrade);
        PlayerPrefs.Save();

        Debug.Log($"[ActionButtonUpgradeShopItem] Mejora guardada: {key} = {currentUpgrade}");
    }
}

