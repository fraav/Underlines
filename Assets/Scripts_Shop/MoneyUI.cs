using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    public static MoneyUI Instance { get; private set; }

    [SerializeField] private TMP_Text moneyText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // No destruir si quieres que persista entre escenas
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateMoneyText(EconomyManager.Instance.CurrentMoney);
    }

    public void UpdateMoneyText(int amount)
    {
        moneyText.text = $"$ {amount}";
    }
}