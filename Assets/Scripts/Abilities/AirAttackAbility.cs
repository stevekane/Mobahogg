using System.Threading;
using Cysharp.Threading.Tasks;
using State;
using UnityEngine;

public class AirAttackAbility : MonoBehaviour, IAbility<Vector2> {
  [SerializeField] Player Player;
  [SerializeField] Animator Animator;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] MoveSpeed MoveSpeed;
  [SerializeField] TurnSpeed TurnSpeed;

  public bool IsRunning { get; private set; } = false;
  public bool CanRun => true;
  public bool TryRun(Vector2 direction) {
    Run(this.destroyCancellationToken).Forget();
    return true;
  }

  async UniTask Run(CancellationToken token) {
    try {
      IsRunning = true;
      await UniTask.WaitUntil(() => Player.Grounded, PlayerLoopTiming.FixedUpdate, cancellationToken: token);
      Animator.SetTrigger("Air Attack");
      await Tasks.EveryFrame(30, LocalClock, f => {
        MoveSpeed.Set(0);
        TurnSpeed.Set(0);
      }, token);
    } finally {
      IsRunning = false;
    }
  }
}