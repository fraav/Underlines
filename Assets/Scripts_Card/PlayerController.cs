using UnityEngine;
using UnityEngine.Playables;
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

    [Header("Death System")]
    [SerializeField] private PlayableDirector deathTimeline; // Timeline de muerte
    [SerializeField] private GameObject objectToActivateOnDeath; // Objeto opcional a activar

    public HealthSystem healthSystem;

    // Sistema de bloqueo
    private bool hasBlockActive = false;
    private bool hasBlockPending = false; // Nuevo: bloqueo pendiente para el siguiente turno
    private float blockReductionMultiplier = 1.0f;
    private Coroutine blockAutoDeactivateCoroutine;

    // Control de estado de muerte
    private bool isDead = false;

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

        // Suscribirse a los eventos de daño y muerte
        if (healthSystem != null)
        {
            healthSystem.OnTakeDamage.AddListener(OnTakeDamage);
            healthSystem.OnDeath.AddListener(OnDeathTimeline); // Usar la nueva función
        }
    }

    void OnDestroy()
    {
        // Desuscribirse de los eventos para evitar memory leaks
        if (healthSystem != null)
        {
            healthSystem.OnTakeDamage.RemoveListener(OnTakeDamage);
            healthSystem.OnDeath.RemoveListener(OnDeathTimeline);
        }
    }

    void Update()
    {
        // Verificación manual cada frame por si el evento OnDeath no funciona
        if (!isDead && healthSystem != null && healthSystem.CurrentHealth <= 0)
        {
            Debug.Log("🔍 Detección manual: Salud <= 0, activando timeline de muerte");
            OnDeathTimeline();
        }

        // Test con tecla T (opcional - puedes remover esto después de probar)
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestDeath();
        }
    }

    /// <summary>
    /// NUEVA FUNCIÓN: Activa la timeline cuando la vida llega a 0
    /// </summary>
    public void OnDeathTimeline()
    {
        if (isDead) return; // Evitar ejecución múltiple

        isDead = true;
        Debug.Log("🎯 ¡FUNCIÓN OnDeathTimeline EJECUTADA!");

        // 1. Activar la timeline de muerte
        if (deathTimeline != null)
        {
            deathTimeline.Play();
            Debug.Log("✅ Timeline de muerte activada: " + deathTimeline.name);
        }
        else
        {
            Debug.LogError("❌ deathTimeline no está asignada en el Inspector");
        }

        // 2. Activar objeto adicional si existe (opcional)
        if (objectToActivateOnDeath != null)
        {
            objectToActivateOnDeath.SetActive(true);
            Debug.Log("✅ Objeto adicional activado: " + objectToActivateOnDeath.name);
        }

        // 3. Desactivar bloqueo si estaba activo
        DeactivateBlock();

        // 4. Desactivar highlight
        SetHighlight(false);

        // 5. Reproducir animación de muerte si existe
        if (animator != null)
        {
            animator.SetTrigger("Die"); // Asegúrate de tener este trigger en tu Animator
            Debug.Log("✅ Animación de muerte activada");
        }

        Debug.Log("💀 Secuencia de muerte completada correctamente");
    }

    /// <summary>
    /// FUNCIÓN DE RESPALDO: Se puede llamar manualmente desde otros scripts
    /// </summary>
    public void TriggerDeathTimeline()
    {
        if (!isDead)
        {
            Debug.Log("🔄 TriggerDeathTimeline llamado manualmente");
            OnDeathTimeline();
        }
    }

    /// <summary>
    /// FUNCIÓN DE TEST: Para probar la muerte con una tecla
    /// </summary>
    public void TestDeath()
    {
        if (!isDead && healthSystem != null)
        {
            Debug.Log("🧪 TEST: Forzando muerte...");
            healthSystem.TakeDamage(healthSystem.CurrentHealth);
        }
    }

    public void PlayCardAnimation(CardData card, System.Action onAction, System.Action onComplete)
    {
        // Si está muerto, no puede realizar animaciones de cartas
        if (isDead)
        {
            Debug.LogWarning("El jugador está muerto, no puede usar cartas");
            return;
        }

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
        if (objectsToDisableDuringCardAnimation != null)
        {
            for (int i = 0; i < objectsToDisableDuringCardAnimation.Length; i++)
            {
                GameObject obj = objectsToDisableDuringCardAnimation[i];
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

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
        if (objectsToDisableDuringCardAnimation != null)
        {
            for (int i = 0; i < objectsToDisableDuringCardAnimation.Length; i++)
            {
                GameObject obj = objectsToDisableDuringCardAnimation[i];
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
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
        if (isDead) return;

        hasBlockPending = true;
        blockReductionMultiplier = reductionMultiplier;
        Debug.Log($"Bloqueo marcado como pendiente para el siguiente turno del enemigo. Multiplicador: {reductionMultiplier}");
    }

    // Método para activar el bloqueo pendiente (se llama cuando el enemigo ataca)
    public void ActivatePendingBlock()
    {
        if (isDead) return;

        if (hasBlockPending)
        {
            hasBlockActive = true;
            hasBlockPending = false;
            Debug.Log($"Bloqueo activado para este ataque enemigo. Multiplicador: {blockReductionMultiplier}");

            // Iniciar animación de bloqueo activo
            PlayBlockIdleAnimation();

            // Iniciar temporizador para desactivar bloqueo automáticamente
            StartBlockAutoDeactivateTimer();
        }
    }

    // Método para desactivar el bloqueo
    public void DeactivateBlock()
    {
        hasBlockActive = false;
        hasBlockPending = false;
        blockReductionMultiplier = 1.0f;

        // Detener animación de bloqueo
        StopBlockIdleAnimation();

        // Detener temporizador de desactivación automática
        StopBlockAutoDeactivateTimer();

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
        if (isDead) return;

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
        if (isDead) return;

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
        if (isDead) return;

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
        if (isDead) return;

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
        if (isDead) return;

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

        // Solo desactivar si el bloqueo sigue activo (no fue desactivado por daño)
        if (hasBlockActive && !isDead)
        {
            DeactivateBlock();
            Debug.Log("Bloqueo desactivado automáticamente por tiempo");
        }
    }

    /// <summary>
    /// Verifica si el jugador está muerto
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }

    public void SetHighlight(bool active)
    {
        if (isDead) return;
        if (highlightEffect != null) highlightEffect.SetActive(active);
    }

    private void OnMouseDown()
    {
        if (isDead) return;
        GameManager.Instance?.SelectTarget(gameObject);
    }
}