using UnityEngine;

/*
This optional system will constrain the direction of the weapon
to point in the desired direction. It is designed to run in the AnimatorIK
callback though it probably does not do any actual IK.

It could be implemented as a driving script for an AnimationRig
such as an aim constraint which would allow it to exist alongside
other animation tech in the standard Unity Setup.
*/
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