using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [System.Serializable]
    public class HealthEvent : UnityEvent<int> { }
    [System.Serializable]
    public class DeathEvent : UnityEvent { }

    public int MaxHealth { get; private set; } = 100;
    public int CurrentHealth { get; private set; }

    public HealthEvent OnHealthChanged = new HealthEvent();
    public DeathEvent OnDeath = new DeathEvent();

    void Start()
    {
        CurrentHealth = MaxHealth;
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
        if (CurrentHealth == 0) OnDeath.Invoke();
    }

    public void Heal(int amount)
    {
        if (CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
        OnHealthChanged.Invoke(CurrentHealth);
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