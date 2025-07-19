Shader "Custom/ColorSuction"
{
  Properties
  {
    _MainTex("Texture", 2D) = "white" {}
  }
  SubShader
  {
    Pass
    {
      Name "Color Suction"
      ZTest False
      ZWrite Off
      Cull Off

      HLSLPROGRAM
      #pragma vertex Vert
      #pragma fragment Frag
      #pragma target 4.5
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

      struct Attributes {
        float4 positionOS : POSITION;
        float2 uv         : TEXCOORD0;
      };

      struct Varyings {
        float4 positionCS   : SV_POSITION;
        float2 uv           : TEXCOORD0;
      };

      Varyings Vert (Attributes IN) {
        Varyings OUT;
        OUT.positionCS = TransformObjectToHClip(IN.positionOS);
        OUT.uv = IN.uv;
        return OUT;
      }

      sampler2D _MainTex;
      float4 Frag(Varyings IN) : SV_Target {
        // float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
        // return float4(c.r, c.b, c.g, c.a);
        return float4(0, 1, 1, 1);
      }
      ENDHLSL
    }
  }
  FallBack "Hidden/InternalErrorShader"
}