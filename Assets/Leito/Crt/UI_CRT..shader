Shader "UI/CRTScreenEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // Efectos CRT
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        _ScanlineSpeed ("Scanline Speed", Float) = 2.0
        _ScanlineSize ("Scanline Size", Float) = 2.0
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.4
        _CRTTint ("CRT Tint", Color) = (0.9, 1.0, 0.8, 1.0)
        _FlickerIntensity ("Flicker Intensity", Range(0, 0.2)) = 0.05
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.1)) = 0.03
        _Curvature ("Curvature", Range(0, 0.5)) = 0.15
        _BorderSize ("Border Size", Range(0, 0.3)) = 0.05
        _BorderSmoothness ("Border Smoothness", Range(0.01, 1)) = 0.3
        _Opacity ("Opacity", Range(0, 1)) = 0.1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Overlay"
        }
        
        Cull Off
        ZWrite Off
        ZTest Always
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            // Efectos CRT
            float _ScanlineIntensity;
            float _ScanlineSpeed;
            float _ScanlineSize;
            float _VignetteIntensity;
            fixed4 _CRTTint;
            float _FlickerIntensity;
            float _ChromaticAberration;
            float _Curvature;
            float _BorderSize;
            float _BorderSmoothness;
            float _Opacity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // CURVATURA - Ahora funciona en toda la pantalla
                float2 centeredUV = uv - 0.5;
                float dist = length(centeredUV);
                
                // Distorsión de barril fuerte
                float2 distortedUV = uv;
                if (_Curvature > 0.0)
                {
                    float barrelDistortion = _Curvature * dist * dist;
                    distortedUV = 0.5 + centeredUV * (1.0 - barrelDistortion);
                    
                    // Efecto adicional para hacerlo más visible
                    distortedUV = 0.5 + (distortedUV - 0.5) * (1.0 + _Curvature * 0.3);
                }
                
                // ABERRACIÓN CROMÁTICA - Ahora funciona correctamente
                fixed4 col;
                if (_ChromaticAberration > 0.0)
                {
                    // Desplazamientos direccionales más grandes
                    float2 offset = _ChromaticAberration * centeredUV * dist * 2.0;
                    
                    float r = tex2D(_MainTex, distortedUV + offset * 1.5).r;
                    float g = tex2D(_MainTex, distortedUV + offset * 0.5).g;
                    float b = tex2D(_MainTex, distortedUV - offset * 1.0).b;
                    float a = 1.0;
                    
                    col = fixed4(r, g, b, a);
                }
                else
                {
                    col = tex2D(_MainTex, distortedUV);
                }
                
                // BORDES - Ahora funciona correctamente
                float borderMask = 1.0;
                if (_BorderSize > 0.0)
                {
                    float2 borderUV = abs(distortedUV - 0.5) * 2.0;
                    float borderDist = max(borderUV.x, borderUV.y);
                    borderMask = 1.0 - smoothstep(1.0 - _BorderSize, 1.0 - _BorderSize + _BorderSmoothness, borderDist);
                    
                    // Negro completo fuera de los bordes
                    if (distortedUV.x < 0.0 || distortedUV.x > 1.0 || distortedUV.y < 0.0 || distortedUV.y > 1.0)
                    {
                        borderMask = 0.0;
                    }
                }
                
                // SCANLINES
                float scanline = 1.0;
                if (_ScanlineIntensity > 0.0)
                {
                    float scanlineY = distortedUV.y * _ScreenParams.y / _ScanlineSize;
                    float scanlineWave = sin(scanlineY * 3.14159 + _Time.y * _ScanlineSpeed * 20.0) * 0.5 + 0.5;
                    scanline = lerp(1.0, scanlineWave, _ScanlineIntensity);
                }
                
                // VIÑETA
                float vignette = 1.0;
                if (_VignetteIntensity > 0.0)
                {
                    float2 vignetteUV = distortedUV - 0.5;
                    float vignetteDist = dot(vignetteUV, vignetteUV);
                    vignette = 1.0 - vignetteDist * _VignetteIntensity * 3.0;
                    vignette = saturate(vignette);
                }
                
                // FLICKER
                float flicker = 1.0;
                if (_FlickerIntensity > 0.0)
                {
                    float flickerWave = (sin(_Time.y * 12.0) + sin(_Time.y * 7.0) + sin(_Time.y * 5.0)) / 3.0 * 0.5 + 0.5;
                    flicker = lerp(1.0, flickerWave, _FlickerIntensity);
                }
                
                // APLICAR EFECTOS
                col.rgb *= _CRTTint.rgb;
                col.rgb *= scanline * vignette * flicker * borderMask;
                
                // Control de opacidad general
                col.a = _Opacity;
                
                return col;
            }
            ENDCG
        }
    }
}