Shader "SDF_Renderer/Contour"
{
  Properties
  {
    _EdgeThreshold ("Edge Threshold (Linear Depth)", Range(0.0001, 0.5)) = 0.05
    _HorizonThickness ("Horizon Thickness", Range(0.1, 20)) = 1.0
    _CoronaThickness ("Corona Thickness", Range(1.0, 100)) = 5.0
    [HDR] _CoronaColor ("Corona Color", Color) = (1, 1, 0, 1)
    _DistortionStrength ("Distortion Strength", Range(0.001, 1)) = 0.01
  }

  SubShader
  {
    Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
    LOD 100

    Pass
    {
      Name "SDFContourPass"
      Cull Off
      ZWrite On
      ZTest LEqual
      Blend Off

      HLSLPROGRAM
      #pragma vertex FullscreenVert
      #pragma fragment FullscreenFrag

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Full Screen Utils.cginc"

      float _EdgeThreshold;
      float _HorizonThickness;
      float _CoronaThickness;
      float4 _CoronaColor;
      float _DistortionStrength;

      TEXTURE2D_X(_SDFDepthTexture);
      SAMPLER(sampler_SDFDepthTexture);
      float4 _SDFDepthTexture_TexelSize;

      TEXTURE2D_X(_SDFMaskTexture);
      SAMPLER(sampler_SDFMaskTexture);
      float4 _SDFMaskTexture_TexelSize;

      TEXTURE2D_X(_ScreenSpaceSDFTexture);
      SAMPLER(sampler_ScreenSpaceSDFTexture);
      float4 _ScreenSpaceSDFTexture_TexelSize;

      TEXTURE2D_X(_ColorTexture);
      SAMPLER(sampler_ColorTexture);
      float4 _ColorTexture_TexelSize;

      struct Attributes
      {
        uint vertexID : SV_VertexID;
      };

      struct Varyings
      {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      Varyings FullscreenVert(Attributes input)
      {
        Varyings output;
        FullScreenQuadFromVertexIDs(input.vertexID, output.uv, output.positionCS);
        // https://gist.github.com/CianNoonan/c56256433801991038c9c40a48fe3002#file-hiddenjumpfloodoutline-shader-L78
        #ifdef UNITY_UV_STARTS_AT_TOP
        output.uv.y = 1.0 - output.uv.y;
        #endif
        return output;
      }

      struct FragmentOutput {
        float4 color : SV_TARGET;
        float depth : SV_DEPTH;
      };

      float2 CoronaDistortionAlongContour(float2 uv, float4 texelSize, float strength)
      {
        float rawC = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, uv).r;
        float rawL = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, uv - float2(texelSize.x, 0)).r;
        float rawR = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, uv + float2(texelSize.x, 0)).r;
        float rawU = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, uv + float2(0, texelSize.y)).r;
        float rawD = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, uv - float2(0, texelSize.y)).r;

        float dx = rawR - rawL;
        float dy = rawU - rawD;
        float2 normal = normalize(float2(dx, dy));
        float2 tangent = float2(-normal.y, normal.x);
        return uv + tangent * strength;
      }

      float EstimateEdgeDistance(float2 uv, float texelStep)
      {
        float minDist = 9999.0;
        for (int y = -2; y <= 2; y++)
        {
          for (int x = -2; x <= 2; x++)
          {
            float2 offset = float2(x, y) * texelStep;
            float maskSample = SAMPLE_TEXTURE2D_X(_SDFMaskTexture, sampler_SDFMaskTexture, uv + offset).r;
            if (maskSample < 0.001)
            {
              float dist = length(offset);
              minDist = min(minDist, dist);
            }
          }
        }
        return minDist;
      }

      FragmentOutput FullscreenFrag(Varyings input)
      {
        const float4 PURE_BLACK = float4(0, 0, 0, 1);
        FragmentOutput o;

        float centerDepth = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, input.uv).r;
        float4 sceneColor = SAMPLE_TEXTURE2D_X(_ColorTexture, sampler_ColorTexture, input.uv);
        half ssDistance = SAMPLE_TEXTURE2D_X(_ScreenSpaceSDFTexture, sampler_ScreenSpaceSDFTexture, input.uv).r;
        ssDistance *= 10;
        float4 distanceColor = float4(ssDistance, ssDistance, ssDistance, 1);

        o.color = lerp(distanceColor, sceneColor, ssDistance);
        o.color = distanceColor;
        // o.depth = centerDepth;
        o.depth = 1;
        return o;
      }
      ENDHLSL
    }
  }
}
