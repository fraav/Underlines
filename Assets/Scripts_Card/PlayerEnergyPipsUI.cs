using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra 8 iconos de energía. Modo manual: coloca cada Image en el Inspector con posición libre.
/// Modo automático: pipContainer + pipPrefab con layout horizontal.
/// </summary>
public class PlayerEnergyPipsUI : MonoBehaviour
{
    [Tooltip("Ocho imágenes en orden (índice 0 = primer punto de energía).")]
    [SerializeField] private Image[] energyPips = new Image[8];

    [Header("Modo de colocación")]
    [Tooltip("Activo: usa las 8 Image ya posicionadas en escena (sin Layout Group).")]
    [SerializeField] private bool useManualPipPositions = true;

    [Tooltip("Solo si useManualPipPositions = false: instancia 8 pips en este contenedor.")]
    [SerializeField] private RectTransform pipContainer;

    [SerializeField] private Image pipPrefab;

    [SerializeField] private Color availableColor = new Color(0.35f, 0.85f, 1f, 1f);
    [SerializeField] private Color spentColor = new Color(0.45f, 0.45f, 0.5f, 1f);

    [SerializeField] private bool syncEnergyEveryFrame = true;

    private static Sprite _unitSquareSprite;
    private bool _subscribed;
    private int _lastAppliedEnergy = int.MinValue;

    void Awake()
    {
        if (useManualPipPositions)
            SetupManualMode();
        else
            EnsureEightPipsFromPrefab();
    }

    void OnEnable() => TrySubscribeAndRefresh();
    void Start() => TrySubscribeAndRefresh();

    void Update()
    {
        if (!syncEnergyEveryFrame || GameManager.Instance == null || energyPips == null || energyPips.Length != 8)
            return;

        int current = GameManager.Instance.CurrentEnergy;
        if (current != _lastAppliedEnergy)
            ApplyVisual(current, GameManager.MaxEnergyPerTurn);
    }

    void OnDisable() => Unsubscribe();
    void OnDestroy() => Unsubscribe();

    private void SetupManualMode()
    {
        if (pipContainer != null)
            DisableLayoutComponents(pipContainer.gameObject);

        if (energyPips == null || energyPips.Length != 8)
        {
            Debug.LogWarning("[PlayerEnergyPipsUI] Modo manual: asigna 8 Image en energyPips.", this);
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            if (energyPips[i] == null)
            {
                Debug.LogWarning($"[PlayerEnergyPipsUI] Falta energyPips[{i}].", this);
                continue;
            }

            DisableLayoutComponents(energyPips[i].gameObject);
        }

        ApplySolidSpriteToPips(energyPips);
    }

    private static void DisableLayoutComponents(GameObject go)
    {
        if (go.GetComponent<HorizontalLayoutGroup>() is HorizontalLayoutGroup hlg)
            hlg.enabled = false;
        if (go.GetComponent<VerticalLayoutGroup>() is VerticalLayoutGroup vlg)
            vlg.enabled = false;
        if (go.GetComponent<ContentSizeFitter>() is ContentSizeFitter csf)
            csf.enabled = false;
        if (go.GetComponent<LayoutElement>() is LayoutElement le)
            le.ignoreLayout = true;
    }

    private void TrySubscribeAndRefresh()
    {
        if (GameManager.Instance == null) return;

        if (!_subscribed)
        {
            GameManager.Instance.OnPlayerEnergyChanged += OnEnergyChanged;
            _subscribed = true;
        }

        OnEnergyChanged(GameManager.Instance.CurrentEnergy, GameManager.MaxEnergyPerTurn);
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerEnergyChanged -= OnEnergyChanged;
        _subscribed = false;
    }

    private void OnEnergyChanged(int current, int max) => ApplyVisual(current, max);

    private void ApplyVisual(int current, int max)
    {
        if (energyPips == null || energyPips.Length != 8) return;

        max = Mathf.Max(1, max);
        current = Mathf.Clamp(current, 0, max);
        _lastAppliedEnergy = current;

        for (int i = 0; i < 8; i++)
        {
            Image img = energyPips[i];
            if (img == null) continue;
            img.color = i < current ? availableColor : spentColor;
            img.enabled = true;
        }
    }

    private void EnsureEightPipsFromPrefab()
    {
        if (pipContainer != null)
            DisableLayoutComponents(pipContainer.gameObject);

        if (pipContainer == null || pipPrefab == null)
        {
            if (energyPips != null && energyPips.Length == 8)
            {
                ApplySolidSpriteToPips(energyPips);
                return;
            }

            Debug.LogWarning("[PlayerEnergyPipsUI] Asigna 8 Image o pipContainer + pipPrefab.", this);
            return;
        }

        for (int c = pipContainer.childCount - 1; c >= 0; c--)
            Destroy(pipContainer.GetChild(c).gameObject);

        energyPips = new Image[8];
        for (int i = 0; i < 8; i++)
        {
            Image img = Instantiate(pipPrefab, pipContainer);
            img.gameObject.name = $"EnergyPip_{i}";
            energyPips[i] = img;
        }

        ApplySolidSpriteToPips(energyPips);
    }

    private static void ApplySolidSpriteToPips(Image[] pips)
    {
        Sprite s = GetOrCreateUnitSquareSprite();
        foreach (Image img in pips)
        {
            if (img == null) continue;
            if (img.sprite == null) img.sprite = s;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.raycastTarget = false;
        }
    }

    private static Sprite GetOrCreateUnitSquareSprite()
    {
        if (_unitSquareSprite != null) return _unitSquareSprite;

        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;

        _unitSquareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
        _unitSquareSprite.name = "PlayerEnergyPips_UnitSquare_Runtime";
        return _unitSquareSprite;
    }
}
