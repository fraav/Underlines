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

    [Header("Objects Hidden During Card Animations")]
    [SerializeField] private GameObject[] objectsToDisableDuringCardAnimation;

    [Header("Block System")]
    [SerializeField] private string blockAnimationTrigger = "Block";
    [SerializeField] private string blockIdleAnimationTrigger = "BlockIdle";
    [SerializeField] private float blockAutoDeactivateTime = 3.0f; // Tiempo antes de desactivar bloqueo automáticamente
    
    [Header("Damage Animations")]
    [SerializeField] private string damageAnimationTrigger = "TakeDamage";
    [SerializeField] private string damageWhileBlockingAnimationTrigger = "TakeDamageWhileBlocking";

    public HealthSystem healthSystem;
    
    // Sistema de bloqueo
    private bool hasBlockActive = false;
    private bool hasBlockPending = false;
    private float blockReductionMultiplier = 1.0f;
    private Coroutine blockAutoDeactivateCoroutine;

    private CardData.BlockBonusType pendingBlockBonus = CardData.BlockBonusType.None;
    private int pendingBlockEnergyReward = 1;
    private float pendingBlockCounterDamage = 10f;
    private bool blockTriggeredThisAttack;

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
            StartCoroutine(PerformCardAnimation(card, animation, onAction, onComplete));
        }
        else
        {
            // Si no hay animación, ejecutar directamente
            onAction?.Invoke();
            onComplete?.Invoke();
        }
    }

    private IEnumerator PerformCardAnimation(CardData card, CardAnimation baseAnimation, System.Action onAction, System.Action onComplete)
    {
        // Determinar trigger y tiempos usando primero los valores personalizados de la carta (si existen)
        string triggerToUse = baseAnimation.animationTrigger;
        if (card != null && !string.IsNullOrEmpty(card.customAnimationTrigger))
        {
            triggerToUse = card.customAnimationTrigger;
        }

        float actionPointTime = baseAnimation.actionPointTime;
        if (card != null && card.customActionPointTime > 0f)
        {
            actionPointTime = card.customActionPointTime;
        }

        float animationDuration = baseAnimation.animationDuration;
        if (card != null && card.customAnimationDuration > 0f)
        {
            animationDuration = card.customAnimationDuration;
        }

        // Desactivar objetos configurados mientras dura la animación
        SetObjectsHiddenDuringAnimation(false);

        // Lanzar la animación del jugador
        if (animator != null && !string.IsNullOrEmpty(triggerToUse))
        {
            animator.SetTrigger(triggerToUse);
        }

        // Reproducir sonido específico de la carta si existe
        if (card != null && card.cardSound != null && GameManager.Instance != null && GameManager.Instance.audioSource != null)
        {
            GameManager.Instance.audioSource.PlayOneShot(card.cardSound);
        }

        // Esperar hasta el punto de acción de la animación
        yield return new WaitForSeconds(actionPointTime);
        
        onAction?.Invoke();

        // Esperar el resto de la animación
        float remainingTime = animationDuration - actionPointTime;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // Reactivar objetos tras completar la animación
        SetObjectsHiddenDuringAnimation(true);

        onComplete?.Invoke();
    }

    /// <summary>
    /// Animación del jugador para carta/objeto (usa customAnimationTrigger del ObjectCardData).
    /// </summary>
    public void PlayObjectCardAnimation(ObjectCardData card, System.Action onAction, System.Action onComplete = null)
    {
        if (card == null)
        {
            onAction?.Invoke();
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(PerformObjectCardAnimation(card, onAction, onComplete));
    }

    private IEnumerator PerformObjectCardAnimation(ObjectCardData card, System.Action onAction, System.Action onComplete)
    {
        string triggerToUse = card.customAnimationTrigger;
        float actionPointTime = card.customActionPointTime > 0f ? card.customActionPointTime : 0.5f;
        float animationDuration = card.customAnimationDuration > 0f ? card.customAnimationDuration : 1f;

        if (string.IsNullOrEmpty(triggerToUse))
        {
            CardAnimation fallback = GetAnimationForCard(CardData.CardType.Heal);
            if (fallback != null)
            {
                triggerToUse = fallback.animationTrigger;
                if (card.customActionPointTime <= 0f) actionPointTime = fallback.actionPointTime;
                if (card.customAnimationDuration <= 0f) animationDuration = fallback.animationDuration;
            }
        }

        SetObjectsHiddenDuringAnimation(false);

        if (animator != null && !string.IsNullOrEmpty(triggerToUse))
            animator.SetTrigger(triggerToUse);

        if (card.cardSound != null && GameManager.Instance != null && GameManager.Instance.audioSource != null)
            GameManager.Instance.audioSource.PlayOneShot(card.cardSound);

        yield return new WaitForSeconds(actionPointTime);
        onAction?.Invoke();

        float remainingTime = animationDuration - actionPointTime;
        if (remainingTime > 0f)
            yield return new WaitForSeconds(remainingTime);

        SetObjectsHiddenDuringAnimation(true);
        onComplete?.Invoke();
    }

    private void SetObjectsHiddenDuringAnimation(bool active)
    {
        if (objectsToDisableDuringCardAnimation == null) return;
        for (int i = 0; i < objectsToDisableDuringCardAnimation.Length; i++)
        {
            GameObject obj = objectsToDisableDuringCardAnimation[i];
            if (obj != null)
                obj.SetActive(active);
        }
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

    /// <summary>
    /// Obtiene el array de animaciones de cartas (para uso en otros scripts)
    /// </summary>
    public CardAnimation[] GetCardAnimations()
    {
        return cardAnimations;
    }

    // Método para activar el bloqueo (ahora solo lo marca como pendiente)
    public void ActivateBlock(float reductionMultiplier)
    {
        MergeBlockState(reductionMultiplier, CardData.BlockBonusType.None, 1, 0f);
    }

    public void ActivateBlock(float reductionMultiplier, CardData.BlockBonusType bonusType, int energyReward, float counterDamage)
    {
        MergeBlockState(reductionMultiplier, bonusType, energyReward, counterDamage);
    }

    /// <summary>
    /// Acumula bloqueo: mejor reducción (menor multiplicador) y conserva bonus de carta si el botón no trae bonus.
    /// </summary>
    private void MergeBlockState(float reductionMultiplier, CardData.BlockBonusType bonusType, int energyReward, float counterDamage)
    {
        blockReductionMultiplier = Mathf.Min(blockReductionMultiplier, reductionMultiplier);

        if (bonusType != CardData.BlockBonusType.None)
        {
            pendingBlockBonus = bonusType;
            pendingBlockEnergyReward = Mathf.Max(1, energyReward);
            pendingBlockCounterDamage = counterDamage;
        }

        hasBlockPending = true;
        Debug.Log($"[PlayerController] Bloqueo pendiente. Reducción: {blockReductionMultiplier}, Bonus: {pendingBlockBonus}, Counter: {pendingBlockCounterDamage}");
    }

    // Método para activar el bloqueo pendiente (se llama cuando el enemigo ataca)
    public void ActivatePendingBlock()
    {
        if (hasBlockPending || hasBlockActive)
        {
            hasBlockActive = true;
            hasBlockPending = false;
            blockTriggeredThisAttack = true;
            Debug.Log($"[PlayerController] Bloqueo activo para ataque enemigo. Bonus: {pendingBlockBonus}, Counter: {pendingBlockCounterDamage}");
            
            PlayBlockIdleAnimation();
            StartBlockAutoDeactivateTimer();
        }
    }

    public bool HasPendingBlockBonus()
    {
        return pendingBlockBonus != CardData.BlockBonusType.None;
    }

    public bool ShouldResolveBlockBonusThisAttack()
    {
        return blockTriggeredThisAttack && HasPendingBlockBonus();
    }

    public void ClearBlockBonuses()
    {
        pendingBlockBonus = CardData.BlockBonusType.None;
        pendingBlockEnergyReward = 1;
        pendingBlockCounterDamage = 0f;
        blockTriggeredThisAttack = false;
    }

    // Método para desactivar el bloqueo
    public void DeactivateBlock()
    {
        hasBlockActive = false;
        hasBlockPending = false;
        blockReductionMultiplier = 1.0f;
        
        StopBlockIdleAnimation();
        StopBlockAutoDeactivateTimer();
        
        Debug.Log("[PlayerController] Bloqueo mecánico desactivado (bonus conservado hasta resolver)");
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

    /// <summary>
    /// Llamado cuando el jugador bloquea un ataque enemigo (daño reducido por bloqueo activo).
    /// </summary>
    public void ResolveBlockBonusOnEnemyHit()
    {
        if (pendingBlockBonus == CardData.BlockBonusType.None)
            return;

        if (!blockTriggeredThisAttack && !hasBlockActive)
        {
            Debug.LogWarning("[PlayerController] Bonus de bloqueo sin bloqueo activo en este ataque.");
            return;
        }

        if (GameManager.Instance == null)
            return;

        switch (pendingBlockBonus)
        {
            case CardData.BlockBonusType.RewardEnergyOnBlock:
                GameManager.Instance.RefundEnergy(pendingBlockEnergyReward);
                Debug.Log($"[PlayerController] Bloqueo exitoso: +{pendingBlockEnergyReward} energía");
                break;

            case CardData.BlockBonusType.CounterDamageOnBlock:
                int counterDamage = Mathf.RoundToInt(pendingBlockCounterDamage);
                if (counterDamage <= 0)
                {
                    Debug.LogWarning("[PlayerController] blockCounterDamage es 0; revisa el asset de la carta.");
                    break;
                }

                if (GameManager.Instance.enemyHealth != null)
                {
                    GameManager.Instance.enemyHealth.TakeDamage(counterDamage);
                    GameManager.Instance.PlayEnemyDamageSound();
                    Debug.Log($"[PlayerController] Contraataque: {counterDamage} daño al enemigo");
                }
                else if (GameManager.Instance.enemyController != null)
                {
                    GameManager.Instance.enemyController.TakeDamage(counterDamage);
                    GameManager.Instance.PlayEnemyDamageSound();
                    Debug.Log($"[PlayerController] Contraataque (EnemyController): {counterDamage} daño");
                }
                else
                {
                    Debug.LogError("[PlayerController] No hay enemyHealth ni enemyController para contraataque.");
                }
                break;
        }

        ClearBlockBonuses();
    }

    // Método llamado cuando el jugador recibe daño
    private void OnTakeDamage(int damage)
    {
        // Reproducir sonido de daño a través del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayPlayerDamageSound();
        }
        
        // Reproducir animación de daño apropiada
        if (hasBlockActive)
        {
            // Si está bloqueando, reproducir animación de daño mientras bloquea
            PlayDamageWhileBlockingAnimation();
            // El bloqueo se mantiene activo después de recibir daño
        }
        else
        {
            // Si no está bloqueando, reproducir animación de daño normal
            PlayDamageAnimation();
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
    /// Reproduce la animación de bloqueo activo (idle)
    /// </summary>
    public void PlayBlockIdleAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(blockIdleAnimationTrigger))
        {
            animator.SetBool(blockIdleAnimationTrigger, true);
        }

        Debug.Log("¡Animación de bloqueo activo ejecutada!");
    }
    
    /// <summary>
    /// Detiene la animación de bloqueo activo
    /// </summary>
    public void StopBlockIdleAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(blockIdleAnimationTrigger))
        {
            animator.SetBool(blockIdleAnimationTrigger, false);
        }
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
    
    /// <summary>
    /// Reproduce la animación de recibir daño mientras bloquea
    /// </summary>
    public void PlayDamageWhileBlockingAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(damageWhileBlockingAnimationTrigger))
        {
            animator.SetTrigger(damageWhileBlockingAnimationTrigger);
        }

        Debug.Log("¡Animación de recibir daño mientras bloquea ejecutada!");
    }
    
    /// <summary>
    /// Inicia el temporizador para desactivar el bloqueo automáticamente
    /// </summary>
    private void StartBlockAutoDeactivateTimer()
    {
        StopBlockAutoDeactivateTimer();
        blockAutoDeactivateCoroutine = StartCoroutine(BlockAutoDeactivateTimer());
    }
    
    /// <summary>
    /// Detiene el temporizador de desactivación automática del bloqueo
    /// </summary>
    private void StopBlockAutoDeactivateTimer()
    {
        if (blockAutoDeactivateCoroutine != null)
        {
            StopCoroutine(blockAutoDeactivateCoroutine);
            blockAutoDeactivateCoroutine = null;
        }
    }
    
    /// <summary>
    /// Corrutina que desactiva el bloqueo automáticamente después de un tiempo
    /// </summary>
    private IEnumerator BlockAutoDeactivateTimer()
    {
        yield return new WaitForSeconds(blockAutoDeactivateTime);

        if (GameManager.Instance != null &&
            GameManager.Instance.currentTurn == GameManager.TurnState.EnemyTurn)
        {
            yield break;
        }

        if (hasBlockActive)
        {
            DeactivateBlock();
            Debug.Log("[PlayerController] Bloqueo desactivado por tiempo (fuera del turno enemigo)");
        }
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