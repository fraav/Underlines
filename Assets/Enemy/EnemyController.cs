using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private float turnDuration = 3.0f;

    private int attackTriggerHash;

    void Start()
    {
        // Convertir el nombre a hash para mejor rendimiento
        attackTriggerHash = Animator.StringToHash(attackTrigger);
    }

    public void StartEnemyTurn()
    {
        StartCoroutine(PerformEnemyTurn());
    }

    private IEnumerator PerformEnemyTurn()
    {
        Debug.Log("Turno del Enemigo - Iniciando acción");

        // Activar trigger de ataque
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger(attackTriggerHash);
        }
        else
        {
            Debug.LogWarning("Animator del enemigo no asignado!");
        }

        // Esperar mientras se ejecuta la animación
        yield return new WaitForSeconds(turnDuration);

        Debug.Log("Turno del Enemigo - Finalizado");

        // Terminar turno y volver al jugador
        GameManager.Instance.StartPlayerTurn();
    }
}