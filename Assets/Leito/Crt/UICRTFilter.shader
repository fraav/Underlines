Shader "UI/CRTFilter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _BloomIntensity ("Bloom Intensity", Range(0, 2)) = 0.5
        _BloomColor ("Bloom Color", Color) = (1,1,1,1)
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.1)) = 0.02
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.3
        _GrainIntensity ("Grain Intensity", Range(0, 1)) = 0.1
        _GrainTex ("Grain Texture", 2D) = "white" {}
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.1
        _ScanlineOffset ("Scanline Offset", Float) = 0
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
            float _BloomIntensity;
            float4 _BloomColor;
            float _ChromaticAberration;
            float _VignetteIntensity;
            float _GrainIntensity;
            sampler2D _GrainTex;
            float _ScanlineIntensity;
            float _ScanlineOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Efecto de aberración cromática
                float2 uvR = i.uv + float2(_ChromaticAberration, _ChromaticAberration);
                float2 uvB = i.uv - float2(_ChromaticAberration, _ChromaticAberration);
                
                fixed4 colR = tex2D(_MainTex, uvR);
                fixed4 colG = tex2D(_MainTex, i.uv);
                fixed4 colB = tex2D(_MainTex, uvB);
                
                fixed4 col = fixed4(colR.r, colG.g, colB.b, colG.a);
                
                // Efecto de bloom
                col.rgb += _BloomColor.rgb * _BloomIntensity;
                
                // Efecto de vignette
                float2 uvCenter = i.uv - 0.5;
                float vignette = 1.0 - dot(uvCenter, uvCenter) * _VignetteIntensity;
                col.rgb *= vignette;
                
                // Efecto de film grain
                if (_GrainIntensity > 0)
                {
                    fixed4 grain = tex2D(_GrainTex, i.uv * 4.0 + _ScanlineOffset);
                    col.rgb += (grain.rgb - 0.5) * _GrainIntensity;
                }
                
                // Efecto de scanlines
                if (_ScanlineIntensity > 0)
                {
                    float scanline = sin((i.uv.y + _ScanlineOffset) * 800) * 0.5 + 0.5;
                    col.rgb *= lerp(1.0, scanline, _ScanlineIntensity);
                }
                
                return col;
            }
            ENDCG
        }
    }
}