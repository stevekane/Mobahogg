using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {
  public int PortIndex;

  void Start() {
    InputRouter.Instance?.TryListen("Move", PortIndex, HandleMove);
    InputRouter.Instance?.TryListen("Jump", PortIndex, HandleJump);
    InputRouter.Instance?.TryListen("Dash", PortIndex, HandleDash);
    InputRouter.Instance?.TryListen("Attack", PortIndex, HandleAttack);
    InputRouter.Instance?.TryListen("Cast Spell", PortIndex, HandleCastSpell);
    InputRouter.Instance?.TryListen("Spin", PortIndex, HandleSpin);
  }

  void OnDestroy() {
    InputRouter.Instance?.TryUnlisten("Move", PortIndex, HandleMove);
    InputRouter.Instance?.TryUnlisten("Jump", PortIndex, HandleJump);
    InputRouter.Instance?.TryUnlisten("Dash", PortIndex, HandleDash);
    InputRouter.Instance?.TryUnlisten("Attack", PortIndex, HandleAttack);
    InputRouter.Instance?.TryUnlisten("Cast Spell", PortIndex, HandleCastSpell);
    InputRouter.Instance?.TryUnlisten("Spin", PortIndex, HandleSpin);
  }

  // TODO:
  // This is sort of a hack for now to send MoveEvents for any player controller
  // That does not have an active connected device
  // Think of this like a poor-man's AI
  void FixedUpdate() {
    if (InputRouter.Instance.HasConnectedDevice(PortIndex))
      return;
    PlayersOnPort.ForEach(p => p.MoveAbility.TryRun(Vector2.zero));
  }

  IEnumerable<Player> PlayersOnPort =>
    LivesManager.Active.Players.Where(p => p.PortIndex == PortIndex);

  public void HandleMove(PortAction action) {
    PlayersOnPort.ForEach(p => p.MoveAbility.TryRun(action.Value));
    PlayersOnPort.ForEach(p => p.TurnAbility.TryRun(action.Value));
  }

  public void HandleJump(PortAction action) => PlayersOnPort.ForEach(p => p.TryJump());

  public void HandleDash(PortAction action) => PlayersOnPort.ForEach(p => p.TryDash());

  public void HandleAttack(PortAction action) => PlayersOnPort.ForEach(p => p.AttackAbility.TryRun());

  public void HandleCastSpell(PortAction action) => PlayersOnPort.ForEach(p => p.SpellCastAbility.TryRun());

  public void HandleSpin(PortAction action) => PlayersOnPort.ForEach(p => p.SpinAbility.TryRun());
}