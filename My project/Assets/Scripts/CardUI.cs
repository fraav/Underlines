using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    public Image artworkImage;      // La imagen de la carta
    public TMP_Text cardIDText;     // Texto con el ID ("A", "B", etc.)

    private Card cardData;

    public void Setup(Card card)
    {
        cardData = card;
        cardIDText.text = card.cardID;
        artworkImage.sprite = card.artwork;
    }

    public Card GetCard()
    {
        return cardData;
    }
}
