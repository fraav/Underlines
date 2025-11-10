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

    [Header("Dialogue Settings")]
    public string lowHealthDialogue = "EnemyLowHealth";
    private bool lowHealthTriggered = false;

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
        healthSystem.OnTakeDamage.AddListener(OnTakeDamage);
    }

    private void OnTakeDamage(int damage)
    {
        if (isDead) return;
        CheckLowHealth();
    }

    private void CheckLowHealth()
    {
        if (healthSystem != null && healthSystem.CurrentHealth <= healthSystem.MaxHealth / 2 && !lowHealthTriggered)
        {
            TriggerDialogue(lowHealthDialogue);
            lowHealthTriggered = true;
        }
    }

    private void TriggerDialogue(string dialogueName)
    {
        if (!string.IsNullOrEmpty(dialogueName) && DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.StartDialogue(dialogueName);
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
        
        if (action.actionType == EnemyAction.ActionType.Damage && GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyAttackStart();
        }
        
        if (animator != null && !string.IsNullOrEmpty(action.animationName))
        {
            animator.Play(action.animationName);
        }

        if (!action.syncSoundWithAction)
        {
            PlayActionSound(action);
        }

        yield return new WaitForSeconds(action.actionPointTime);
        
        ApplyActionEffect(action);
        
        if (action.syncSoundWithAction)
        {
            PlayActionSound(action);
        }

        float remainingTime = action.duration - action.actionPointTime;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyTurnEnd();
            GameManager.Instance.StartPlayerTurn();
        }
    }
    
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
                    
                    if (GameManager.Instance.playerController != null && 
                        GameManager.Instance.playerController.HasBlockActive())
                    {
                        float blockMultiplier = GameManager.Instance.playerController.GetBlockReductionMultiplier();
                        finalDamage = Mathf.RoundToInt(finalDamage * blockMultiplier);
                        GameManager.Instance.OnEnemyAttackApplied();
                    }
                    
                    GameManager.Instance.playerHealth.TakeDamage(finalDamage);
                    OnEnemyAttack.Invoke();
                }
                break;

            case EnemyAction.ActionType.Heal:
                if (healthSystem != null)
                {
                    healthSystem.Heal(action.value);
                    OnEnemyHeal.Invoke();
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
            StartCoroutine(PlayDamageAnimationWithSound());
        }
    }
    
    private IEnumerator PlayDamageAnimationWithSound()
    {
        if (animator != null && !string.IsNullOrEmpty(damageAnimationTrigger))
        {
            animator.SetTrigger(damageAnimationTrigger);
        }

        if (!syncDamageSoundWithAction)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayEnemyDamageSoundFromController();
            }
        }

        if (syncDamageSoundWithAction)
        {
            yield return new WaitForSeconds(damageSoundPointTime);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayEnemyDamageSoundFromController();
            }
        }

        float remainingDamageTime = damageAnimationDuration - damageSoundPointTime;
        if (remainingDamageTime > 0)
        {
            yield return new WaitForSeconds(remainingDamageTime);
        }
        
        yield return null;
    }
    
    public void PlayDamageAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(damageAnimationTrigger))
        {
            animator.SetTrigger(damageAnimationTrigger);
        }
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

       // Destroy(gameObject, 2f);
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