using System.Linq;
using UnityEngine;

public class SpellCastAbility : MonoBehaviour {
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
    => CharacterController.IsGrounded
    && !IsRunning
    && SpellHolder.SpellQueue.Count > 0
    && !Player.IsDashing()
    && !AttackAbility.IsRunning;

  public bool TryRun() {
    if (CanRun) {
      // This dequeue operation is very suspect. Multiple readers would have
      // order-dependence and it would matter what order these scripts ran in
      // this feels odd?
      // At the least, having to call ElementAt and ALSO Dequeue is quite strange
      var spellPrefab = SpellHolder.SpellQueue.ElementAt(0);
      var position = transform.position + transform.forward + transform.up;
      var rotation = transform.rotation;
      var spell = Instantiate(spellPrefab);
      spell.Cast(position, rotation, Player);
      SpellHolder.Dequeue();
      Animator.SetTrigger("Cast");
      Frame = 0;
      Debug.Log($"Tried to cast a spell {spell.GetType().Name}");
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