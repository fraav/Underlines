using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [System.Serializable]
    public class EnemyAttack
    {
        public string animationName;
        public float duration = 1f;
        public int damage = 10;
        public ParticleSystem effect;
        public AudioClip sound;
    }

    [Header("Attacks")]
    [SerializeField] private EnemyAttack[] attacks;
    [SerializeField] private GameObject highlightEffect;
    [SerializeField] private Animator animator;

    public HealthSystem healthSystem;
    private int lastAttackIndex = -1;
    private bool isDead = false;

    void Start()
    {
        InitializeHealthSystem();
    }

    private void InitializeHealthSystem()
    {
        if (healthSystem == null)
        {
            healthSystem = GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                healthSystem = gameObject.AddComponent<HealthSystem>();
            }
        }

        healthSystem.OnDeath.AddListener(OnDeath);
    }

    public void StartEnemyTurn()
    {
        if (isDead) return;
        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        EnemyAttack attack = SelectAttack();
        
        // Play animation
        if (animator != null)
        {
            animator.Play(attack.animationName);
        }

        // Play effects
        if (attack.effect != null)
        {
            attack.effect.Play();
        }

        if (attack.sound != null)
        {
            AudioSource.PlayClipAtPoint(attack.sound, transform.position);
        }

        yield return new WaitForSeconds(attack.duration);

        // Apply damage
        if (GameManager.Instance != null && GameManager.Instance.playerHealth != null)
        {
            GameManager.Instance.playerHealth.TakeDamage(attack.damage);
        }

        // End turn
        GameManager.Instance.StartPlayerTurn();
    }

    private EnemyAttack SelectAttack()
    {
        if (attacks.Length == 0)
        {
            Debug.LogError("No attacks configured!");
            return new EnemyAttack();
        }

        if (attacks.Length == 1)
        {
            return attacks[0];
        }

        int newIndex;
        do
        {
            newIndex = Random.Range(0, attacks.Length);
        } while (newIndex == lastAttackIndex && attacks.Length > 1);

        lastAttackIndex = newIndex;
        return attacks[newIndex];
    }

    public void TakeDamage(int damage)
    {
        if (!isDead && healthSystem != null)
        {
            healthSystem.TakeDamage(damage);
        }
    }

    private void OnDeath()
    {
        isDead = true;
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        GameManager.Instance.OnEnemyDefeated();
        Destroy(gameObject, 2f);
    }

    public void SetHighlight(bool active)
    {
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(active);
        }
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectTarget(gameObject);
        }
    }
}