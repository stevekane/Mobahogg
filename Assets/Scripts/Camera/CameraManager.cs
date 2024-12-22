using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : SingletonBehavior<CameraManager> {
  public Camera Active;

  public void Shake(float intensity) {
    var activeBrain = Instance.Active.GetComponent<CinemachineBrain>();
    var camera = activeBrain.ActiveVirtualCamera as CinemachineCamera;
    var cameraShaker = camera.GetComponent<CameraShaker>();
    if (cameraShaker)
      cameraShaker.Shake(intensity);
  }
}