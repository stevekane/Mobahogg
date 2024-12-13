Shader "UI/DashedOutline"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Thickness ("Outline Thickness", Float) = 0.01
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Thickness;
            fixed4 _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = abs(2.0 * i.uv - float2(1.0,1.0));
                float a = p.x >= (1.0-_Thickness) || p.y >= (1.0-_Thickness);
                return fixed4(_Color.rgb, a);
            }
            ENDCG
        }
    }
}
