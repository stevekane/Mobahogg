using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Rendering)]
public class WeaponAim : MonoBehaviour {
  [SerializeField] AnimatorCallbackHandler AnimatorCallbackHandler;
  [SerializeField] Transform Weapon;

  public Quaternion DefaultLocalRotation;
  public Vector3 Direction = Vector3.forward;
  public float TurnSpeed = 360;
  public bool Enabled = true;

  void Start() {
    DefaultLocalRotation = Weapon.localRotation;
    AnimatorCallbackHandler.OnIK.Listen(OnAnimatorIK);
  }

  void OnDestroy() {
    AnimatorCallbackHandler.OnIK.Unlisten(OnAnimatorIK);
  }

  void OnAnimatorIK(int layer) {
    var worldDirection = transform.TransformDirection(Direction);
    var weaponLocalDirection = Weapon.parent.InverseTransformDirection(worldDirection);
    var weaponLocalRotation = Quaternion.LookRotation(weaponLocalDirection);
    var target = Enabled ? weaponLocalRotation : DefaultLocalRotation;
    var nextLocalRotation = target;
    Weapon.localRotation = nextLocalRotation;
  }
}