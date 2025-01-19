using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Rendering)]
public class WeaponAim : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] AnimatorCallbackHandler AnimatorCallbackHandler;
  [SerializeField] Transform Weapon;
  [SerializeField] float TurnSpeed = 720;

  Quaternion DefaultLocalRotation;

  public Vector3? AimDirection = null;

  Quaternion WeaponLocalRotationFromWorldSpaceVector(Vector3 v) {
    var worldDirection = transform.TransformDirection(v);
    var weaponLocalDirection = Weapon.parent.InverseTransformDirection(worldDirection);
    return Quaternion.LookRotation(weaponLocalDirection);
  }

  void Start() {
    DefaultLocalRotation = Weapon.localRotation;
    AnimatorCallbackHandler.OnIK.Listen(OnAnimatorIK);
  }

  void OnDestroy() {
    AnimatorCallbackHandler.OnIK.Unlisten(OnAnimatorIK);
  }

  void OnAnimatorIK(int layer) {
    var aimDirection = AimDirection;
    var targetLocalRotation = aimDirection.HasValue
      ? WeaponLocalRotationFromWorldSpaceVector(aimDirection.Value)
      : DefaultLocalRotation;
    var maxDegrees = LocalClock.DeltaTime() * TurnSpeed;
    Weapon.localRotation = Quaternion.RotateTowards(Weapon.localRotation, targetLocalRotation, maxDegrees);
  }
}