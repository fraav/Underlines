// CardAudioManager.cs
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CardAudioManager : MonoBehaviour
{
    [System.Serializable]
    public class CardSounds
    {
        public AudioClip playSound;
        public AudioClip drawSound;
        public AudioClip discardSound;
        public AudioClip hoverSound;
    }

    [Header("Audio Clips")]
    public CardSounds defaultSounds;
    public CardSounds[] cardTypeSounds;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void PlayCardSound(int cardType, SoundType soundType)
    {
        AudioClip clipToPlay = GetSoundClip(cardType, soundType);
        if (clipToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    private AudioClip GetSoundClip(int cardType, SoundType soundType)
    {
        // Verificar si hay sonidos específicos para este tipo de carta
        if (cardType >= 0 && cardType < cardTypeSounds.Length && cardTypeSounds[cardType] != null)
        {
            switch (soundType)
            {
                case SoundType.Play: return cardTypeSounds[cardType].playSound;
                case SoundType.Draw: return cardTypeSounds[cardType].drawSound;
                case SoundType.Discard: return cardTypeSounds[cardType].discardSound;
                case SoundType.Hover: return cardTypeSounds[cardType].hoverSound;
            }
        }

        // Usar sonidos por defecto si no hay específicos
        switch (soundType)
        {
            case SoundType.Play: return defaultSounds.playSound;
            case SoundType.Draw: return defaultSounds.drawSound;
            case SoundType.Discard: return defaultSounds.discardSound;
            case SoundType.Hover: return defaultSounds.hoverSound;
            default: return null;
        }
    }
}

public enum SoundType
{
    Play,
    Draw,
    Discard,
    Hover
}