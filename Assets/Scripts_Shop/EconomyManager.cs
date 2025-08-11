using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    // Evento estático para recompensas
    public static event System.Action<int> OnRewardGiven;

    private int currentMoney;
    public int CurrentMoney => currentMoney;

    private const string MoneyKey = "PlayerMoney";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMoney();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetMoney()
    {
        currentMoney = 1500;
        SaveMoney();

        if (MoneyUI.Instance != null)
            MoneyUI.Instance.UpdateMoneyText(currentMoney);
    }

    // Método público para añadir dinero
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        SaveMoney();

        if (MoneyUI.Instance != null)
            MoneyUI.Instance.UpdateMoneyText(currentMoney);
    }

    // Método estático para dar recompensas
    public static void GiveReward(int amount)
    {
        if (Instance != null)
        {
            Instance.AddMoney(amount);
        }
        OnRewardGiven?.Invoke(amount);
    }

    public bool TryPurchaseItem(int cost)
    {
        if (currentMoney >= cost)
        {
            currentMoney -= cost;
            SaveMoney();

            if (MoneyUI.Instance != null)
                MoneyUI.Instance.UpdateMoneyText(currentMoney);

            return true;
        }
        return false;
    }

    private void LoadMoney()
    {
        currentMoney = PlayerPrefs.GetInt(MoneyKey, 0);
    }

    private void SaveMoney()
    {
        PlayerPrefs.SetInt(MoneyKey, currentMoney);
        PlayerPrefs.Save();
    }
}