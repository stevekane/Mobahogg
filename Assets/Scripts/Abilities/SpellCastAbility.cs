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

  public override bool CanRun => SpellHolder.Count > 0;

  public override bool CanStop => false;

  public void Aim(Vector2 input) {
    var spellPrefab = SpellHolder.Dequeue();
    var delta = input.XZ();
    var rotation = delta.sqrMagnitude > 0
      ? Quaternion.LookRotation(delta.normalized)
      : transform.rotation;
    CharacterController.Rotation.Set(rotation);
    var position = transform.position + rotation * Vector3.forward + transform.up;
    var spell = Instantiate(spellPrefab);
    spell.Cast(position, rotation, AbilityManager);
  }

  public override void Run() {
    Frame = 0;
  }

  public override void Stop() {
    Frame = TotalAttackFrames;
  }

  void FixedUpdate() {
    Frame = Mathf.Min(TotalAttackFrames, Frame+LocalClock.DeltaFrames());
  }
}