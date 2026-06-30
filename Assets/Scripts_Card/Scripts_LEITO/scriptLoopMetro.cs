using UnityEngine;
using System.Collections;

public class TimedObjectActivator : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject targetObject;     // El objeto que se activará
    [SerializeField] private float intervalSeconds = 3f;  // Tiempo entre activaciones
    [SerializeField] private float activeDuration = 1f;   // Cuánto tiempo permanece activo

    private void Start()
    {
        StartCoroutine(ActivateLoop());
    }

    private IEnumerator ActivateLoop()
    {
        while (true)
        {
            // Activar
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }

            // Esperar duración activa
            yield return new WaitForSeconds(activeDuration);

            // Desactivar
            if (targetObject != null)
            {
                targetObject.SetActive(false);
            }

            // Esperar hasta la siguiente activación
            yield return new WaitForSeconds(intervalSeconds);
        }
    }
}
