using System.Threading;
using Cysharp.Threading.Tasks;
using KinematicCharacterController;
using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class Player : MonoBehaviour {
  [Header("Settings")]
  [SerializeField] AbilitySettings Settings;

  [Header("Character Control")]
  [SerializeField] KCharacterController CharacterController;

  [Header("Child References")]
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Health Health;

  // TODO: Would it make sense to use the already-available "name" property Unity has?
  public string Name => MatchManager.Instance.Players[PortIndex].Name;
  public AttackAbility AttackAbility;
  public SpinAbility SpinAbility;
  public SpellCastAbility SpellCastAbility;
  public MoveAbility MoveAbility;
  public TurnAbility TurnAbility;
  public int PortIndex;

  void Start() {
    LivesManager.Active.Players.AddFirst(this);
  }

  void OnDestroy() {
    LivesManager.Active.Players.Remove(this);
  }

  void FixedUpdate() {
    // TODO: Handling falling death in this hardcoded way probably isn't that bright...
    if (!LocalClock.Frozen() && (Health.CurrentValue <= 0 || transform.position.y <= -10)) {
      // TODO: This is probably not quite right. Seems like potentially this could be an
      // event that goes on some bus where high level systems listen to react?
      CreepManager.Active.OnOwnerDeath(GetComponent<CreepOwner>());
      LivesManager.Active.OnPlayerDeath(this);
    }
  }

  #region JUMP
  public bool CanJump() => CharacterController.IsGrounded;
  public bool TryJump() {
    if (CanJump()) {
      var jumpSpeed = Settings.InitialJumpSpeed(Physics.gravity.y);
      CharacterController.ForceUnground.Set(true);
      CharacterController.Velocity.SetY(jumpSpeed);
      return true;
    } else {
      return false;
    }
  }
  #endregion
}