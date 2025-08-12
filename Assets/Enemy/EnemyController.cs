// EnemyController.cs
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    [System.Serializable]
    public class EnemyAction
    {
        public enum ActionType { Damage, Heal }
        public ActionType actionType;
        public string animationName;
        public float duration = 1f;
        public float actionPointTime = 0.5f;
        public int value = 10;
        
        [Header("Audio Timing")]
        [Tooltip("Tiempo en segundos desde el inicio de la animación hasta reproducir el sonido")]
        public float soundPointTime = 0.3f;
        [Tooltip("Si es true, el sonido se reproduce en el punto de acción. Si es false, se reproduce al inicio")]
        public bool syncSoundWithAction = true;
    }

    [Header("Actions")]
    [SerializeField] private EnemyAction[] actions;
    [SerializeField] private GameObject highlightEffect;
    [SerializeField] private Animator animator;

    [Header("Reward Settings")]
    [SerializeField] private int deathReward = 50;

    [Header("Attack Settings")]
    public int baseAttack = 10;
    public float currentAttackMultiplier = 1.0f;
    private float originalAttackMultiplier = 1.0f;
    
    [Header("Damage Animations")]
    [SerializeField] private string damageAnimationTrigger = "TakeDamage";
    [SerializeField] private float damageActionPointTime = 0.2f;
    [Header("Damage Audio Timing")]
    [Tooltip("Tiempo en segundos desde el inicio de la animación hasta reproducir el sonido de daño")]
    [SerializeField] private float damageSoundPointTime = 0.1f;
    [Tooltip("Si es true, el sonido se reproduce en el punto de acción. Si es false, se reproduce al inicio")]
    [SerializeField] private bool syncDamageSoundWithAction = true;
    [Tooltip("Duración de la animación de daño")]
    [SerializeField] private float damageAnimationDuration = 0.5f;

    public HealthSystem healthSystem;
    private int lastActionIndex = -1;
    private bool isDead = false;

    // Nuevos eventos para acciones del enemigo
    public UnityEvent OnEnemyAttack = new UnityEvent();
    public UnityEvent OnEnemyHeal = new UnityEvent();

    void Start()
    {
        SetupHealthSystem();
        originalAttackMultiplier = currentAttackMultiplier;
    }

    private void OnEnable()
    {
        EconomyManager.OnRewardGiven += HandleReward;
    }

    private void OnDisable()
    {
        EconomyManager.OnRewardGiven -= HandleReward;
    }

    private void SetupHealthSystem()
    {
        if (healthSystem == null)
        {
            healthSystem = GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                healthSystem = gameObject.AddComponent<HealthSystem>();
            }
        }
        healthSystem.SetMaxHealth(100);
        healthSystem.OnDeath.AddListener(OnDeath);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemyHealth = healthSystem;
        }
    }

    public void StartEnemyTurn()
    {
        if (isDead) return;
        StartCoroutine(PerformAction());
    }

    private IEnumerator PerformAction()
    {
        EnemyAction action = SelectAction();
        
        // Notificar al GameManager antes de iniciar la animación para activar bloqueo
        if (action.actionType == EnemyAction.ActionType.Damage && GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyAttackStart();
        }
        
        if (animator != null && !string.IsNullOrEmpty(action.animationName))
        {
            animator.Play(action.animationName);
        }

        // Reproducir sonido al inicio si no está sincronizado con la acción
        if (!action.syncSoundWithAction)
        {
            PlayActionSound(action);
        }

        // Esperar hasta el punto de acción para aplicar el efecto
        yield return new WaitForSeconds(action.actionPointTime);
        
        // Aplicar el efecto en el momento exacto de la acción
        ApplyActionEffect(action);
        
        // Reproducir sonido en el punto de acción si está sincronizado
        if (action.syncSoundWithAction)
        {
            PlayActionSound(action);
        }

        float remainingTime = action.duration - action.actionPointTime;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        // Notificar al GameManager que el turno del enemigo ha terminado
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyTurnEnd();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartPlayerTurn();
        }
    }
    
    /// <summary>
    /// Reproduce el sonido de la acción del enemigo
    /// </summary>
    private void PlayActionSound(EnemyAction action)
    {
        switch (action.actionType)
        {
            case EnemyAction.ActionType.Damage:
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayEnemyAttackSound();
                }
                break;
            case EnemyAction.ActionType.Heal:
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayEnemyHealSound();
                }
                break;
        }
    }

    private void ApplyActionEffect(EnemyAction action)
    {
        switch (action.actionType)
        {
            case EnemyAction.ActionType.Damage:
                if (GameManager.Instance != null && GameManager.Instance.playerHealth != null)
                {
                    int finalDamage = Mathf.RoundToInt(action.value * currentAttackMultiplier);
                    
                    // Verificar si el jugador tiene bloqueo activo
                    if (GameManager.Instance.playerController != null && 
                        GameManager.Instance.playerController.HasBlockActive())
                    {
                        float blockMultiplier = GameManager.Instance.playerController.GetBlockReductionMultiplier();
                        finalDamage = Mathf.RoundToInt(finalDamage * blockMultiplier);
                        Debug.Log($"Jugador bloqueó el ataque! Daño reducido de {action.value * currentAttackMultiplier} a {finalDamage}");
                        
                        // Notificar al GameManager que el ataque fue aplicado (para sonido de bloqueo)
                        GameManager.Instance.OnEnemyAttackApplied();
                    }
                    
                    GameManager.Instance.playerHealth.TakeDamage(finalDamage);
                    OnEnemyAttack.Invoke(); // Invocar evento de ataque
                }
                break;

            case EnemyAction.ActionType.Heal:
                if (healthSystem != null)
                {
                    healthSystem.Heal(action.value);
                    OnEnemyHeal.Invoke(); // Invocar evento de curación
                }
                break;
        }
    }

    private EnemyAction SelectAction()
    {
        if (actions.Length == 0) return new EnemyAction();
        if (actions.Length == 1) return actions[0];

        int newIndex;
        do
        {
            newIndex = Random.Range(0, actions.Length);
        } while (newIndex == lastActionIndex && actions.Length > 1);

        lastActionIndex = newIndex;
        return actions[newIndex];
    }

    public void TakeDamage(int damage)
    {
        if (!isDead) 
        {
            healthSystem?.TakeDamage(damage);
            // Reproducir animación de recibir daño y sincronizar el sonido
            StartCoroutine(PlayDamageAnimationWithSound());
        }
    }
    
    /// <summary>
    /// Reproduce la animación de recibir daño del enemigo con sonido sincronizado
    /// </summary>
    private IEnumerator PlayDamageAnimationWithSound()
    {
        if (animator != null && !string.IsNullOrEmpty(damageAnimationTrigger))
        {
            animator.SetTrigger(damageAnimationTrigger);
        }

        // Reproducir sonido al inicio si no está sincronizado con la acción
        if (!syncDamageSoundWithAction)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayEnemyDamageSoundFromController();
            }
        }

        // Esperar hasta el punto de acción si está sincronizado
        if (syncDamageSoundWithAction)
        {
            yield return new WaitForSeconds(damageSoundPointTime);
            
            // Reproducir sonido de daño del enemigo en el momento exacto configurado
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayEnemyDamageSoundFromController();
            }
        }

        // Esperar el resto de la animación de daño
        float remainingDamageTime = damageAnimationDuration - damageSoundPointTime;
        if (remainingDamageTime > 0)
        {
            yield return new WaitForSeconds(remainingDamageTime);
        }

        Debug.Log("¡Animación de recibir daño del enemigo ejecutada con sonido configurado!");
        
        // Asegurar que todos los caminos de código retornen un valor
        yield return null;
    }
    
    /// <summary>
    /// Reproduce la animación de recibir daño del enemigo (método público para llamadas externas)
    /// </summary>
    public void PlayDamageAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(damageAnimationTrigger))
        {
            animator.SetTrigger(damageAnimationTrigger);
        }

        Debug.Log("¡Animación de recibir daño del enemigo ejecutada!");
    }

    private void OnDeath()
    {
        isDead = true;
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Collider collider3D = GetComponent<Collider>();
        Collider2D collider2D = GetComponent<Collider2D>();

        if (collider3D != null) collider3D.enabled = false;
        if (collider2D != null) collider2D.enabled = false;

        EconomyManager.GiveReward(deathReward);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemyController = null;
            GameManager.Instance.enemyHealth = null;
            GameManager.Instance.OnEnemyDefeated();
        }

        Destroy(gameObject, 2f);
    }

    private void HandleReward(int amount)
    {
        if (isDead && amount == deathReward)
        {
            Debug.Log($"Enemy gave reward: {amount}");
        }
    }

    public void SetHighlight(bool active)
    {
        if (highlightEffect != null) highlightEffect.SetActive(active);
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectTarget(gameObject);
        }
    }

    public void ApplyAttackReduction(float reductionMultiplier)
    {
        currentAttackMultiplier = reductionMultiplier;
    }

    public void ResetAttackMultiplier()
    {
        currentAttackMultiplier = originalAttackMultiplier;
    }
}