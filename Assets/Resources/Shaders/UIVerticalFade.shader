Shader "Custom/UIVerticalFade"
{
    Properties
    {
        _ColorBottom ("Color Bottom", Color) = (0, 0.2, 1, 1)
        _ColorTop ("Color Top", Color) = (0, 0.8, 1, 1)
        _FadeWidth ("Fade Width", Range(0, 0.5)) = 0.2
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
            #include "UnityUI.cginc"

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

            float4 _ColorBottom;
            float4 _ColorTop;
            float _FadeWidth;

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
                // Gradient verticale tra i due colori (uv.y = 0 in basso, 1 in alto)
                fixed4 col = lerp(_ColorBottom, _ColorTop, i.uv.y) * i.color;

                // Fade ai bordi superiore e inferiore
                float alpha = smoothstep(0.0, _FadeWidth, i.uv.y)
                            * smoothstep(0.0, _FadeWidth, 1.0 - i.uv.y);

                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}
