using Abilities;
using UnityEngine;

public class SpellCastAbility : MonoBehaviour, IAbility<Vector2>, Async, Cancellable {
  [Header("Reads From")]
  [SerializeField] LocalClock LocalClock;
  [Header("Writes To")]
  [SerializeField] SpellHolder SpellHolder;
  [SerializeField] Player Player;
  [SerializeField] Animator Animator;
  [SerializeField] int TotalAttackFrames = 20;

  int Frame;

  void Awake() {
    Frame = TotalAttackFrames;
  }

  public bool IsRunning => Frame < TotalAttackFrames;

  public bool CanRun => SpellHolder.Count > 0;

  public bool CanCancel => false;

  public void Run(Vector2 direction) {
    var spellPrefab = SpellHolder.Dequeue();
    var position = transform.position + transform.forward + transform.up;
    var rotation = transform.rotation;
    var spell = Instantiate(spellPrefab);
    spell.Cast(position, rotation, Player);
    Animator.SetTrigger("Cast");
    Frame = 0;
  }

  public void Cancel() {
    Frame = TotalAttackFrames;
    Animator.SetTrigger("Cancel");
  }

  void FixedUpdate() {
    Frame = Mathf.Min(TotalAttackFrames, Frame+LocalClock.DeltaFrames());
  }
}