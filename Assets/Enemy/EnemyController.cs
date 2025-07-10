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
        SetupHealthSystem();
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
        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        EnemyAttack attack = SelectAttack();
        if (animator != null && !string.IsNullOrEmpty(attack.animationName))
        {
            animator.Play(attack.animationName);
        }

        if (attack.effect != null)
        {
            attack.effect.Play();
        }

        if (attack.sound != null)
        {
            AudioSource.PlayClipAtPoint(attack.sound, transform.position);
        }

        yield return new WaitForSeconds(attack.duration);

        if (GameManager.Instance != null && GameManager.Instance.playerHealth != null)
        {
            GameManager.Instance.playerHealth.TakeDamage(attack.damage);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartPlayerTurn();
        }
    }

    private EnemyAttack SelectAttack()
    {
        if (attacks.Length == 0) return new EnemyAttack();
        if (attacks.Length == 1) return attacks[0];

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
        if (!isDead) healthSystem?.TakeDamage(damage);
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDefeated();
        }
        Destroy(gameObject, 2f);
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
}