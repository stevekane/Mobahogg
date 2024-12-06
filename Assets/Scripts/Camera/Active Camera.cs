using UnityEngine;

public class ActiveCamera : MonoBehaviour {
  void Start() {
    CameraManager.Instance.Active = GetComponent<Camera>();
  }
}