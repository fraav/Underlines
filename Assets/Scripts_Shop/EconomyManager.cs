using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

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
        currentMoney = 1500; // O el valor inicial que prefieras
        SaveMoney();
        
        if (MoneyUI.Instance != null)
            MoneyUI.Instance.UpdateMoneyText(currentMoney);
    }

    // Funci�n p�blica para a�adir dinero desde cualquier script
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        SaveMoney();

        // Actualizar UI
        if (MoneyUI.Instance != null)
            MoneyUI.Instance.UpdateMoneyText(currentMoney);
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


    // A�adir 100 unidades de dinero //
   // EconomyManager.Instance.AddMoney(100); //

}