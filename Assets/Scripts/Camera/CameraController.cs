using UnityEngine;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour {
  public Transform Target;
  public float distance = 10f;
  public float pitch = 30f; // Pitch angle in degrees

  void Update() {
    var rotation = Quaternion.Euler(pitch, 0, 0);
    var position = Target.position-distance*(rotation*Vector3.forward);
    transform.SetLocalPositionAndRotation(position, rotation);
  }
}