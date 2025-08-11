using UnityEngine;
using System.Collections;

[System.Serializable]
public class CardAnimation
{
    [Header("Animation Settings")]
    [Tooltip("Tipo de carta para esta animación")]
    public CardData.CardType cardType;
    [Tooltip("Nombre del trigger de la animación")]
    public string animationTrigger;
    [Tooltip("Tiempo en segundos desde el inicio de la animación hasta el punto de acción")]
    public float actionPointTime = 0.5f;
    [Tooltip("Duración total de la animación en segundos")]
    public float animationDuration = 1.0f;
}

public class PlayerController : MonoBehaviour
{
    [Header("Card Animations")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject highlightEffect;
    [SerializeField] private CardAnimation[] cardAnimations;

    [Header("Block System")]
    [SerializeField] private string blockAnimationTrigger = "Block";
    
    [Header("Damage Animations")]
    [SerializeField] private string damageAnimationTrigger = "TakeDamage";

    public HealthSystem healthSystem;
    
    // Sistema de bloqueo
    private bool hasBlockActive = false;
    private bool hasBlockPending = false; // Nuevo: bloqueo pendiente para el siguiente turno
    private float blockReductionMultiplier = 1.0f;

    void Start()
    {
        if (healthSystem == null)
        {
            healthSystem = GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                healthSystem = gameObject.AddComponent<HealthSystem>();
                healthSystem.SetMaxHealth(100);
            }
        }
        
        // Suscribirse al evento de daño para detectar cuando se activa el bloqueo
        if (healthSystem != null)
        {
            healthSystem.OnTakeDamage.AddListener(OnTakeDamage);
        }
    }

    public void PlayCardAnimation(CardData card, System.Action onAction, System.Action onComplete)
    {
        // Buscar la animación correspondiente al tipo de carta
        CardAnimation animation = GetAnimationForCard(card.cardType);
        if (animation != null)
        {
            StartCoroutine(PerformCardAnimation(animation, onAction, onComplete));
        }
        else
        {
            // Si no hay animación, ejecutar directamente
            onAction?.Invoke();
            onComplete?.Invoke();
        }
    }

    private IEnumerator PerformCardAnimation(CardAnimation animation, System.Action onAction, System.Action onComplete)
    {
        if (animator != null && !string.IsNullOrEmpty(animation.animationTrigger))
        {
            animator.SetTrigger(animation.animationTrigger);
        }

        // Esperar hasta el punto de acción de la animación
        yield return new WaitForSeconds(animation.actionPointTime);
        
        onAction?.Invoke();

        // Esperar el resto de la animación
        float remainingTime = animation.animationDuration - animation.actionPointTime;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        onComplete?.Invoke();
    }

    private CardAnimation GetAnimationForCard(CardData.CardType cardType)
    {
        if (cardAnimations == null) return null;
        
        foreach (CardAnimation animation in cardAnimations)
        {
            if (animation != null && animation.cardType == cardType)
            {
                return animation;
            }
        }
        
        return null;
    }

    // Método para activar el bloqueo (ahora solo lo marca como pendiente)
    public void ActivateBlock(float reductionMultiplier)
    {
        hasBlockPending = true;
        blockReductionMultiplier = reductionMultiplier;
        Debug.Log($"Bloqueo marcado como pendiente para el siguiente turno del enemigo. Multiplicador: {reductionMultiplier}");
    }

    // Método para activar el bloqueo pendiente (se llama cuando el enemigo ataca)
    public void ActivatePendingBlock()
    {
        if (hasBlockPending)
        {
            hasBlockActive = true;
            hasBlockPending = false;
            Debug.Log($"Bloqueo activado para este ataque enemigo. Multiplicador: {blockReductionMultiplier}");
        }
    }

    // Método para desactivar el bloqueo
    public void DeactivateBlock()
    {
        hasBlockActive = false;
        hasBlockPending = false;
        blockReductionMultiplier = 1.0f;
        Debug.Log("Bloqueo desactivado");
    }

    // Método para verificar si tiene bloqueo activo
    public bool HasBlockActive()
    {
        return hasBlockActive;
    }

    // Método para verificar si tiene bloqueo pendiente
    public bool HasBlockPending()
    {
        return hasBlockPending;
    }

    // Método para obtener el multiplicador de reducción del bloqueo
    public float GetBlockReductionMultiplier()
    {
        return blockReductionMultiplier;
    }

    // Método llamado cuando el jugador recibe daño
    private void OnTakeDamage(int damage)
    {
        // Reproducir sonido de daño a través del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayPlayerDamageSound();
        }
        
        // Desactivar bloqueo si está activo
        if (hasBlockActive)
        {
            DeactivateBlock();
            Debug.Log("Bloqueo desactivado después de recibir daño");
        }
    }

    // Método para reproducir la animación de bloqueo
    public void PlayBlockAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(blockAnimationTrigger))
        {
            animator.SetTrigger(blockAnimationTrigger);
        }

        Debug.Log("¡Animación de bloqueo ejecutada!");
    }
    
    /// <summary>
    /// Reproduce la animación de recibir daño del jugador
    /// Se puede llamar cuando el jugador recibe daño (especialmente mientras bloquea)
    /// </summary>
    public void PlayDamageAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(damageAnimationTrigger))
        {
            animator.SetTrigger(damageAnimationTrigger);
        }

        Debug.Log("¡Animación de recibir daño ejecutada!");
    }

    public void SetHighlight(bool active)
    {
        if (highlightEffect != null) highlightEffect.SetActive(active);
    }

    private void OnMouseDown()
    {
        GameManager.Instance?.SelectTarget(gameObject);
    }
}