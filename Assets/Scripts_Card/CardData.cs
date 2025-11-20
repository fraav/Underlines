using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Game/Card")]
public class CardData : ScriptableObject
{
    public enum CardType { Attack, Block, Heal, Booster }
    public CardType cardType;
    public float baseValue;
    public string cardName;
    [TextArea] public string description;

    // Sprite para uso en el juego
    public Sprite icon;

    // Sprite específico para la tienda
    public Sprite shopIcon;

    [Header("Card Animation & Audio Settings")]
    [Tooltip("Trigger de animación específico para esta carta (opcional). Si se deja vacío, se usará la animación por tipo de carta en PlayerController.")]
    public string customAnimationTrigger = "";

    [Tooltip("Tiempo desde el inicio de la animación hasta el punto de acción (si se deja en 0, se usa el valor por defecto de PlayerController).")]
    public float customActionPointTime = 0f;

    [Tooltip("Duración total de la animación (si se deja en 0, se usa el valor por defecto de PlayerController).")]
    public float customAnimationDuration = 0f;

    [Tooltip("Sonido específico que se reproducirá al jugar esta carta.")]
    public AudioClip cardSound;

    // Campos para mejoras individuales
    public float individualBaseValueUpgrade = 0f;
    public float individualDamageMultiplier = 1.0f;

    // Para cartas potenciadoras (Booster)
    public enum BoosterEffectType { 
        DoubleAction,      // Duplica la acción siguiente
        IncreaseDamage,    // Aumenta el daño
        IncreaseBlock,     // Aumenta el bloqueo
        IncreaseHeal       // Aumenta la curación
    }
    
    [Header("Booster Settings")]
    public BoosterEffectType boosterEffectType = BoosterEffectType.DoubleAction;
    public float boosterValue = 1.0f; // Valor del potenciador

    // ========== PARÁMETROS PARA FUTURAS EXPANSIONES ==========
    // Estos parámetros no afectan el funcionamiento actual del sistema
    // Solo están disponibles para futuras expansiones de contenido

    [Header("Expansion Parameters - No afectan el sistema actual")]
    [Tooltip("Activar para habilitar efectos personalizados adicionales")]
    public bool enableCustomEffects = false;

    [Header("Custom Effect Settings")]
    [Tooltip("Duración del efecto en turnos (0 = permanente hasta el final del turno)")]
    public int effectDuration = 0;

    [Tooltip("Efecto adicional de daño (se suma al baseValue)")]
    public float additionalDamage = 0f;

    [Tooltip("Efecto adicional de curación (se suma al baseValue)")]
    public float additionalHeal = 0f;

    [Tooltip("Efecto adicional de bloqueo (se suma al baseValue)")]
    public float additionalBlock = 0f;

    [Tooltip("Multiplicador de energía/coste (para futuros sistemas de energía)")]
    public float energyCost = 0f;

    [Tooltip("Tipo de objetivo adicional (para futuros sistemas de targeting)")]
    public TargetType customTargetType = TargetType.None;

    [Tooltip("Efectos de estado adicionales (para futuros sistemas de estados)")]
    public StatusEffectType statusEffect = StatusEffectType.None;

    [Tooltip("Duración del efecto de estado en turnos")]
    public int statusEffectDuration = 0;

    [Tooltip("Valores personalizados adicionales (para efectos únicos)")]
    public float[] customValues = new float[5];

    [Tooltip("Texto de efecto personalizado (para mostrar en UI)")]
    [TextArea(2, 4)]
    public string customEffectDescription = "";

    [Tooltip("ID único para efectos personalizados (para sistemas de scripting)")]
    public string customEffectID = "";

    // Enums para futuras expansiones
    public enum TargetType
    {
        None,
        Self,
        Enemy,
        AllEnemies,
        AllAllies,
        RandomEnemy,
        RandomAlly
    }

    public enum StatusEffectType
    {
        None,
        Poison,
        Burn,
        Freeze,
        Stun,
        Shield,
        Regeneration,
        Strength,
        Weakness
    }

    // Métodos helper para futuras expansiones (no se usan actualmente)
    /// <summary>
    /// Obtiene el valor total de daño incluyendo efectos personalizados
    /// </summary>
    public float GetTotalDamage()
    {
        if (!enableCustomEffects) return baseValue;
        return baseValue + additionalDamage;
    }

    /// <summary>
    /// Obtiene el valor total de curación incluyendo efectos personalizados
    /// </summary>
    public float GetTotalHeal()
    {
        if (!enableCustomEffects) return baseValue;
        return baseValue + additionalHeal;
    }

    /// <summary>
    /// Obtiene el valor total de bloqueo incluyendo efectos personalizados
    /// </summary>
    public float GetTotalBlock()
    {
        if (!enableCustomEffects) return baseValue;
        return baseValue + additionalBlock;
    }

    /// <summary>
    /// Verifica si la carta tiene efectos personalizados activos
    /// </summary>
    public bool HasCustomEffects()
    {
        return enableCustomEffects && (
            additionalDamage != 0f ||
            additionalHeal != 0f ||
            additionalBlock != 0f ||
            statusEffect != StatusEffectType.None ||
            customTargetType != TargetType.None ||
            !string.IsNullOrEmpty(customEffectID)
        );
    }
}
