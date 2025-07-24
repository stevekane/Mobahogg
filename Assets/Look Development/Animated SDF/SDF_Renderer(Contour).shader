Shader "SDF_Renderer/Contour"
{
  Properties
  {
    _EdgeThreshold ("Edge Threshold (Linear Depth)", Range(0.0001, 0.5)) = 0.05
    [HDR] _CoronaColor ("Corona Color", Color) = (1,1,0,1)
    _CoronaThickness ("Corona Thickness", Range(1, 5)) = 1.0
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
      float _CoronaThickness;
      float4 _CoronaColor;

      TEXTURE2D_X(_SDFDepthTexture);
      SAMPLER(sampler_SDFDepthTexture);
      float4 _SDFDepthTexture_TexelSize;

      TEXTURE2D_X(_SDFMaskTexture);
      SAMPLER(sampler_SDFMaskTexture);
      float4 _SDFMaskTexture_TexelSize;

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
        return output;
      }

      struct FragmentOutput {
        float4 color : SV_TARGET;
        float depth : SV_DEPTH;
      };

      FragmentOutput FullscreenFrag(Varyings input)
      {
        FragmentOutput o;
        // ABSOLUTELY CRITICAL to remap uv-space to texture space.
        input.uv.y = 1-input.uv.y;
        float centerRawDepth = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, input.uv).r;
        float centerMask = SAMPLE_TEXTURE2D_X(_SDFMaskTexture, sampler_SDFMaskTexture, input.uv).r;

        if (centerMask < 0.001)
        {
          discard;
        }

        float linearCenterDepth = LinearEyeDepth(centerRawDepth, _ZBufferParams);
        float2 offset = _SDFDepthTexture_TexelSize.xy * _CoronaThickness;
        bool isEdge = false;

        float raw_depth_up    = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, input.uv + float2(0, offset.y)).r;
        float mask_up         = SAMPLE_TEXTURE2D_X(_SDFMaskTexture, sampler_SDFMaskTexture, input.uv + float2(0, offset.y)).r;
        float linear_depth_up = LinearEyeDepth(raw_depth_up, _ZBufferParams);
        if (abs(linearCenterDepth - linear_depth_up) > _EdgeThreshold && mask_up < 0.001) { isEdge = true; }

        float raw_depth_down  = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, input.uv - float2(0, offset.y)).r;
        float mask_down       = SAMPLE_TEXTURE2D_X(_SDFMaskTexture, sampler_SDFMaskTexture, input.uv - float2(0, offset.y)).r;
        float linear_depth_down = LinearEyeDepth(raw_depth_down, _ZBufferParams);
        if (abs(linearCenterDepth - linear_depth_down) > _EdgeThreshold && mask_down < 0.001) { isEdge = true; }

        float raw_depth_left  = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, input.uv - float2(offset.x, 0)).r;
        float mask_left       = SAMPLE_TEXTURE2D_X(_SDFMaskTexture, sampler_SDFMaskTexture, input.uv - float2(offset.x, 0)).r;
        float linear_depth_left = LinearEyeDepth(raw_depth_left, _ZBufferParams);
        if (abs(linearCenterDepth - linear_depth_left) > _EdgeThreshold && mask_left < 0.001) { isEdge = true; }

        float raw_depth_right = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, input.uv + float2(offset.x, 0)).r;
        float mask_right      = SAMPLE_TEXTURE2D_X(_SDFMaskTexture, sampler_SDFMaskTexture, input.uv + float2(offset.x, 0)).r;
        float linear_depth_right = LinearEyeDepth(raw_depth_right, _ZBufferParams);
        if (abs(linearCenterDepth - linear_depth_right) > _EdgeThreshold && mask_right < 0.001) { isEdge = true; }

        o.color = isEdge ? _CoronaColor : float4(0,0,0,1);
        o.depth = centerRawDepth;
        return o;
      }
      ENDHLSL
    }
  }
}