using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Rendering)]
public class WeaponAim : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Transform Weapon;

  Quaternion DefaultLocalRotation;

  public Vector3? AimDirection = null;

  public void Aim(float dt, float turnSpeed = 720) {
    var aimDirection = AimDirection;
    var targetLocalRotation = aimDirection.HasValue
      ? WeaponLocalRotationFromWorldSpaceVector(aimDirection.Value)
      : DefaultLocalRotation;
    var maxDegrees = dt * turnSpeed;
    Weapon.localRotation = Quaternion.RotateTowards(Weapon.localRotation, targetLocalRotation, maxDegrees);
  }

  Quaternion WeaponLocalRotationFromWorldSpaceVector(Vector3 v) {
    var worldDirection = transform.TransformDirection(v);
    var weaponLocalDirection = Weapon.parent.InverseTransformDirection(worldDirection);
    return Quaternion.LookRotation(weaponLocalDirection);
  }

  void Start() {
    DefaultLocalRotation = Weapon.localRotation;
  }

  void FixedUpdate() {
    Aim(LocalClock.DeltaTime());
  }
}