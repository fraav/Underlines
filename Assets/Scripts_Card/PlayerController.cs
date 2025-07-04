using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerCardAction
    {
        public CardData.CardType cardType;
        public string animationTrigger;
        public float damagePointTime; // Tiempo para aplicar daño
        public float animationDuration;
        public ParticleSystem effect;
        public AudioClip sound;
    }

    [Header("Configuración de Acciones")]
    [SerializeField] private PlayerCardAction[] cardActions;

    [Header("Referencias")]
    [SerializeField] private Animator animator;

    private PlayerCardAction GetActionForCard(CardData.CardType type)
    {
        foreach (var action in cardActions)
        {
            if (action.cardType == type) return action;
        }
        return null;
    }

    public void PlayCardAnimation(CardData card, System.Action damageCallback, System.Action animationComplete)
    {
        PlayerCardAction action = GetActionForCard(card.cardType);
        if (action == null)
        {
            Debug.LogError($"No hay acción configurada para el tipo de carta: {card.cardType}");
            damageCallback?.Invoke();
            animationComplete?.Invoke();
            return;
        }

        StartCoroutine(PerformCardAction(action, damageCallback, animationComplete));
    }

    private IEnumerator PerformCardAction(PlayerCardAction action, System.Action damageCallback, System.Action animationComplete)
    {
        // Activar animación
        if (animator != null)
        {
            animator.SetTrigger(action.animationTrigger);
        }
        else
        {
            Debug.LogWarning("Animator del jugador no asignado!");
        }

        // Activar efecto visual
        if (action.effect != null) action.effect.Play();

        // Reproducir sonido
        if (action.sound != null) AudioSource.PlayClipAtPoint(action.sound, transform.position);

        // Esperar hasta el momento de aplicar daño
        yield return new WaitForSeconds(action.damagePointTime);
        
        // Aplicar daño
        damageCallback?.Invoke();

        // Esperar el resto de la animación
        float remainingTime = action.animationDuration - action.damagePointTime;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);
        
        // Completar animación
        animationComplete?.Invoke();
    }
}