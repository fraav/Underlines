using UnityEngine;

public class HealthChangeTextDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Transform textSpawnPoint;
    
    [Header("Display Settings")]
    [SerializeField] private bool showDamage = true;
    [SerializeField] private bool showHeal = true;
    [SerializeField] private Vector3 textOffset = Vector3.up * 2f;
    
    private HealthChangeTextManager textManager;
    
    void Start()
    {
        // Obtener referencia al HealthChangeTextManager
        textManager = HealthChangeTextManager.Instance;
        
        if (textManager == null)
        {
            Debug.LogError("HealthChangeTextManager no encontrado en la escena!", this);
            return;
        }
        
        // Configurar el HealthSystem si no está asignado
        if (healthSystem == null)
        {
            healthSystem = GetComponent<HealthSystem>();
        }
        
        // Configurar el punto de spawn del texto si no está asignado
        if (textSpawnPoint == null)
        {
            textSpawnPoint = transform;
        }
        
        // Suscribirse a los eventos del HealthSystem
        if (healthSystem != null)
        {
            healthSystem.OnTakeDamage.AddListener(OnTakeDamage);
            healthSystem.OnHeal.AddListener(OnHeal);
        }
        else
        {
            Debug.LogWarning("HealthSystem no encontrado en HealthChangeTextDisplay", this);
        }
    }
    
    private void OnTakeDamage(int damageAmount)
    {
        if (!showDamage || textManager == null) return;
        
        Vector3 spawnPosition = GetTextSpawnPosition();
        textManager.ShowHealthChange(spawnPosition, damageAmount, false);
    }
    
    private void OnHeal(int healAmount)
    {
        if (!showHeal || textManager == null) return;
        
        Vector3 spawnPosition = GetTextSpawnPosition();
        textManager.ShowHealthChange(spawnPosition, healAmount, true);
    }
    
    private Vector3 GetTextSpawnPosition()
    {
        if (textSpawnPoint != null)
        {
            return textSpawnPoint.position + textOffset;
        }
        
        return transform.position + textOffset;
    }
    
    public void SetHealthSystem(HealthSystem newHealthSystem)
    {
        // Desuscribirse del HealthSystem anterior
        if (healthSystem != null)
        {
            healthSystem.OnTakeDamage.RemoveListener(OnTakeDamage);
            healthSystem.OnHeal.RemoveListener(OnHeal);
        }
        
        // Asignar nuevo HealthSystem
        healthSystem = newHealthSystem;
        
        // Suscribirse al nuevo HealthSystem
        if (healthSystem != null)
        {
            healthSystem.OnTakeDamage.AddListener(OnTakeDamage);
            healthSystem.OnHeal.AddListener(OnHeal);
        }
    }
    
    public void SetTextSpawnPoint(Transform newSpawnPoint)
    {
        textSpawnPoint = newSpawnPoint;
    }
    
    public void SetTextOffset(Vector3 newOffset)
    {
        textOffset = newOffset;
    }
    
    public void SetDisplaySettings(bool showDmg, bool showHl)
    {
        showDamage = showDmg;
        showHeal = showHl;
    }
    
    void OnDestroy()
    {
        // Desuscribirse de los eventos al destruir
        if (healthSystem != null)
        {
            healthSystem.OnTakeDamage.RemoveListener(OnTakeDamage);
            healthSystem.OnHeal.RemoveListener(OnHeal);
        }
    }
}

