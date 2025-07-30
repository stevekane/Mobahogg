Shader "SDF_Renderer/Render"
{
  Properties
  {
    [HDR] _PhotonSphereColor ("PhotonSphere Color", Color) = (1, 1, 0, 1)
    [HDR] _EinsteinRingColor ("EinsteinRing Color", Color) = (1, 1, 0, 1)
    [HDR] _InnerRegionTint ("InnerRegion Tint", Color) = (.1, .1, .1, 1)
    _PhotonSphereMin ("PhotonSphere Min", Range(.0001, 1)) = .008
    _PhotonSphereThickness ("PhotonSphere Thickness", Range(.0001, .01)) = .001
    _EinsteinRingThickness ("Einstein Ring Thickness", Range(.0001, .01)) = .001
    _RadialDistortionAngle ("Radial Distortion Angle", Range(0.0, 360.0)) = 30
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
      #include "Assets/Shaders/Includes/Full Screen.hlsl"
      #include "Assets/Shaders/Includes/Noise.hlsl"

      float4 _PhotonSphereColor;
      float4 _EinsteinRingColor;
      float4 _InnerRegionTint;
      float _PhotonSphereMin;
      float _PhotonSphereThickness;
      float _EinsteinRingThickness;
      float _RadialDistortionAngle;

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

      float2 ComputeSDFGradient(
      float2 uv_current,
      TEXTURE2D_X(sdfTexture),
      SAMPLER(sampler_sdfTexture),
      float4 sdfTexture_TexelSize,
      float2 screenSize_pixels
      ) {
        float maxPossibleDistance_pixels = length(screenSize_pixels);
        float d = SAMPLE_TEXTURE2D_X(sdfTexture, sampler_sdfTexture, uv_current).r * maxPossibleDistance_pixels;
        float fx = ddx(d);
        float fy = ddy(d);
        return float2(fx, fy);
      }

      float ComputeSDFCurvature(
      float2 uv_current,
      float2 gradient) {
        float fx = gradient.x;
        float fy = gradient.y;

        float fxx2 = ddx(fx);
        float fyy2 = ddy(fy);
        float fxy2 = ddy(fx);

        float gradMagSq2 = fx * fx + fy * fy;
        if (gradMagSq2 < 0.00001) return 0.0;

        float numerator = fxx2 * fy * fy - 2 * fx * fy * fxy2 + fyy2 * fx * fx;
        float denominator = pow(gradMagSq2, 1.5);
        return numerator / denominator;
      }

      float ComputeRadiusOfCurvature(
      float2 uv_current,
      float2 gradient) {
        float kappa = ComputeSDFCurvature(uv_current, gradient);
        if (abs(kappa) < 0.00001) return 1e10;
        return 1.0 / abs(kappa);
      }

      float2 ComputeRadiallyOffsetSampleUV(
      float2 uv_current,
      TEXTURE2D_X(sdfTexture),
      SAMPLER(sampler_sdfTexture),
      float4 sdfTexture_TexelSize,
      float distortionAngleDegrees,
      float maxRadius_pixels,
      float2 screenSize_pixels) {
        float2 gradient_pixel_deriv = ComputeSDFGradient(
        uv_current,
        sdfTexture,
        sampler_sdfTexture,
        sdfTexture_TexelSize,
        screenSize_pixels);
        float gradMag_pixel_deriv = length(gradient_pixel_deriv);
        float radius_pixels = ComputeRadiusOfCurvature(
        uv_current,
        gradient_pixel_deriv);
        radius_pixels = min(radius_pixels, maxRadius_pixels);
        float2 currentPixelCoord_texel = uv_current * screenSize_pixels;
        float2 centerPixelCoord_texel = currentPixelCoord_texel;

        if (gradMag_pixel_deriv >= 0.00001) {
          float2 normal_pixel_deriv = - gradient_pixel_deriv / gradMag_pixel_deriv;
          centerPixelCoord_texel = currentPixelCoord_texel + normal_pixel_deriv * radius_pixels;
        }

        float2 vecToPixel_texel = currentPixelCoord_texel - centerPixelCoord_texel;
        float currentRadiusFromCenter_pixels = length(vecToPixel_texel);

        vecToPixel_texel /= currentRadiusFromCenter_pixels;

        float distortionAngle_radians = radians(distortionAngleDegrees);
        float cosAngle = cos(distortionAngle_radians);
        float sinAngle = sin(distortionAngle_radians);
        float2 rotatedVec_texel = float2(
        vecToPixel_texel.x * cosAngle - vecToPixel_texel.y * sinAngle,
        vecToPixel_texel.x * sinAngle + vecToPixel_texel.y * cosAngle);

        float2 samplePixelCoord_texel = centerPixelCoord_texel + rotatedVec_texel * radius_pixels;
        float2 sample_uv = samplePixelCoord_texel / screenSize_pixels;
        return sample_uv;
      }

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
        // https : //gist.github.com / CianNoonan / c56256433801991038c9c40a48fe3002
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
        float d = SAMPLE_TEXTURE2D_X(_ScreenSpaceSDFTexture, sampler_ScreenSpaceSDFTexture, input.uv).r;

        if (d < 0)
        discard;

        float EinsteinRingThickness = _EinsteinRingThickness;
        float PhotonSphereMin = _PhotonSphereMin;
        float PhotonSpheremax = _PhotonSphereMin + _PhotonSphereThickness;
        bool EinsteinRing = d <= EinsteinRingThickness;
        bool PhotonSphere = d >= PhotonSphereMin && d <= PhotonSpheremax;
        bool WarpedBackground = d > EinsteinRingThickness && d < PhotonSphereMin;
        if (EinsteinRing) {
          o.color = _EinsteinRingColor;
        } else if (PhotonSphere) {
          o.color = _PhotonSphereColor;
        } else if (WarpedBackground) {
          float maxPixelRadius = 100;
          float strength = 1 - smoothstep(EinsteinRingThickness, PhotonSphereMin, d);
          float2 radiallyOffsetUV = ComputeRadiallyOffsetSampleUV(
          input.uv,
          _ScreenSpaceSDFTexture,
          sampler_ScreenSpaceSDFTexture,
          _ScreenSpaceSDFTexture_TexelSize,
          strength * _RadialDistortionAngle,
          maxPixelRadius,
          _ScreenParams.xy);

          o.color = SAMPLE_TEXTURE2D_X(
          _ColorTexture,
          sampler_ColorTexture,
          radiallyOffsetUV);
          o.color += pow(strength, 2) * _InnerRegionTint;
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
