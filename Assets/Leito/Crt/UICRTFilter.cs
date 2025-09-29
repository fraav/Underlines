using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UICRTFilter : MonoBehaviour
{
    [Header("CRT Filter Settings")]
    public Volume crtVolume;
    public RawImage crtOverlay;
    public bool enableCRTEffects = true;
    
    [Header("Bloom Effect (Simplificado)")]
    public bool enableBloom = true;
    [Range(0, 2)] public float bloomIntensity = 0.5f;
    public Color bloomColor = Color.white;
    
    [Header("Chromatic Aberration")]
    public bool enableChromaticAberration = true;
    [Range(0, 0.1f)] public float chromaticIntensity = 0.02f;
    
    [Header("Vignette")]
    public bool enableVignette = true;
    [Range(0, 1)] public float vignetteIntensity = 0.3f;
    
    [Header("Film Grain")]
    public bool enableFilmGrain = true;
    [Range(0, 1)] public float grainIntensity = 0.1f;
    public Texture2D grainTexture;
    
    [Header("Scanlines")]
    public bool enableScanlines = true;
    [Range(0, 1)] public float scanlineIntensity = 0.1f;
    public float scanlineSpeed = 1f;
    
    // Material para efectos CRT
    private Material crtMaterial;
    private RenderTexture renderTexture;
    private Camera uiCamera;
    
    void Start()
    {
        CreateCRTMaterial();
        SetupUICamera();
        
        if (crtVolume != null)
        {
            SyncWithVolume();
        }
    }
    
    void Update()
    {
        UpdateCRTEffects();
    }
    
    void CreateCRTMaterial()
    {
        // Crear un shader simple para efectos CRT
        Shader crtShader = Shader.Find("UI/Default");
        crtMaterial = new Material(crtShader);
        
        // Si tenemos un RawImage para overlay, configurarlo
        if (crtOverlay != null)
        {
            crtOverlay.material = crtMaterial;
        }
    }
    
    void SetupUICamera()
    {
        // Crear una cámara para renderizar la UI con efectos
        GameObject cameraObj = new GameObject("UICRTCamera");
        uiCamera = cameraObj.AddComponent<Camera>();
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.cullingMask = LayerMask.GetMask("UI");
        uiCamera.orthographic = true;
        uiCamera.depth = Camera.main.depth + 1;
        
        // Crear Render Texture
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        renderTexture.antiAliasing = 4;
        uiCamera.targetTexture = renderTexture;
        
        // Configurar para URP
        var additionalData = uiCamera.GetUniversalAdditionalCameraData();
        additionalData.renderType = CameraRenderType.Overlay;
        
        // Añadir a la cámara principal
        var mainCameraData = Camera.main.GetUniversalAdditionalCameraData();
        mainCameraData.cameraStack.Add(uiCamera);
    }
    
    void SyncWithVolume()
    {
        if (crtVolume == null || crtVolume.profile == null) return;
        
        // Sincronizar con los efectos del Volume
        Bloom volumeBloom;
        ChromaticAberration volumeChromatic;
        Vignette volumeVignette;
        FilmGrain volumeGrain;
        PaniniProjection volumePanini;
        LensDistortion volumeLens;
        
        if (crtVolume.profile.TryGet(out volumeBloom) && volumeBloom.active)
        {
            enableBloom = true;
            bloomIntensity = volumeBloom.intensity.value * 0.5f;
        }
        
        if (crtVolume.profile.TryGet(out volumeChromatic) && volumeChromatic.active)
        {
            enableChromaticAberration = true;
            chromaticIntensity = volumeChromatic.intensity.value * 0.1f;
        }
        
        if (crtVolume.profile.TryGet(out volumeVignette) && volumeVignette.active)
        {
            enableVignette = true;
            vignetteIntensity = volumeVignette.intensity.value;
        }
        
        if (crtVolume.profile.TryGet(out volumeGrain) && volumeGrain.active)
        {
            enableFilmGrain = true;
            grainIntensity = volumeGrain.intensity.value;
        }
    }
    
    void UpdateCRTEffects()
    {
        if (crtMaterial == null) return;
        
        // Aplicar efectos al material
        ApplyBloomEffect();
        ApplyChromaticAberration();
        ApplyVignette();
        ApplyFilmGrain();
        ApplyScanlines();
    }
    
    void ApplyBloomEffect()
    {
        if (!enableBloom) return;
        
        // Para bloom en UI, usaríamos un shader con glow
        // Por ahora, aplicamos un brillo simple
        crtMaterial.SetFloat("_BloomIntensity", bloomIntensity);
        crtMaterial.SetColor("_BloomColor", bloomColor);
    }
    
    void ApplyChromaticAberration()
    {
        if (!enableChromaticAberration) return;
        
        crtMaterial.SetFloat("_ChromaticAberration", chromaticIntensity);
        
        // Aplicar a todos los elementos UI directamente
        ApplyChromaticToUIElements();
    }
    
    void ApplyVignette()
    {
        if (!enableVignette) return;
        
        crtMaterial.SetFloat("_VignetteIntensity", vignetteIntensity);
    }
    
    void ApplyFilmGrain()
    {
        if (!enableFilmGrain) return;
        
        crtMaterial.SetFloat("_GrainIntensity", grainIntensity);
        if (grainTexture != null)
        {
            crtMaterial.SetTexture("_GrainTex", grainTexture);
        }
    }
    
    void ApplyScanlines()
    {
        if (!enableScanlines) return;
        
        float scanlineOffset = Time.time * scanlineSpeed;
        crtMaterial.SetFloat("_ScanlineIntensity", scanlineIntensity);
        crtMaterial.SetFloat("_ScanlineOffset", scanlineOffset);
    }
    
    void ApplyChromaticToUIElements()
    {
        // Aplicar aberración cromática sutil a elementos UI específicos
        // Esto es opcional y puede desactivarse si causa problemas
        if (chromaticIntensity > 0.01f)
        {
            // Podrías aplicar desplazamientos sutiles a elementos específicos aquí
        }
    }
    
    // Método alternativo: Overlay de pantalla completa
    void CreateFullScreenOverlay()
    {
        if (crtOverlay == null)
        {
            GameObject overlayObj = new GameObject("CRTOverlay");
            crtOverlay = overlayObj.AddComponent<RawImage>();
            crtOverlay.transform.SetParent(GetComponent<Canvas>().transform, false);
            crtOverlay.rectTransform.anchorMin = Vector2.zero;
            crtOverlay.rectTransform.anchorMax = Vector2.one;
            crtOverlay.rectTransform.offsetMin = Vector2.zero;
            crtOverlay.rectTransform.offsetMax = Vector2.zero;
            crtOverlay.material = crtMaterial;
        }
    }
    
    // Métodos públicos para control
    public void SetBloomIntensity(float intensity)
    {
        bloomIntensity = Mathf.Clamp(intensity, 0, 2);
    }
    
    public void SetChromaticIntensity(float intensity)
    {
        chromaticIntensity = Mathf.Clamp(intensity, 0, 0.1f);
    }
    
    public void SetVignetteIntensity(float intensity)
    {
        vignetteIntensity = Mathf.Clamp01(intensity);
    }
    
    public void ToggleCRTEffects(bool enabled)
    {
        enableCRTEffects = enabled;
        if (crtOverlay != null)
        {
            crtOverlay.gameObject.SetActive(enabled);
        }
    }
    
    void OnDestroy()
    {
        // Limpiar recursos
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
        
        if (uiCamera != null)
        {
            Destroy(uiCamera.gameObject);
        }
    }
}