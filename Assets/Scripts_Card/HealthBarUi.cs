using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private bool showText = true;

    void Start()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.AddListener(UpdateHealthBar);
            UpdateHealthBar(healthSystem.CurrentHealth);
        }
        else
        {
            Debug.LogWarning("HealthSystem reference is missing in HealthBarUI", this);
        }
    }

    public void SetHealthSystem(HealthSystem newHealthSystem)
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }

        healthSystem = newHealthSystem;

        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.AddListener(UpdateHealthBar);
            UpdateHealthBar(healthSystem.CurrentHealth);
        }
        else
        {
            Debug.LogWarning("Trying to set a null HealthSystem in HealthBarUI", this);
        }
    }

    private void UpdateHealthBar(int currentHealth)
    {
        if (healthSystem == null)
        {
            Debug.LogWarning("HealthSystem is null in HealthBarUI", this);
            return;
        }

        if (healthSlider == null)
        {
            Debug.LogWarning("HealthSlider is null in HealthBarUI", this);
            return;
        }

        healthSlider.maxValue = healthSystem.MaxHealth;
        healthSlider.value = currentHealth;

        if (showText && healthText != null)
        {
            healthText.text = $"{currentHealth}/{healthSystem.MaxHealth}";
        }
    }
}