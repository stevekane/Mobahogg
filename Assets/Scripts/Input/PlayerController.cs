using UnityEngine;

public class PlayerController : MonoBehaviour {
  [SerializeField] Player Player;

  public int PortIndex;

  void Start() {
    InputRouter.Instance.TryListen("Move", PortIndex, HandleMove);
    InputRouter.Instance.TryListen("Jump", PortIndex, HandleJump);
    InputRouter.Instance.TryListen("Dash", PortIndex, HandleDash);
    InputRouter.Instance.TryListen("Attack", PortIndex, HandleAttack);
    InputRouter.Instance.TryListen("Spin", PortIndex, HandleSpin);
    InputRouter.Instance.TryListen("Test", PortIndex, HandleTest);
  }

  void OnDestroy() {
    InputRouter.Instance.TryUnlisten("Move", PortIndex, HandleMove);
    InputRouter.Instance.TryUnlisten("Jump", PortIndex, HandleJump);
    InputRouter.Instance.TryUnlisten("Dash", PortIndex, HandleDash);
    InputRouter.Instance.TryUnlisten("Attack", PortIndex, HandleAttack);
    InputRouter.Instance.TryUnlisten("Spin", PortIndex, HandleSpin);
    InputRouter.Instance.TryUnlisten("Test", PortIndex, HandleTest);
  }

  public void HandleMove(PortAction action) => Player.TryMove(action.Value);

  public void HandleJump(PortAction action) => Player.TryJump();

  public void HandleDash(PortAction action) => Player.TryDash();

  public void HandleAttack(PortAction action) => Player.AttackAbility.TryRun();

  public void HandleSpin(PortAction action) => Player.SpinAbility.TryRun();

  public void HandleTest(PortAction action) => WorldSpaceMessageManager.Instance.SpawnMessage("Cracktober", Player.transform.position, 3);
}