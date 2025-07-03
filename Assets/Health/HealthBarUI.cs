using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text healthText;

    [Header("Configuración")]
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private bool showText = true;

    private HealthSystem healthSystem;

    public void SetHealthSystem(HealthSystem system)
    {
        healthSystem = system;
        healthSystem.OnHealthChanged.AddListener(UpdateHealthBar);
        UpdateHealthBar(healthSystem.CurrentHealth);
    }

    private void UpdateHealthBar(int currentHealth)
    {
        if (healthSystem == null) return;

        if (healthSlider != null)
        {
            healthSlider.maxValue = healthSystem.MaxHealth;
            healthSlider.value = currentHealth;

            if (fillImage != null)
            {
                fillImage.color = healthGradient.Evaluate(healthSystem.HealthPercentage);
            }
        }

        if (healthText != null && showText)
        {
            healthText.text = $"{currentHealth}/{healthSystem.MaxHealth}";
        }
    }
}