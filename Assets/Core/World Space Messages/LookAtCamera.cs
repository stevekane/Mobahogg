using UnityEngine;

public class LookAtCamera : MonoBehaviour {
  void LateUpdate() {
    var camera = CameraManager.Instance.Active;
    if (camera)
      transform.LookAt(transform.position + camera.transform.forward);
  }
}