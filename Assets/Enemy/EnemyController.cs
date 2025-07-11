using UnityEngine;
using System.Collections;

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
        public ParticleSystem effect;
        public AudioClip sound;
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

    public HealthSystem healthSystem;
    private int lastActionIndex = -1;
    private bool isDead = false;

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
        if (animator != null && !string.IsNullOrEmpty(action.animationName))
        {
            animator.Play(action.animationName);
        }

        if (action.effect != null)
        {
            action.effect.Play();
        }

        if (action.sound != null)
        {
            AudioSource.PlayClipAtPoint(action.sound, transform.position);
        }

        yield return new WaitForSeconds(action.actionPointTime);
        ApplyActionEffect(action);

        float remainingTime = action.duration - action.actionPointTime;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartPlayerTurn();
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
                    GameManager.Instance.playerHealth.TakeDamage(finalDamage);
                }
                break;

            case EnemyAction.ActionType.Heal:
                if (healthSystem != null)
                {
                    healthSystem.Heal(action.value);
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
        if (!isDead) healthSystem?.TakeDamage(damage);
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