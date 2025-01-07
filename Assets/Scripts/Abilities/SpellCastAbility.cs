using Abilities;
using UnityEngine;

public class SpellCastAbility : MonoBehaviour, IAbility<Vector2>, Async, Cancellable {
  [Header("Reads From")]
  [SerializeField] AbilitySettings Settings;
  [SerializeField] LocalClock LocalClock;
  [Header("Writes To")]
  [SerializeField] SpellHolder SpellHolder;
  [SerializeField] Player Player;
  [SerializeField] Animator Animator;

  int Frame;

  void Awake() {
    Frame = Settings.TotalAttackFrames;
  }

  public bool IsRunning => Frame < Settings.TotalAttackFrames;

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
    Frame = Settings.TotalAttackFrames;
    Animator.SetTrigger("Cancel");
  }

  void FixedUpdate() {
    Frame = Mathf.Min(Settings.TotalAttackFrames, Frame+LocalClock.DeltaFrames());
  }
}