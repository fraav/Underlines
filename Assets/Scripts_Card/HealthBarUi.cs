using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    }

    private void UpdateHealthBar(int currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = healthSystem.MaxHealth;
            healthSlider.value = currentHealth;
        }

        if (showText && healthText != null)
        {
            healthText.text = $"{currentHealth}/{healthSystem.MaxHealth}";
        }
    }
}