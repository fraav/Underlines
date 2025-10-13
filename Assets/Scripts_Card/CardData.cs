using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Game/Card")]
public class CardData : ScriptableObject
{
    public enum CardType { Attack, Block, Heal, Assistance }
    public enum AssistanceType { DoubleEffect, ExtraDamage, ExtraBlock, ExtraHeal }
    public CardType cardType;
    public float baseValue;
    public string cardName;
    [TextArea] public string description;

    // Sprite para uso en el juego
    public Sprite icon;

    // Sprite espec�fico para la tienda
    public Sprite shopIcon;

    // Campos para mejoras individuales
    public float individualBaseValueUpgrade = 0f;
    public float individualDamageMultiplier = 1.0f;
    
    [Header("Assistance Card Settings")]
    public AssistanceType assistanceType = AssistanceType.DoubleEffect;
    public float effectMultiplier = 1.0f;
    public Color cardColor = Color.white;
    public Color textColor = Color.black;
}