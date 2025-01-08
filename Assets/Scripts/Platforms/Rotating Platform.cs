using KinematicCharacterController;
using UnityEngine;

public class RotatingPlatform : MonoBehaviour, IMoverController {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] float DegreesPerSecond = 30f;

  void Start() {
    GetComponent<PhysicsMover>().MoverController = this;
  }

  void OnDestroy() {
    GetComponent<PhysicsMover>().MoverController = null;
  }

  public void UpdateMovement(out Vector3 position, out Quaternion rotation, float deltaTime) {
    var axis = transform.parent.up;
    var origin = transform.parent.position;
    var angle = LocalClock.DeltaTime() * DegreesPerSecond;
    var rotationStep = Quaternion.AngleAxis(angle, axis);
    var offset = transform.position - origin;
    offset = rotationStep * offset;
    position = origin + offset;
    rotation = rotationStep * transform.rotation;
  }
}
