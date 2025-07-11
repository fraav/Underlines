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

    void Start()
    {
        if (healthSystem == null)
        {
            healthSystem = GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                healthSystem = gameObject.AddComponent<HealthSystem>();
                healthSystem.SetMaxHealth(100);
            }
        }
    }

    public void PlayCardAnimation(CardData card, System.Action onAction, System.Action onComplete)
    {
        CardAnimation animation = GetAnimationForCard(card.cardType);
        if (animation == null) return;
        StartCoroutine(PerformCardAnimation(animation, onAction, onComplete));
    }

    private IEnumerator PerformCardAnimation(CardAnimation animation, System.Action onAction, System.Action onComplete)
    {
        if (animator != null && !string.IsNullOrEmpty(animation.animationTrigger))
        {
            animator.SetTrigger(animation.animationTrigger);
        }

        if (animation.effect != null)
        {
            animation.effect.Play();
        }
        else
        {
            Debug.LogWarning("Particle effect is missing for animation: " + animation.animationTrigger);
        }

        if (animation.sound != null)
        {
            AudioSource.PlayClipAtPoint(animation.sound, transform.position);
        }

        yield return new WaitForSeconds(animation.actionPointTime);
        onAction?.Invoke();

        float remainingTime = animation.animationDuration - animation.actionPointTime;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        onComplete?.Invoke();
    }

    private CardAnimation GetAnimationForCard(CardData.CardType cardType)
    {
        foreach (var anim in cardAnimations)
        {
            if (anim.cardType == cardType) return anim;
        }
        return null;
    }

    public void SetHighlight(bool active)
    {
        if (highlightEffect != null) highlightEffect.SetActive(active);
    }

    private void OnMouseDown()
    {
        GameManager.Instance?.SelectTarget(gameObject);
    }
}