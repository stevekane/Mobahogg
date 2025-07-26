Shader "EuclideanDistance/ScreenSpaceDistanceField" {
  CGINCLUDE
  #include "UnityCG.cginc"
  #include "../Animated SDF/Full Screen Utils.cginc"

  struct appdata {
    uint vertexID : SV_VERTEXID;
  };

  struct v2f {
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
  };

  float _Step;
  float2 _ScreenSize;
  sampler2D _Mask;
  sampler2D _Source;

  float ScreenDistSq(float2 a, float2 b) {
    float2 pixelA = a * _ScreenSize;
    float2 pixelB = b * _ScreenSize;
    float2 delta = pixelA - pixelB;
    return dot(delta, delta);
  }

  bool Seed(float2 uv) {
    float2 mask = tex2D(_Mask, uv);
    return mask.r > 0.5;
  }

  v2f vert(appdata v) {
    v2f o;
    FullScreenQuadFromVertexIDs(v.vertexID, o.uv, o.vertex);
    #ifdef UNITY_UV_STARTS_AT_TOP
    o.uv.y = 1.0 - o.uv.y;
    #endif
    return o;
  }

  half2 frag_init(v2f i) : SV_Target {
    return Seed(i.uv) ? half2(i.uv) : half2(-1, -1);
  }

  half2 frag_jump(v2f i) : SV_Target {
    float2 selfUV = i.uv;
    float2 bestSeed = tex2D(_Source, selfUV).rg;
    float bestDist = (bestSeed.r < 0.0)
    ? 1e20
    : ScreenDistSq(bestSeed, selfUV);

    float2 AbsStep = abs(_Step);
    float2 stepOverScreenSize = AbsStep / _ScreenSize;
    for (int dy = - 1; dy <= 1; dy ++) {
      for (int dx = - 1; dx <= 1; dx ++) {
        float2 offset = stepOverScreenSize * float2(dx, dy);
        float2 sampleUV = selfUV + offset;
        if (any(sampleUV < 0.0) || any(sampleUV > 1.0)) continue;

        half2 candidate = tex2D(_Source, sampleUV).rg;
        if (candidate.x < 0) continue;
        float dist = ScreenDistSq(candidate, selfUV);
        if (dist < bestDist) {
          bestDist = dist;
          bestSeed = candidate;
        }
      }
    }

    // TODO: Move this to finalize pass probably
    if (_Step == -1) {
      bool inside = Seed(selfUV);
      if (inside) {
        return half2(0, 0);
      } else {
        float distPixels = sqrt(bestDist);
        float maxPossibleDistance = length(_ScreenSize);
        float normalizedDist = saturate(distPixels / maxPossibleDistance);
        return half2(normalizedDist, normalizedDist);
      }
    }
    return half2(bestSeed);
  }

  ENDCG

  SubShader {
    Cull Off ZWrite Off ZTest Always
    Blend One Zero

    Pass {
      Name "Init"
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag_init
      ENDCG
    }

    Pass {
      Name "Jump"
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag_jump
      ENDCG
    }
  }
}