using UnityEngine;

public class SpellCastAbility : MonoBehaviour, IAbility {
  [SerializeField] SpellHolder SpellHolder;
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] Player Player;
  [SerializeField] AttackAbility AttackAbility;
  [SerializeField] AbilitySettings Settings;
  [SerializeField] Animator Animator;
  [SerializeField] LocalClock LocalClock;

  int Frame;

  void Awake() {
    Frame = Settings.TotalAttackFrames;
  }

  public bool IsRunning => Frame < Settings.TotalAttackFrames;

  public bool CanRun
    => !IsRunning
    && CharacterController.IsGrounded
    && SpellHolder.Count > 0
    && !AttackAbility.IsRunning;

  public bool TryRun() {
    if (CanRun) {
      var spellPrefab = SpellHolder.Dequeue();
      var position = transform.position + transform.forward + transform.up;
      var rotation = transform.rotation;
      var spell = Instantiate(spellPrefab);
      spell.Cast(position, rotation, Player);
      Animator.SetTrigger("Cast");
      Frame = 0;
      return true;
    } else {
      return false;
    }
  }

  public void Cancel() {
    Frame = Settings.TotalAttackFrames;
  }

  void FixedUpdate() {
    Frame = Mathf.Min(Settings.TotalAttackFrames, Frame+LocalClock.DeltaFrames());
  }
}