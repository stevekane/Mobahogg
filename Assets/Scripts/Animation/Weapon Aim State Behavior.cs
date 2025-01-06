using UnityEngine;

public class WeaponAimStateBehavior : StateMachineBehaviour {
  public Vector3 AttackerDirection = Vector3.forward;

  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    var weaponAim = animator.GetComponent<WeaponAim>();
    weaponAim.Direction = AttackerDirection;
    weaponAim.Enabled = true;
  }

  public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    var weaponAim = animator.GetComponent<WeaponAim>();
    weaponAim.Enabled = false;
  }
}