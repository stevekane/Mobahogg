using Abilities;
using UnityEngine;

public class SpellCastAbility : Ability {
  [Header("Writes To")]
  [SerializeField] SpellHolder SpellHolder;
  [SerializeField] int TotalAttackFrames = 20;

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
    var spellPrefab = SpellHolder.Remove();
    var delta = input.XZ();
    var rotation = delta.sqrMagnitude > 0
      ? Quaternion.LookRotation(delta.normalized)
      : transform.rotation;
    CharacterController.Rotation.Set(rotation);
    var position = transform.position + rotation * Vector3.forward + transform.up;
    var spell = Instantiate(spellPrefab);
    spell.Cast(position, rotation, AbilityManager);
  }

  public override bool CanRun => SpellHolder.Spell != null;
  public override void Run() {
    Frame = 0;
  }

  public override bool CanCancel => false;
  public override void Cancel() {
    Frame = TotalAttackFrames;
  }

  void FixedUpdate() {
    Frame = Mathf.Min(TotalAttackFrames, Frame+LocalClock.DeltaFrames());
  }
}