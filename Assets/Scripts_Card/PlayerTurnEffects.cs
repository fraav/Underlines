using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema que gestiona los efectos acumulados del jugador durante su turno
/// </summary>
public class PlayerTurnEffects : MonoBehaviour
{
    public static PlayerTurnEffects Instance;

    [System.Serializable]
    public class ActiveEffect
    {
        public CardData card;
        public CardData.BoosterEffectType effectType;
        public float value;

        public string sourceName;

        public ActiveEffect(CardData card)
        {
            this.card = card;
            this.effectType = card.boosterEffectType;
            this.value = card.boosterValue;
            this.sourceName = card != null ? card.cardName : "Unknown";
        }

        public ActiveEffect(CardData.BoosterEffectType type, float effectValue, CardData cardRef, string source)
        {
            this.card = cardRef;
            this.effectType = type;
            this.value = effectValue;
            this.sourceName = source;
        }

        public string GetEffectDescription()
        {
            switch (effectType)
            {
                case CardData.BoosterEffectType.DoubleAction:
                    return "Duplica la próxima acción";
                case CardData.BoosterEffectType.IncreaseDamage:
                    return $"Aumenta el daño en {value * 100}%";
                case CardData.BoosterEffectType.IncreaseBlock:
                    return $"Aumenta el bloqueo en {value * 100}%";
                case CardData.BoosterEffectType.IncreaseHeal:
                    return $"Aumenta la curación en {value * 100}%";
                default:
                    return "Efecto desconocido";
            }
        }
    }

    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Añade un efecto activo al turno del jugador
    /// </summary>
    public void AddEffect(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("[PlayerTurnEffects] Intento de añadir efecto con carta nula");
            return;
        }

        if (card.cardType == CardData.CardType.Booster)
        {
            ActiveEffect effect = new ActiveEffect(card);
            activeEffects.Add(effect);
            
            Debug.Log($"[PlayerTurnEffects] ✓ Efecto añadido: {card.cardName} - {effect.GetEffectDescription()}");
            Debug.Log($"[PlayerTurnEffects] Total de efectos activos: {activeEffects.Count}");
            
            LogEffectsSummary();
        }
        else
        {
            Debug.LogWarning($"[PlayerTurnEffects] Intento de añadir efecto de carta no potenciadora: {card.cardType}");
        }
    }

    /// <summary>
    /// Duplica la siguiente acción (desde carta/objeto u otra fuente).
    /// </summary>
    public void AddDoubleActionEffect(string sourceName = "Object")
    {
        ActiveEffect effect = new ActiveEffect(CardData.BoosterEffectType.DoubleAction, 1f, null, sourceName);
        activeEffects.Add(effect);
        Debug.Log($"[PlayerTurnEffects] ✓ Duplicar acción desde: {sourceName}");
        LogEffectsSummary();
    }

    /// <summary>
    /// Muestra un resumen de todos los efectos acumulados en la consola
    /// </summary>
    private void LogEffectsSummary()
    {
        int doubleActionCount = GetDoubleActionCount();
        float damageMult = GetDamageMultiplier();
        float blockMult = GetBlockMultiplier();
        float healMult = GetHealMultiplier();
        
        Debug.Log($"[PlayerTurnEffects] Resumen de efectos acumulados:");
        Debug.Log($"  - Duplicaciones de acción: {doubleActionCount}");
        Debug.Log($"  - Multiplicador de daño: {damageMult:F2}x");
        Debug.Log($"  - Multiplicador de bloqueo: {blockMult:F2}x");
        Debug.Log($"  - Multiplicador de curación: {healMult:F2}x");
    }

    /// <summary>
    /// Elimina todos los efectos activos (deshacer)
    /// </summary>
    public void ClearEffects()
    {
        Debug.Log($"[PlayerTurnEffects] Limpiando {activeEffects.Count} efectos activos");
        activeEffects.Clear();
    }

    /// <summary>
    /// Obtiene todos los efectos activos
    /// </summary>
    public List<ActiveEffect> GetActiveEffects()
    {
        return new List<ActiveEffect>(activeEffects);
    }

    /// <summary>
    /// Obtiene el conteo de efectos de duplicación
    /// </summary>
    public int GetDoubleActionCount()
    {
        int count = 0;
        foreach (var effect in activeEffects)
        {
            if (effect.effectType == CardData.BoosterEffectType.DoubleAction)
            {
                count++;
            }
        }
        
        if (count > 0)
        {
            Debug.Log($"[PlayerTurnEffects] Efectos de duplicación activos: {count} (la acción se ejecutará {count + 1} vez/veces)");
        }
        
        return count;
    }

    /// <summary>
    /// Obtiene el multiplicador total de daño
    /// </summary>
    public float GetDamageMultiplier()
    {
        float multiplier = 1.0f;
        int effectCount = 0;
        
        foreach (var effect in activeEffects)
        {
            if (effect.effectType == CardData.BoosterEffectType.IncreaseDamage)
            {
                multiplier += effect.value;
                effectCount++;
            }
        }
        
        if (effectCount > 0)
        {
            Debug.Log($"[PlayerTurnEffects] Multiplicador de daño calculado: {multiplier:F2}x (base: 1.0 + {effectCount} efecto(s))");
        }
        
        return multiplier;
    }

    /// <summary>
    /// Obtiene el multiplicador total de bloqueo
    /// </summary>
    public float GetBlockMultiplier()
    {
        float multiplier = 1.0f;
        int effectCount = 0;
        
        foreach (var effect in activeEffects)
        {
            if (effect.effectType == CardData.BoosterEffectType.IncreaseBlock)
            {
                multiplier += effect.value;
                effectCount++;
            }
        }
        
        if (effectCount > 0)
        {
            Debug.Log($"[PlayerTurnEffects] Multiplicador de bloqueo calculado: {multiplier:F2}x (base: 1.0 + {effectCount} efecto(s))");
        }
        
        return multiplier;
    }

    /// <summary>
    /// Obtiene el multiplicador total de curación
    /// </summary>
    public float GetHealMultiplier()
    {
        float multiplier = 1.0f;
        int effectCount = 0;
        
        foreach (var effect in activeEffects)
        {
            if (effect.effectType == CardData.BoosterEffectType.IncreaseHeal)
            {
                multiplier += effect.value;
                effectCount++;
            }
        }
        
        if (effectCount > 0)
        {
            Debug.Log($"[PlayerTurnEffects] Multiplicador de curación calculado: {multiplier:F2}x (base: 1.0 + {effectCount} efecto(s))");
        }
        
        return multiplier;
    }

    /// <summary>
    /// Obtiene una descripción de todos los efectos acumulados
    /// </summary>
    public string GetEffectsDescription()
    {
        if (activeEffects.Count == 0)
        {
            return "Sin efectos activos";
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Efectos activos:");

        Dictionary<string, int> effectCounts = new Dictionary<string, int>();
        foreach (var effect in activeEffects)
        {
            string desc = effect.GetEffectDescription();
            if (effectCounts.ContainsKey(desc))
            {
                effectCounts[desc]++;
            }
            else
            {
                effectCounts[desc] = 1;
            }
        }

        foreach (var kvp in effectCounts)
        {
            if (kvp.Value > 1)
            {
                sb.AppendLine($"- {kvp.Key} x{kvp.Value}");
            }
            else
            {
                sb.AppendLine($"- {kvp.Key}");
            }
        }

        return sb.ToString();
    }
}




