Shader "SDF_Renderer/Contour"
{
  Properties
  {
    _EdgeThreshold ("Edge Threshold (Linear Depth)", Range(0.0001, 0.5)) = 0.05
    _HorizonThickness ("Horizon Thickness", Range(0.1, 20)) = 1.0
    _CoronaThickness ("Corona Thickness", Range(1.0, 100)) = 5.0
    [HDR] _CoronaColor ("Corona Color", Color) = (1, 1, 0, 1)
    [HDR] _EinsteinRingColor ("EinsteinRing Color", Color) = (1, 1, 0, 1)
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
      float4 _EinsteinRingColor;
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

      FragmentOutput FullscreenFrag(Varyings input)
      {
        const float4 PURE_BLACK = float4(0, 0, 0, 1);
        FragmentOutput o;
        float depth = SAMPLE_TEXTURE2D_X(_SDFDepthTexture, sampler_SDFDepthTexture, input.uv).r;
        float4 c = SAMPLE_TEXTURE2D_X(_ColorTexture, sampler_ColorTexture, input.uv);
        float d = SAMPLE_TEXTURE2D_X(_ScreenSpaceSDFTexture, sampler_ScreenSpaceSDFTexture, input.uv).r;

        if (d < 0)
          discard;

        float EinsteinRingThickness = .0008;
        float PhotonSphereMin = .008;
        float PhotonSpheremax = .009;
        bool EinsteinRing = d <= EinsteinRingThickness;
        bool PhotonSphere = d >= PhotonSphereMin && d <= PhotonSpheremax;
        bool WarpedBackground = d > EinsteinRingThickness && d < PhotonSphereMin;
        if (EinsteinRing) {
          o.color = _EinsteinRingColor;
        } else if (PhotonSphere) {
          o.color = _CoronaColor;
        } else if (WarpedBackground) {
          o.color = lerp(c, float4(1,1,1,1), .1);
        } else {
          o.color = PURE_BLACK;
        }
        o.depth = depth;
        return o;
      }
      ENDHLSL
    }
  }
}
