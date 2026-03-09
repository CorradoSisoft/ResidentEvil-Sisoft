Shader "Custom/UIRainbowATB"
{
    Properties
    {
        _Speed ("Rainbow Speed", Range(0, 5)) = 1.0
        _Offset ("Hue Offset", Range(0, 1)) = 0.0
        _FadeWidth ("Fade Width", Range(0, 0.5)) = 0.15
        _Saturation ("Saturation", Range(0, 1)) = 1.0
        _Brightness ("Brightness", Range(0, 1)) = 1.0
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.3
        _AspectRatio ("Aspect Ratio (W/H)", Float) = 6.0
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
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
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            float _Speed;
            float _Offset;
            float _FadeWidth;
            float _Saturation;
            float _Brightness;
            float _CornerRadius;
            float _AspectRatio;

            float3 HSVtoRGB(float h, float s, float v)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(h + K.xyz) * 6.0 - K.www);
                return v * lerp(K.xxx, saturate(p - K.xxx), s);
            }

            // Signed Distance Function per rettangolo arrotondato
            float RoundedBoxSDF(float2 uv, float radius)
            {
                // Porta le UV in spazio -0.5/+0.5
                float2 p = uv - 0.5;
                // Compensa l'aspect ratio per avere angoli circolari e non ovali
                p.x *= _AspectRatio;
                float r = radius * _AspectRatio;

                float2 q = abs(p) - float2(_AspectRatio * 0.5 - r, 0.5 - r);
                return length(max(q, 0.0)) - r;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Angoli arrotondati via SDF
                float dist = RoundedBoxSDF(i.uv, _CornerRadius);
                float cornerAlpha = 1.0 - smoothstep(-0.01, 0.01, dist);

                // Rainbow
                float hue = frac(i.uv.x + _Time.y * _Speed * 0.1 + _Offset);
                float3 rainbow = HSVtoRGB(hue, _Saturation, _Brightness);
                fixed4 col = fixed4(rainbow, 1.0) * i.color;

                col.a *= cornerAlpha;
                return col;
            }
            ENDCG
        }
    }
}