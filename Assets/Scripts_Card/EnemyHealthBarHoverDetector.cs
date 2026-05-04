using UnityEngine;

/// <summary>
/// Colócalo en el mismo GameObject del enemigo que tenga un Collider (no trigger, a menos que uses capas adecuadas).
/// Asigna la barra de vida del enemigo que use <see cref="HealthBarUI"/> con <c>enemyStealthBar</c> activado.
/// </summary>
public class EnemyHealthBarHoverDetector : MonoBehaviour
{
    [SerializeField] private HealthBarUI enemyHealthBarUi;

    void OnMouseEnter()
    {
        if (enemyHealthBarUi != null)
        {
            enemyHealthBarUi.SetEnemyBarHover(true);
        }
    }

    void OnMouseExit()
    {
        if (enemyHealthBarUi != null)
        {
            enemyHealthBarUi.SetEnemyBarHover(false);
        }
    }
}
