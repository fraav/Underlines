using UnityEngine;
using System.Collections;

public class ImpactFrame : MonoBehaviour
{
    [SerializeField] private GameObject blackPanel;  // Sprite negro detrás del player
    [SerializeField] private GameObject whitePanel;  // Sprite blanco detrás del player
    [SerializeField] private float flashDuration = 1f;

    private Coroutine currentCoroutine;

    // Evento para impacto negro
    public void FlashBlack()
    {
        // Detener solo la corrutina actual si existe
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // Asegurar que ambos paneles están en estado correcto
        if (whitePanel != null) whitePanel.SetActive(false);

        currentCoroutine = StartCoroutine(DoFlashBlack());
    }

    // Evento para impacto blanco
    public void FlashWhite()
    {
        // Detener solo la corrutina actual si existe
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // Asegurar que ambos paneles están en estado correcto
        if (blackPanel != null) blackPanel.SetActive(false);

        currentCoroutine = StartCoroutine(DoFlashWhite());
    }

    private IEnumerator DoFlashBlack()
    {
        if (blackPanel == null) yield break;

        blackPanel.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        blackPanel.SetActive(false);

        currentCoroutine = null;
    }

    private IEnumerator DoFlashWhite()
    {
        if (whitePanel == null) yield break;

        whitePanel.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        whitePanel.SetActive(false);

        currentCoroutine = null;
    }
}