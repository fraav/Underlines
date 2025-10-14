using UnityEngine;
using UnityEngine.UI;

public class CRTManager : MonoBehaviour
{
    [Header("CRT Screen Effect - Screen Space")]
    public Material crtMaterial;
    
    [Header("Animation Settings")]
    [Range(0, 1f)] public float scanlineIntensity = 0.3f;
    [Range(0, 5f)] public float scanlineSpeed = 2.0f;
    [Range(1, 10f)] public float scanlineSize = 2.0f;
    [Range(0, 1f)] public float vignetteIntensity = 0.4f;
    [Range(0, 0.2f)] public float flickerIntensity = 0.05f;
    [Range(0, 1f)] public float overlayOpacity = 0.1f;
    public Color crtTint = new Color(0.9f, 1.0f, 0.8f, 1f);
    
    [Header("Screen Space Effects")]
    [Range(0, 0.1f)] public float chromaticAberration = 0.03f;
    [Range(0, 0.5f)] public float curvature = 0.15f;
    [Range(0, 0.3f)] public float borderSize = 0.05f;
    [Range(0.01f, 1f)] public float borderSmoothness = 0.3f;
    
    private Camera mainCamera;
    private bool effectsEnabled = false;
    
    void Start()
    {
        mainCamera = Camera.main;
        SetupScreenEffect();
        UpdateEffects();
    }
    
    void Update()
    {
        UpdateEffects();
    }
    
    void SetupScreenEffect()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No se encontró la cámara principal!");
                return;
            }
        }
        
        // Crear o obtener el material del shader
        if (crtMaterial == null)
        {
            Shader crtShader = Shader.Find("UI/CRTScreenEffect");
            if (crtShader != null)
            {
                crtMaterial = new Material(crtShader);
            }
            else
            {
                Debug.LogError("Shader UI/CRTScreenEffect no encontrado!");
                return;
            }
        }
        
        // Aplicar el efecto a la cámara
        if (!mainCamera.GetComponent<CRTEffect>())
        {
            var crtEffect = mainCamera.gameObject.AddComponent<CRTEffect>();
            crtEffect.crtMaterial = crtMaterial;
        }
        
        effectsEnabled = true;
    }
    
    void UpdateEffects()
    {
        if (crtMaterial == null || !effectsEnabled) return;
        
        // Efectos base
        crtMaterial.SetFloat("_ScanlineIntensity", scanlineIntensity);
        crtMaterial.SetFloat("_ScanlineSpeed", scanlineSpeed);
        crtMaterial.SetFloat("_ScanlineSize", scanlineSize);
        crtMaterial.SetFloat("_VignetteIntensity", vignetteIntensity);
        crtMaterial.SetFloat("_FlickerIntensity", flickerIntensity);
        crtMaterial.SetColor("_CRTTint", crtTint);
        
        // Efectos de screen space
        crtMaterial.SetFloat("_ChromaticAberration", chromaticAberration);
        crtMaterial.SetFloat("_Curvature", curvature);
        crtMaterial.SetFloat("_BorderSize", borderSize);
        crtMaterial.SetFloat("_BorderSmoothness", borderSmoothness);
        crtMaterial.SetFloat("_Opacity", overlayOpacity);
    }
    
    [ContextMenu("Enable Full CRT Effect - SCREEN SPACE")]
    public void EnableFullCRTEffect()
    {
        scanlineIntensity = 0.5f;
        scanlineSpeed = 3.0f;
        scanlineSize = 1.5f;
        vignetteIntensity = 0.7f;
        flickerIntensity = 0.08f;
        overlayOpacity = 0.15f;
        crtTint = new Color(0.8f, 1.0f, 0.7f, 1f);
        
        // Efectos de screen space muy visibles
        chromaticAberration = 0.05f;
        curvature = 0.25f;
        borderSize = 0.08f;
        borderSmoothness = 0.2f;
        
        UpdateEffects();
        Debug.Log("Efecto CRT Screen Space activado!");
    }
    
    [ContextMenu("TEST Curvatura Máxima")]
    public void TestMaxCurvature()
    {
        curvature = 0.4f;
        chromaticAberration = 0.06f;
        UpdateEffects();
        Debug.Log("Curvatura máxima - debería verse como lente de ojo de pez");
    }
    
    [ContextMenu("TEST Aberración Máxima")]
    public void TestMaxChromatic()
    {
        chromaticAberration = 0.08f;
        curvature = 0.1f;
        UpdateEffects();
        Debug.Log("Aberración máxima - deberían verse colores separados en bordes");
    }
    
    [ContextMenu("Disable CRT Effects")]
    public void DisableCRTEffects()
    {
        overlayOpacity = 0f;
        UpdateEffects();
        Debug.Log("Efectos CRT desactivados (opacidad a 0)");
    }
    
    [ContextMenu("Enable CRT Effects")]
    public void EnableCRTEffects()
    {
        overlayOpacity = 0.15f;
        UpdateEffects();
        Debug.Log("Efectos CRT activados");
    }
    
    // Métodos públicos para control desde otros scripts
    public void SetCurvature(float curve)
    {
        curvature = Mathf.Clamp(curve, 0, 0.5f);
        UpdateEffects();
    }
    
    public void SetChromaticAberration(float chroma)
    {
        chromaticAberration = Mathf.Clamp(chroma, 0, 0.1f);
        UpdateEffects();
    }
    
    public void SetOpacity(float opacity)
    {
        overlayOpacity = Mathf.Clamp01(opacity);
        UpdateEffects();
    }
    
    void OnDestroy()
    {
        // Limpiar el efecto de la cámara
        if (mainCamera != null)
        {
            var crtEffect = mainCamera.GetComponent<CRTEffect>();
            if (crtEffect != null)
            {
                Destroy(crtEffect);
            }
        }
    }
}

// Componente auxiliar para aplicar el efecto a la cámara
public class CRTEffect : MonoBehaviour
{
    public Material crtMaterial;
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (crtMaterial != null)
        {
            Graphics.Blit(source, destination, crtMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}