using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    // Evento estático para recompensas
    public static event System.Action<int> OnRewardGiven;
    
    [Header("Configuración de Efectos Visuales")]
    [SerializeField] private GameObject[] cardPurchaseEffects; // Objetos a activar (uno por carta)
    [SerializeField] private float effectDuration = 2f; // Tiempo que se muestra el efecto

    private int currentMoney;
    public int CurrentMoney => currentMoney;
    private const string MoneyKey = "PlayerMoney";
    private Coroutine[] activeEffects; // Para controlar efectos activos

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMoney();
            InitializeEffects();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeEffects()
    {
        activeEffects = new Coroutine[cardPurchaseEffects.Length];
        
        // Asegurarse que todos los efectos están desactivados al inicio
        foreach (var effect in cardPurchaseEffects)
        {
            if (effect != null) effect.SetActive(false);
        }
    }

    public void ResetMoney()
    {
        currentMoney = 1500;
        SaveMoney();
        UpdateMoneyUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        SaveMoney();
        UpdateMoneyUI();
    }

    public static void GiveReward(int amount)
    {
        if (Instance != null)
        {
            Instance.AddMoney(amount);
        }
        OnRewardGiven?.Invoke(amount);
    }

    // Método modificado para compra de cartas
    public bool TryPurchaseItem(int cost, int cardIndex = -1)
    {
        if (currentMoney >= cost)
        {
            currentMoney -= cost;
            SaveMoney();
            UpdateMoneyUI();

            // Mostrar efecto visual si es una compra de carta válida
            if (cardIndex >= 0 && cardIndex < cardPurchaseEffects.Length)
            {
                ShowCardPurchaseEffect(cardIndex);
            }

            return true;
        }
        return false;
    }

    private void ShowCardPurchaseEffect(int cardIndex)
    {
        // Cancelar efecto previo si existe
        if (activeEffects[cardIndex] != null)
        {
            StopCoroutine(activeEffects[cardIndex]);
        }

        // Iniciar nuevo efecto
        activeEffects[cardIndex] = StartCoroutine(CardEffectRoutine(cardIndex));
    }

    private IEnumerator CardEffectRoutine(int cardIndex)
    {
        GameObject effect = cardPurchaseEffects[cardIndex];
        
        if (effect != null)
        {
            effect.SetActive(true);
            
            // Opcional: Animación de entrada
            if (effect.TryGetComponent<Animator>(out var animator))
            {
                animator.Play("Enter");
            }

            yield return new WaitForSeconds(effectDuration);

            // Opcional: Animación de salida
            if (animator != null && animator.HasState(0, Animator.StringToHash("Exit")))
            {
                animator.Play("Exit");
                yield return new WaitForSeconds(0.5f); // Esperar animación de salida
            }

            effect.SetActive(false);
        }
        
        activeEffects[cardIndex] = null;
    }

    private void UpdateMoneyUI()
    {
        if (MoneyUI.Instance != null)
            MoneyUI.Instance.UpdateMoneyText(currentMoney);
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