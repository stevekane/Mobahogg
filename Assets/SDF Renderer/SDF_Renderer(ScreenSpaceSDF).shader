Shader "ScreenSpaceSDF/ScreenSpaceSignedDistanceField" {
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "Assets/Shaders/Includes/Full Screen.hlsl"

  TEXTURE2D_X(_Mask);
  SAMPLER(sampler_Mask);
  float4 _Mask_TexelSize;

  struct appdata {
    uint vertexID : SV_VERTEXID;
  };

  struct v2f {
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
  };

  float2 UV_to_ScreenTexel(float2 uv) {
    return floor(uv * _ScreenParams.xy) + 0.5;
  }

  float2 ScreenTexel_to_UV(float2 texel) {
    return (texel + 0.5) / _ScreenParams.xy;
  }

  v2f vert(appdata v) {
    v2f o;
    FullScreenQuadFromVertexIDs(v.vertexID, o.uv, o.positionCS);
    #ifdef UNITY_UV_STARTS_AT_TOP
    o.uv.y = 1.0 - o.uv.y;
    #endif
    return o;
  }
  ENDHLSL

  SubShader {
    Cull Off
    ZWrite Off
    ZTest Always
    Blend One Zero

    Pass {
      Name "Init"
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag_init

      float4 frag_init(v2f i) : SV_Target {
        float mask = SAMPLE_TEXTURE2D_X(_Mask, sampler_Mask, i.uv).r;
        bool inside = mask > 0.5;
        float2 texel = UV_to_ScreenTexel(i.uv);
        return float4(texel, inside ? 1.0 : 0.0, 0.0);
      }
      ENDHLSL
    }

    Pass {
      Name "Step"
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag_jump

      float _Step;
      TEXTURE2D_X(_Source);
      SAMPLER(sampler_Source);

      float4 frag_jump(v2f i) : SV_Target {
        float2 pTexel = UV_to_ScreenTexel(i.uv);
        float4 selfData = SAMPLE_TEXTURE2D_X(_Source, sampler_Source, i.uv);
        float2 bestSeed = selfData.xy;
        bool setSelf = selfData.z > 0.5;
        bool resolvedSelf = selfData.w > 0.5;
        float bestDist = resolvedSelf ? length(bestSeed - pTexel) : 1e20;
        float4 bestData = selfData;

        [unroll]
        for (int oy = -1; oy <= 1; oy++) {
          [unroll]
          for (int ox = -1; ox <= 1; ox++) {
            // optimization i think could remove for now if need to simplify
            if (ox == 0 && oy == 0) continue;
            int2 offset = int2(ox, oy) * (int)_Step;
            float2 qTexel = pTexel + offset;
            float2 qUV = ScreenTexel_to_UV(qTexel);
            float4 qData = SAMPLE_TEXTURE2D_X(_Source, sampler_Source, qUV);
            float2 qSeed = qData.xy;
            bool setQ = qData.z > 0.5;
            bool resolvedQ = qData.w > 0.5;

            if (setQ != setSelf) {
              float dist = length(qTexel - pTexel);
              if (dist < bestDist) {
                bestDist = dist;
                bestData = float4(qTexel, setSelf ? 1.0 : 0.0, 1.0);
              }
            } else if (resolvedQ) {
              float dist = length(qSeed - pTexel);
              if (dist < bestDist) {
                bestDist = dist;
                bestData = float4(qSeed, setSelf ? 1.0 : 0.0, 1.0);
              }
            }
          }
        }

        return bestData;
      }
      ENDHLSL
    }

    Pass {
      Name "Distance"
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag_distance

      TEXTURE2D_X(_Source);
      SAMPLER(sampler_Source);
      float4 _Source_TexelSize;

      float frag_distance(v2f i) : SV_Target {
        float2 pTexel = UV_to_ScreenTexel(i.uv);
        float4 data = SAMPLE_TEXTURE2D_X(_Source, sampler_Source, i.uv);
        float2 closest = data.xy;
        bool inside = data.z > 0.5;
        bool resolved = data.w > 0.5;
        float sign = inside ? 1 : -1;

        float texelDistance = resolved ? length(pTexel - closest) : 0;
        // TODO: Could be computed on CPU. Constant for all pixels and involves sqrt
        float maxPossibleTexelDistance = length(_ScreenSize);
        float uvDistance = saturate(texelDistance / maxPossibleTexelDistance);
        return sign * uvDistance;
      }
      ENDHLSL
    }
  }
}
