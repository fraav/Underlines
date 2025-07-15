// HealthSystem.cs
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [System.Serializable]
    public class HealthEvent : UnityEvent<int> { }
    [System.Serializable]
    public class DeathEvent : UnityEvent { }
    [System.Serializable]
    public class TakeDamageEvent : UnityEvent<int> { }
    [System.Serializable]
    public class HealEvent : UnityEvent<int> { }

    public int MaxHealth { get; private set; } = 200;
    public int CurrentHealth { get; private set; }

    public HealthEvent OnHealthChanged = new HealthEvent();
    public DeathEvent OnDeath = new DeathEvent();
    public TakeDamageEvent OnTakeDamage = new TakeDamageEvent(); // Nuevo evento para daño
    public HealEvent OnHeal = new HealEvent(); // Nuevo evento para curación

    void Start()
    {
        CurrentHealth = MaxHealth;

        OnHealthChanged.Invoke(CurrentHealth);
    }

    public void SetMaxHealth(int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        OnHealthChanged.Invoke(CurrentHealth);
    }

    public void TakeDamage(int amount)
    {
        if (CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, MaxHealth);
        OnHealthChanged.Invoke(CurrentHealth);
        OnTakeDamage.Invoke(amount); // Invocar evento de daño
        if (CurrentHealth == 0) OnDeath.Invoke();
    }

    public void Heal(int amount)
    {
        if (CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
        OnHealthChanged.Invoke(CurrentHealth);
        OnHeal.Invoke(amount); // Invocar evento de curación
    }

    public void SaveHealth(string saveKey)
    {
        PlayerPrefs.SetInt(saveKey, CurrentHealth);
        PlayerPrefs.Save();
    }

    public void LoadHealth(string saveKey, int defaultHealth)
    {
        CurrentHealth = PlayerPrefs.GetInt(saveKey, defaultHealth);
        OnHealthChanged.Invoke(CurrentHealth);
    }
}