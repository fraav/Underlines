using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [System.Serializable]
    public class EnemyAttack
    {
        public string animationName;
        public float duration;
        public int damage;
        public ParticleSystem effect;
        public AudioClip sound;
    }

    [Header("Configuración de Ataques")]
    [SerializeField] private EnemyAttack[] attacks;

    [Header("Referencias")]
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private Transform healthBarPosition;

    [Header("Configuración de Salud")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject healthBarPrefab;

    private HealthSystem healthSystem;
    private HealthBarUI healthBarUI;
    private int lastAttackIndex = -1;
    private bool isDead = false;

    void Start()
    {
        // Inicializar sistema de salud
        healthSystem = gameObject.AddComponent<HealthSystem>();
        healthSystem.SetMaxHealth(maxHealth);

        // Suscribir a eventos de salud
        healthSystem.OnHealthChanged.AddListener(OnHealthChanged);
        healthSystem.OnDeath.AddListener(OnEnemyDeath);

        // Crear barra de vida
        if (healthBarPrefab != null && healthBarPosition != null)
        {
            GameObject healthBarObj = Instantiate(healthBarPrefab, healthBarPosition.position, Quaternion.identity);
            healthBarUI = healthBarObj.GetComponent<HealthBarUI>();

            if (healthBarUI != null)
            {
                healthBarUI.SetHealthSystem(healthSystem);
            }
        }

        // Validar configuración
        if (attacks.Length == 0)
        {
            Debug.LogError("No hay ataques configurados para el enemigo!");
        }

        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }
    }

    public void StartEnemyTurn()
    {
        if (isDead) return;

        StartCoroutine(PerformEnemyTurn());
    }

    private IEnumerator PerformEnemyTurn()
    {
        Debug.Log("Turno del Enemigo - Iniciando acción");

        // Seleccionar ataque aleatorio
        int attackIndex = SelectRandomAttack();
        EnemyAttack selectedAttack = attacks[attackIndex];

        // Ejecutar animación
        if (enemyAnimator != null)
        {
            enemyAnimator.Play(selectedAttack.animationName);
        }
        else
        {
            Debug.LogWarning("Animator del enemigo no asignado!");
        }

        // Activar efecto visual si existe
        if (selectedAttack.effect != null)
        {
            selectedAttack.effect.Play();
        }

        // Reproducir sonido si existe
        if (selectedAttack.sound != null)
        {
            AudioSource.PlayClipAtPoint(selectedAttack.sound, transform.position);
        }

        // Aplicar daño al jugador
        if (GameManager.Instance != null && GameManager.Instance.playerHealth != null)
        {
            GameManager.Instance.playerHealth.TakeDamage(selectedAttack.damage);
        }

        // Esperar mientras se ejecuta la animación
        yield return new WaitForSeconds(selectedAttack.duration);

        Debug.Log("Turno del Enemigo - Finalizado");

        // Terminar turno y volver al jugador
        GameManager.Instance.StartPlayerTurn();
    }

    private int SelectRandomAttack()
    {
        // Si solo hay un ataque
        if (attacks.Length == 1) return 0;

        // Seleccionar aleatoriamente evitando repeticiones consecutivas
        int newIndex;
        do
        {
            newIndex = Random.Range(0, attacks.Length);
        } while (newIndex == lastAttackIndex && attacks.Length > 1);

        lastAttackIndex = newIndex;
        return newIndex;
    }

    private void OnHealthChanged(int currentHealth)
    {
        // Feedback visual cuando el enemigo recibe daño
        if (currentHealth < healthSystem.MaxHealth)
        {
            StartCoroutine(FlashDamageEffect());
        }
    }

    private IEnumerator FlashDamageEffect()
    {
        if (TryGetComponent<SpriteRenderer>(out var renderer))
        {
            Color originalColor = renderer.color;
            renderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.color = originalColor;
        }
    }

    private void OnEnemyDeath()
    {
        isDead = true;
        Debug.Log("¡Enemigo derrotado!");

        // Desactivar componentes
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("Die");
        }

        // Desactivar colisiones
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        // Notificar al GameManager
        GameManager.Instance.OnEnemyDefeated();

        // Destruir después de un tiempo
        Destroy(gameObject, 2f);
    }

    public void TakeDamage(int damage)
    {
        if (healthSystem != null && !isDead)
        {
            healthSystem.TakeDamage(damage);
        }
    }
}