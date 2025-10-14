using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CRTUIEffect : MonoBehaviour
{
    [Header("CRT Effect Settings")]
    [Range(0, 0.3f)] public float scanlineIntensity = 0.1f;
    [Range(0, 0.5f)] public float vignetteIntensity = 0.2f;
    [Range(0, 0.02f)] public float chromaticAberration = 0.005f;
    [Range(0, 0.1f)] public float curvature = 0.02f;
    [Range(0.8f, 1.2f)] public float brightness = 1.0f;
    public Color crtTint = new Color(0.9f, 1.0f, 0.9f, 1.0f);
    
    [Header("Auto-Apply Settings")]
    public bool applyToAllImages = true;
    public bool applyToAllText = true;
    
    private Material crtMaterial;
    private List<Graphic> affectedGraphics = new List<Graphic>();
    
    void Start()
    {
        CreateCRTMaterial();
        ApplyToUIElements();
    }
    
    void Update()
    {
        UpdateMaterialProperties();
    }
    
    void CreateCRTMaterial()
    {
        crtMaterial = new Material(Shader.Find("UI/CRTShader"));
        UpdateMaterialProperties();
    }
    
    void ApplyToUIElements()
    {
        affectedGraphics.Clear();
        
        if (applyToAllImages)
        {
            Image[] images = FindObjectsOfType<Image>();
            foreach (Image image in images)
            {
                if (image.material == null) // No sobreescribir materiales existentes
                {
                    image.material = crtMaterial;
                    affectedGraphics.Add(image);
                }
            }
        }
        
        if (applyToAllText)
        {
            Text[] texts = FindObjectsOfType<Text>();
            foreach (Text text in texts)
            {
                if (text.material == null)
                {
                    text.material = crtMaterial;
                    affectedGraphics.Add(text);
                }
            }
        }
    }
    
    void UpdateMaterialProperties()
    {
        if (crtMaterial != null)
        {
            crtMaterial.SetFloat("_ScanlineIntensity", scanlineIntensity);
            crtMaterial.SetFloat("_VignetteIntensity", vignetteIntensity);
            crtMaterial.SetFloat("_ChromaticAberration", chromaticAberration);
            crtMaterial.SetFloat("_Curvature", curvature);
            crtMaterial.SetFloat("_Brightness", brightness);
            crtMaterial.SetColor("_CRTTint", crtTint);
        }
    }
    
    // Métodos públicos para control
    public void SetScanlineIntensity(float intensity)
    {
        scanlineIntensity = Mathf.Clamp(intensity, 0, 0.3f);
    }
    
    public void SetVignetteIntensity(float intensity)
    {
        vignetteIntensity = Mathf.Clamp(intensity, 0, 0.5f);
    }
    
    public void EnableCRTEffect(bool enable)
    {
        foreach (Graphic graphic in affectedGraphics)
        {
            if (graphic != null)
            {
                graphic.material = enable ? crtMaterial : null;
            }
        }
    }
    
    void OnDestroy()
    {
        // Limpiar materiales
        foreach (Graphic graphic in affectedGraphics)
        {
            if (graphic != null)
            {
                graphic.material = null;
            }
        }
        
        if (crtMaterial != null)
        {
            DestroyImmediate(crtMaterial);
        }
    }
}