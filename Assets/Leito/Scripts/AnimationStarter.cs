using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationStarter : MonoBehaviour
{
    [SerializeField] private string defaultAnimation = "FadeOut";
    
    void Start()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null && !string.IsNullOrEmpty(defaultAnimation))
        {
            animator.Play(defaultAnimation, 0, 0f);
            animator.Update(0f);
        }
    }
}