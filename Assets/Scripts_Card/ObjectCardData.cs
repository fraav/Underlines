using UnityEngine;

/// <summary>
/// Carta/objeto: mazo y mano separados de CardData. Efecto instantáneo al jugarse (no termina el turno).
/// </summary>
[CreateAssetMenu(fileName = "NewObjectCard", menuName = "Card Game/Object Card")]
public class ObjectCardData : ScriptableObject
{
    public enum ObjectEffectType
    {
        DoubleNextAction,
        Heal,
        RestoreEnergy,
        DrawCard
    }

    [Header("Identidad")]
    public string cardName;
    [TextArea] public string description;
    public Sprite icon;
    public Sprite shopIcon;

    [Header("Efecto")]
    public ObjectEffectType effectType = ObjectEffectType.Heal;

    [Tooltip("Curación (Heal) o valor auxiliar según el efecto.")]
    public float baseValue = 15f;

    [Tooltip("Energía restaurada (RestoreEnergy).")]
    [Min(1)]
    public int restoreEnergyAmount = 2;

    [Tooltip("Cartas robadas del mazo de cartas (DrawCard).")]
    [Min(1)]
    public int drawCardCount = 1;

    [Header("Energy")]
    [Min(0)]
    public int playEnergyCost = 1;

    [Header("Mejoras (mismo patrón que CardData)")]
    public float individualBaseValueUpgrade = 0f;

    [Header("Animación y audio (configurar en editor)")]
    public string customAnimationTrigger = "";
    public float customActionPointTime = 0f;
    public float customAnimationDuration = 0f;
    public AudioClip cardSound;
    public float animationObjectActiveTime = 2f;
    public string nombreObjetoEnEscenaAActivar = "";
    public GameObject animationPrefab;
    public Vector3 prefabSpawnPosition = Vector3.zero;
    public bool attachToTarget = false;

    public GameObject GetObjetoAActivar()
    {
        if (string.IsNullOrEmpty(nombreObjetoEnEscenaAActivar))
            return null;

        GameObject obj = GameObject.Find(nombreObjetoEnEscenaAActivar);
        if (obj == null)
            Debug.LogError($"[ObjectCardData] No se encontró el objeto: {nombreObjetoEnEscenaAActivar}");
        return obj;
    }

    public GameObject GetAnimationPrefab() => animationPrefab;
}
