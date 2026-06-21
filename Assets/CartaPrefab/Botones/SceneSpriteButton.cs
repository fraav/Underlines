using UnityEngine;
using UnityEngine.Events;

public class SceneSpriteButton : MonoBehaviour
{
    [Header("Visual Settings")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.9f, 0.9f, 0.9f);
    public Color pressedColor = new Color(0.75f, 0.75f, 0.75f);

    [Header("Optional Sprites")]
    public Sprite normalSprite;
    public Sprite hoverSprite;
    public Sprite pressedSprite;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    [Header("Events (like Unity Button)")]
    public UnityEvent onClick;

    bool isHovering = false;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        ApplyNormalState();
    }

    private void OnMouseEnter()
    {
        isHovering = true;
        ApplyHoverState();
    }

    private void OnMouseExit()
    {
        isHovering = false;
        ApplyNormalState();
    }

    private void OnMouseDown()
    {
        ApplyPressedState();
    }

    private void OnMouseUp()
    {
        if (isHovering)
        {
            ApplyHoverState();
            PlaySound();
            onClick?.Invoke();
        }
        else
        {
            ApplyNormalState();
        }
    }

    // --------------------------------------------------------
    // STATE HANDLING
    // --------------------------------------------------------

    void ApplyNormalState()
    {
        spriteRenderer.color = normalColor;

        if (normalSprite != null)
            spriteRenderer.sprite = normalSprite;
    }

    void ApplyHoverState()
    {
        spriteRenderer.color = hoverColor;

        if (hoverSprite != null)
            spriteRenderer.sprite = hoverSprite;
    }

    void ApplyPressedState()
    {
        spriteRenderer.color = pressedColor;

        if (pressedSprite != null)
            spriteRenderer.sprite = pressedSprite;
    }

    void PlaySound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
