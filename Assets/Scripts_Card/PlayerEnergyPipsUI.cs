using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra 8 iconos de color sólido: los primeros N reflejan la energía actual del jugador.
/// Asigna 8 <see cref="Image"/> en el Inspector (o usa pipContainer + pipPrefab).
/// </summary>
public class PlayerEnergyPipsUI : MonoBehaviour
{
    [Tooltip("Ocho imágenes en orden. Si usas pipContainer + pipPrefab, este array se rellena en Awake.")]
    [SerializeField] private Image[] energyPips = new Image[8];

    [Tooltip("Contenedor con HorizontalLayoutGroup; se instancian 8 hijos desde pipPrefab.")]
    [SerializeField] private RectTransform pipContainer;

    [SerializeField] private Image pipPrefab;

    [SerializeField] private Color availableColor = new Color(0.35f, 0.85f, 1f, 1f);

    [SerializeField] private Color spentColor = new Color(0.45f, 0.45f, 0.5f, 1f);

    [Tooltip("Si está activo, cada frame se compara con GameManager.CurrentEnergy por si el evento no llegó (orden de ejecución).")]
    [SerializeField] private bool syncEnergyEveryFrame = true;

    private static Sprite _unitSquareSprite;

    private bool _subscribed;
    private int _lastAppliedEnergy = int.MinValue;

    void Awake()
    {
        EnsureEightPips();
    }

    void OnEnable()
    {
        TrySubscribeAndRefresh();
    }

    void Start()
    {
        TrySubscribeAndRefresh();
    }

    void Update()
    {
        if (!syncEnergyEveryFrame || GameManager.Instance == null || energyPips == null || energyPips.Length != 8)
        {
            return;
        }

        int current = GameManager.Instance.CurrentEnergy;
        if (current != _lastAppliedEnergy)
        {
            ApplyVisual(current, GameManager.MaxEnergyPerTurn);
        }
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void OnDestroy()
    {
        Unsubscribe();
    }

    private void TrySubscribeAndRefresh()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (!_subscribed)
        {
            GameManager.Instance.OnPlayerEnergyChanged += OnEnergyChanged;
            _subscribed = true;
        }

        OnEnergyChanged(GameManager.Instance.CurrentEnergy, GameManager.MaxEnergyPerTurn);
    }

    private void Unsubscribe()
    {
        if (!_subscribed)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerEnergyChanged -= OnEnergyChanged;
        }

        _subscribed = false;
    }

    private void OnEnergyChanged(int current, int max)
    {
        ApplyVisual(current, max);
    }

    private void ApplyVisual(int current, int max)
    {
        if (energyPips == null || energyPips.Length != 8)
        {
            return;
        }

        max = Mathf.Max(1, max);
        current = Mathf.Clamp(current, 0, max);
        _lastAppliedEnergy = current;

        for (int i = 0; i < 8; i++)
        {
            Image img = energyPips[i];
            if (img == null) continue;

            bool lit = i < current;
            img.color = lit ? availableColor : spentColor;
            img.enabled = true;
        }

        RebuildLayoutIfNeeded();
    }

    private void RebuildLayoutIfNeeded()
    {
        RectTransform root = pipContainer;
        if (root == null && energyPips != null && energyPips.Length > 0 && energyPips[0] != null)
        {
            root = energyPips[0].transform.parent as RectTransform;
        }

        if (root != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
        }
    }

    private void EnsureEightPips()
    {
        if (pipContainer != null && pipPrefab != null)
        {
            for (int c = pipContainer.childCount - 1; c >= 0; c--)
            {
                Destroy(pipContainer.GetChild(c).gameObject);
            }

            energyPips = new Image[8];
            for (int i = 0; i < 8; i++)
            {
                Image img = Instantiate(pipPrefab, pipContainer);
                img.gameObject.name = $"EnergyPip_{i}";
                RectTransform rt = img.rectTransform;

                LayoutElement le = img.GetComponent<LayoutElement>();
                if (le == null)
                {
                    le = img.gameObject.AddComponent<LayoutElement>();
                }

                le.minWidth = 28f;
                le.minHeight = 28f;
                le.preferredWidth = 28f;
                le.preferredHeight = 28f;
                le.flexibleWidth = 0f;
                le.flexibleHeight = 0f;

                rt.sizeDelta = new Vector2(28f, 28f);
                energyPips[i] = img;
            }

            ApplySolidSpriteToPips(energyPips);
            RebuildLayoutIfNeeded();
            return;
        }

        if (energyPips == null || energyPips.Length != 8)
        {
            Debug.LogWarning("[PlayerEnergyPipsUI] Asigna 8 Image en energyPips, o bien pipContainer + pipPrefab (Image).", this);
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            if (energyPips[i] == null)
            {
                Debug.LogWarning("[PlayerEnergyPipsUI] Falta una referencia en energyPips.", this);
                return;
            }

            LayoutElement le = energyPips[i].GetComponent<LayoutElement>();
            if (le == null)
            {
                le = energyPips[i].gameObject.AddComponent<LayoutElement>();
            }

            le.minWidth = 28f;
            le.minHeight = 28f;
            le.preferredWidth = 28f;
            le.preferredHeight = 28f;
            le.flexibleWidth = 0f;
            le.flexibleHeight = 0f;
        }

        ApplySolidSpriteToPips(energyPips);
        RebuildLayoutIfNeeded();
    }

    private static void ApplySolidSpriteToPips(Image[] pips)
    {
        Sprite s = GetOrCreateUnitSquareSprite();
        foreach (Image img in pips)
        {
            if (img == null) continue;
            if (img.sprite == null)
            {
                img.sprite = s;
            }

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
