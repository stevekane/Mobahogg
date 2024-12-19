using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
public class BattlefieldCamera : CinemachineExtension {
  public Transform Target; // Target for the camera to follow
  public float Distance = 10f; // Distance from the target
  public float Pitch = 30f; // Pitch angle in degrees

  protected override void PostPipelineStageCallback(
  CinemachineVirtualCameraBase vcam,
  CinemachineCore.Stage stage,
  ref CameraState state,
  float deltaTime) {
    if (Target == null || stage != CinemachineCore.Stage.Body)
      return;
    var rotation = Quaternion.Euler(Pitch, 0, 0);
    var position = Target.position - Distance * (rotation * Vector3.forward);
    state.RawPosition = position;
    state.RawOrientation = rotation;
  }
}
