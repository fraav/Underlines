using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private bool showText = true;

    [Header("Enemy bar — reveal on hover / health change")]
    [Tooltip("Si está activo, la barra permanece invisible hasta pasar el ratón por el enemigo o hasta que reciba daño o curación.")]
    [SerializeField] private bool enemyStealthBar;
    [Tooltip("Raíz visual de la barra (Slider + textos). Debe tener CanvasGroup para controlar la visibilidad.")]
    [SerializeField] private CanvasGroup stealthCanvasGroup;
    [Tooltip("Segundos en los que la barra permanece visible tras daño o curación (si el ratón no está sobre el enemigo).")]
    [SerializeField] private float revealAfterHealthChangeSeconds = 2f;

    private bool hoverReveal;
    private float revealUntilTime;

    void Start()
    {
        if (enemyStealthBar && stealthCanvasGroup == null)
        {
            stealthCanvasGroup = GetComponent<CanvasGroup>();
            if (stealthCanvasGroup == null)
            {
                Debug.LogWarning("HealthBarUI: enemyStealthBar activo pero falta CanvasGroup. Asigna uno en el Inspector.", this);
                enemyStealthBar = false;
            }
        }

        if (healthSystem != null)
        {
            RegisterHealthListeners();
            UpdateHealthBar(healthSystem.CurrentHealth);
        }
        else
        {
            Debug.LogWarning("HealthSystem reference is missing in HealthBarUI", this);
        }

        ApplyStealthVisibility();
    }

    void OnDestroy()
    {
        UnregisterHealthListeners();
    }

    void LateUpdate()
    {
        ApplyStealthVisibility();
    }

    public void SetHealthSystem(HealthSystem newHealthSystem)
    {
        UnregisterHealthListeners();

        healthSystem = newHealthSystem;

        if (healthSystem != null)
        {
            RegisterHealthListeners();
            UpdateHealthBar(healthSystem.CurrentHealth);
        }
        else
        {
            Debug.LogWarning("Trying to set a null HealthSystem in HealthBarUI", this);
        }

        ApplyStealthVisibility();
    }

    /// <summary>
    /// Llamado desde un script con OnMouseEnter/Exit en el collider del enemigo (misma referencia a esta barra).
    /// </summary>
    public void SetEnemyBarHover(bool hovering)
    {
        hoverReveal = hovering;
        ApplyStealthVisibility();
    }

    private void RegisterHealthListeners()
    {
        if (healthSystem == null) return;

        healthSystem.OnHealthChanged.AddListener(UpdateHealthBar);
        if (enemyStealthBar)
        {
            healthSystem.OnTakeDamage.AddListener(OnEnemyCombatHealthChanged);
            healthSystem.OnHeal.AddListener(OnEnemyCombatHealthChanged);
        }
    }

    private void UnregisterHealthListeners()
    {
        if (healthSystem == null) return;

        healthSystem.OnHealthChanged.RemoveListener(UpdateHealthBar);
        healthSystem.OnTakeDamage.RemoveListener(OnEnemyCombatHealthChanged);
        healthSystem.OnHeal.RemoveListener(OnEnemyCombatHealthChanged);
    }

    private void OnEnemyCombatHealthChanged(int _)
    {
        if (!enemyStealthBar || stealthCanvasGroup == null) return;

        revealUntilTime = Time.time + Mathf.Max(0.05f, revealAfterHealthChangeSeconds);
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

    private void ApplyStealthVisibility()
    {
        if (!enemyStealthBar || stealthCanvasGroup == null)
        {
            return;
        }

        bool show = hoverReveal || Time.time < revealUntilTime;
        stealthCanvasGroup.alpha = show ? 1f : 0f;
        stealthCanvasGroup.blocksRaycasts = show;
        stealthCanvasGroup.interactable = show;
    }
}
