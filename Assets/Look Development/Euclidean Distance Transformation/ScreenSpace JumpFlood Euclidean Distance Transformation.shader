Shader "ScreenSpaceSDF/JumpFloodEuclideanDistanceTransformation" {
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "../Animated SDF/Full Screen Utils.cginc"

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

  float ScreenDistSq(float2 a, float2 b) {
    float2 delta = a - b;
    return dot(delta, delta);
  }


  float2 UV_to_ScreenTexel(float2 uv) {
    return floor(uv * _ScreenParams.xy) + 0.5;
  }

  float2 ScreenTexel_to_UV(float2 texel) {
    return (texel + 0.5) / _ScreenParams.xy;
  }

  float2 FetchTexelFromMask(float2 texel) {
    return SAMPLE_TEXTURE2D_X(_Mask, sampler_Mask, ScreenTexel_to_UV(texel)).rg;
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

      float2 frag_init(v2f i) : SV_Target {
        float2 sample = SAMPLE_TEXTURE2D_X(_Mask, sampler_Mask, i.uv);
        return sample.r > 0.5
        ? UV_to_ScreenTexel(i.uv)
        : float2(-1, -1);
      }
      ENDHLSL
    }

    Pass {
      Name "Jump"
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag_jump

      float _Step;
      TEXTURE2D_X(_Source);
      SAMPLER(sampler_Source);
      float4 _Source_TexelSize;

      float2 FetchTexelFromSource(float2 texel) {
        return SAMPLE_TEXTURE2D_X(_Source, sampler_Source, ScreenTexel_to_UV(texel)).rg;
      }

      float2 frag_jump(v2f i) : SV_Target {
        float2 AbsStep = abs(_Step);
        float2 pTexel = UV_to_ScreenTexel(i.uv);
        float2 p = FetchTexelFromSource(pTexel);
        float2 bestSeed = p;
        float bestDist = p.r < 0.0
        ? 1e20
        : ScreenDistSq(p, pTexel);
        [unroll]
        for (int dy = - 1; dy <= 1; dy ++) {
          [unroll]
          for (int dx = - 1; dx <= 1; dx ++) {
            if (dx == 0 && dy == 0) continue;
            float2 qTexel = pTexel + AbsStep * float2(dx, dy);
            float2 q = FetchTexelFromSource(qTexel);

            if (q.x < 0) continue;

            float qDist = ScreenDistSq(q, pTexel);
            if (qDist < bestDist) {
              bestDist = qDist;
              bestSeed = q;
            }
          }
        }

        if (_Step == - 1) {
          bool inside = FetchTexelFromMask(pTexel) > 0.5;
          if (inside) {
            return float2(0, 0);
          } else {
            float distPixels = sqrt(bestDist);
            float maxPossibleDistance = length(_ScreenParams.xy);
            float normalizedDist = saturate(distPixels / maxPossibleDistance);
            return float2(normalizedDist, normalizedDist);
          }
        }
        return bestSeed;
      }
      ENDHLSL
    }
  }
}