Shader "Grass/InstancedIndirectShader"
{
  Properties
  {
    _Color ("Color", Color) = (1, 1, 1, 1)
  }
  SubShader
  {
    Pass
    {
      Tags {
        "RenderType" = "Opaque"
        "RenderPipeline" = "UniversalPipeline"
      }
      HLSLPROGRAM
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_instancing
      #pragma instancing_options renderingLayer

      StructuredBuffer<float4x4> _Matrices;
      float4 _Color;

      struct appdata
      {
        float4 vertex : POSITION;
        uint instanceID : SV_InstanceID;
      };

      struct v2f
      {
        float4 pos : SV_POSITION;
      };

      v2f vert(appdata v, uint svInstanceID : SV_INSTANCEID)
      {
        v2f o;
        float4x4 modelMatrix = _Matrices[svInstanceID];
        o.pos = mul(modelMatrix, v.vertex);
        o.pos = mul(UNITY_MATRIX_VP, o.pos);
        return o;
      }

      float4 frag(v2f i) : SV_Target
      {
        return _Color;
      }
      ENDHLSL
    }
  }
}
