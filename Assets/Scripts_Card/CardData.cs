using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Game/Card")]
public class CardData : ScriptableObject
{
    public enum CardType { Attack, Block, Heal }
    public CardType cardType;
    public float baseValue;
    public string cardName;
    [TextArea] public string description;

    // Sprite para uso en el juego
    public Sprite icon;

    // Sprite específico para la tienda
    public Sprite shopIcon;

    // Campos para mejoras individuales
    public float individualBaseValueUpgrade = 0f;
    public float individualDamageMultiplier = 1.0f;
}