using Unity.Cinemachine;
using UnityEngine;

public class CameraShaker : CinemachineExtension {
  [SerializeField] CameraConfig Config;
  [SerializeField] CinemachineBasicMultiChannelPerlin Noise;

  protected override void OnDestroy() {
    base.OnDestroy();
    Noise.AmplitudeGain = 0;
  }

  public void Shake(float targetIntensity) {
    Noise.AmplitudeGain = Mathf.Min(Noise.AmplitudeGain+targetIntensity, Config.MAX_SHAKE_INTENSITY);
  }

  protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float dt) {
    Noise.AmplitudeGain = Mathf.Lerp(0, Noise.AmplitudeGain, Mathf.Exp(dt*Config.SHAKE_DECAY_EPSILON));
  }
}