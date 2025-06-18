using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class Card : ScriptableObject
{
    public string cardID;         // Por ejemplo: "A", "B", etc.
    public Sprite artwork;        // Imagen de la carta
    public string description;    // Opcional
}
