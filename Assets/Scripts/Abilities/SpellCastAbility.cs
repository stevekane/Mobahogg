using UnityEngine;

public class SpellCastAbility : MonoBehaviour {
  [SerializeField] SpellHolder SpellHolder;
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] Player Player;
  [SerializeField] AttackAbility AttackAbility;
  [SerializeField] AbilitySettings Settings;
  [SerializeField] Animator Animator;
  [SerializeField] LocalClock LocalClock;

  /*
  Remove the first spell.
  Could color the spell charges to verify the behavior is correct
  */

  int Frame;

  void Awake() {
    Frame = Settings.TotalAttackFrames;
  }

  public bool IsRunning => Frame < Settings.TotalAttackFrames;

  public bool CanRun
    => CharacterController.IsGrounded
    && SpellHolder.SpellQueue.Count > 0
    && !Player.IsDashing()
    && !AttackAbility.IsRunning
    && (Frame < Settings.WindupAttackFrames || Frame >= Settings.TotalAttackFrames);

  public bool TryRun() {
    if (CanRun) {
      // TODO: Hacky way to test instant-cast. probably not correct for final
      Debug.Log("Tried to cast a spell");
      SpellHolder.Dequeue();
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