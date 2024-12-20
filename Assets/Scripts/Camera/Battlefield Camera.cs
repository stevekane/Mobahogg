using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
public class BattlefieldCamera : CinemachineExtension {
  public Vector3 Pivot;
  public float Distance = 10f;
  public float Pitch = 30f;
  public float HorizontalFOV = 45;

  protected override void PostPipelineStageCallback(
  CinemachineVirtualCameraBase vcam,
  CinemachineCore.Stage stage,
  ref CameraState state,
  float deltaTime) {
    if (Pivot == null || stage != CinemachineCore.Stage.Body)
      return;
    var rotation = Quaternion.Euler(Pitch, 0, 0);
    var position = Pivot - Distance * (rotation * Vector3.forward);
    state.RawPosition = position;
    state.RawOrientation = rotation;
    state.Lens.FieldOfView = HorizontalToVerticalFOV(HorizontalFOV, state.Lens.Aspect);
  }

  float HorizontalToVerticalFOV(float horizontalFOV, float aspectRatio) {
    var radHFOV = horizontalFOV * Mathf.Deg2Rad;
    var radVFOV = 2 * Mathf.Atan(Mathf.Tan(radHFOV / 2) / aspectRatio);
    return radVFOV * Mathf.Rad2Deg;
  }
}