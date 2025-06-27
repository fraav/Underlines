using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Game/Card")]
public class CardData : ScriptableObject
{
    public enum CardType { Attack, Block, Heal }
    public CardType cardType;
    public float baseValue;
    public string cardName;
    [TextArea] public string description;
    public Sprite icon;

    // Nuevos campos para mejoras individuales
    public float individualBaseValueUpgrade = 0f;
    public float individualDamageMultiplier = 1.0f;
}