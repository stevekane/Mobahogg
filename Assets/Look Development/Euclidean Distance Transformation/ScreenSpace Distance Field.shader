Shader "EuclideanDistance/ScreenSpaceDistanceField" {
  Properties {
    _Mask ("Texture", 2D) = "white" {}
  }

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
    float2 center = float2(0.5, 0.5);
    float radius = 0.25;
    float dist = distance(uv, center);
    return dist < radius;
  }

  v2f vert(appdata v) {
    v2f o;
    FullScreenQuadFromVertexIDs(v.vertexID, o.uv, o.vertex);
    #ifdef UNITY_UV_STARTS_AT_TOP
    o.uv.y = 1.0 - o.uv.y;
    #endif
    return o;
  }

  float4 frag_init(v2f i) : SV_Target {
    return Seed(i.uv) ? float4(i.uv, 0, 1) : float4(- 1, - 1, - 1, 1);
  }

  float4 frag_jump(v2f i) : SV_Target {
    float2 selfUV = i.uv;
    float2 bestSeed = tex2D(_Source, selfUV).rg;
    float bestDist = (bestSeed.r < 0.0)
    ? 1e20
    : ScreenDistSq(bestSeed, selfUV);

    float2 stepOverScreenSize = _Step / _ScreenSize;
    for (int dy = - 1; dy <= 1; dy ++) {
      for (int dx = - 1; dx <= 1; dx ++) {
        if (dx == 0 && dy == 0) continue;

        float2 offset = stepOverScreenSize * float2(dx, dy);
        float2 sampleUV = selfUV + offset;
        if (any(sampleUV < 0.0) || any(sampleUV > 1.0)) continue;

        float4 candidate = tex2D(_Source, sampleUV);
        if (candidate.x < 0) continue;

        float2 seedUV = candidate.rg;
        float dist = ScreenDistSq(seedUV, selfUV);
        if (dist < bestDist) {
          bestDist = dist;
          bestSeed = seedUV;
        }
      }
    }

    if (abs(_Step - 1.0) < 0.001) {
      bool inside = Seed(selfUV);
      if (inside) {
          return float4(0, 0, 0, 1);
      } else {
          float distPixels = sqrt(bestDist);
          float maxPossibleDistance = length(_ScreenSize);
          float normalizedDist = saturate(distPixels / maxPossibleDistance);
          return float4(normalizedDist, normalizedDist, normalizedDist, 1);
      }
    }

    return float4(bestSeed, 0, 1);
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