using Abilities;
using UnityEngine;

public class SpellCastAbility : Ability, IAimed {
  [SerializeField] int TotalAttackFrames = 20;
  [SerializeField] GameObject SpellPrefab;

  int Frame;

  void Awake() {
    Frame = TotalAttackFrames;
  }

  void OnDestroy() {
    Frame = TotalAttackFrames;
  }

  public override bool IsRunning => Frame < TotalAttackFrames;

  public bool CanAim => Frame == 0;
  public void Aim(Vector2 input) {
    var delta = input.XZ();
    var rotation = delta.sqrMagnitude > 0
      ? Quaternion.LookRotation(delta.normalized)
      : transform.rotation;
    CharacterController.Rotation.Set(rotation);
    var position = transform.position + rotation * Vector3.forward + transform.up;
    var spell = Instantiate(SpellPrefab, position, rotation);
    // note, for now this needs to happen here otherwise aim is not called
    // because removing the spell from SpellHolder causes the ultimate reference
    // to be nulled out ( which is correct )
    AbilityManager.GetComponent<SpellHolder>().Remove();
    // TODO: Possibly assign an owner or something to attribute damage/kills to
  }

  public override bool CanRun => true;
  public override void Run() {
    Frame = 0;
  }

  public override bool CanCancel => false;
  public override void Cancel() {
    Frame = TotalAttackFrames;
  }

  void FixedUpdate() {
    if (!IsRunning)
      return;
    Frame = Mathf.Min(TotalAttackFrames, Frame+LocalClock.DeltaFrames());
  }
}