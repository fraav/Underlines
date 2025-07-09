using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class CardAnimation
    {
        public CardData.CardType cardType;
        public string animationTrigger;
        public float actionPointTime = 0.5f;
        public float animationDuration = 1f;
        public ParticleSystem effect;
        public AudioClip sound;
    }

    [Header("Card Animations")]
    [SerializeField] private CardAnimation[] cardAnimations;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject highlightEffect;

    public HealthSystem healthSystem;

    public void PlayCardAnimation(CardData card, System.Action onAction, System.Action onComplete)
    {
        CardAnimation animation = GetAnimationForCard(card.cardType);
        if (animation == null)
        {
            Debug.LogWarning($"No animation found for card type: {card.cardType}");
            onAction?.Invoke();
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(PerformCardAnimation(animation, onAction, onComplete));
    }

    private IEnumerator PerformCardAnimation(CardAnimation animation, System.Action onAction, System.Action onComplete)
    {
        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger(animation.animationTrigger);
        }

        // Play effects
        if (animation.effect != null)
        {
            animation.effect.Play();
        }

        if (animation.sound != null)
        {
            AudioSource.PlayClipAtPoint(animation.sound, transform.position);
        }

        // Wait for action point
        yield return new WaitForSeconds(animation.actionPointTime);
        onAction?.Invoke();

        // Wait for remaining animation time
        float remainingTime = animation.animationDuration - animation.actionPointTime;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        onComplete?.Invoke();
    }

    private CardAnimation GetAnimationForCard(CardData.CardType cardType)
    {
        foreach (var anim in cardAnimations)
        {
            if (anim.cardType == cardType)
            {
                return anim;
            }
        }
        return null;
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