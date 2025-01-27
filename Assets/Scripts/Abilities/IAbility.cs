using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Abilities {
  public abstract class Ability : MonoBehaviour {
    protected AbilityManager AbilityManager;
    protected LocalClock LocalClock => AbilityManager.LocalClock;
    protected Animator Animator => AbilityManager.Animator;
    protected AnimatorCallbackHandler AnimatorCallbackHandler => AbilityManager.AnimatorCallbackHandler;
    protected KCharacterController CharacterController => AbilityManager.CharacterController;

    public void Register(AbilityManager abilityManager) {
      AbilityManager = abilityManager;
    }
    public abstract bool CanRun { get; }
    public abstract bool CanCancel { get; }
    public abstract bool IsRunning { get; }
    public abstract void Run();
    public abstract void Cancel();
  }

  public abstract class UniTaskAbility : Ability {
    CancellationTokenSource CancellationTokenSource;
    bool IsActive;

    void Activate() => IsActive = true;
    void Deactivate() => IsActive = false;

    public override bool IsRunning => IsActive;
    public override void Run() {
      CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.destroyCancellationToken);
      Activate();
      Task(CancellationTokenSource.Token)
      .ContinueWith(Deactivate)
      .Forget();
    }
    public override void Cancel() {
      Deactivate();
      if (CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested) {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
      }
    }

    protected abstract UniTask Task(CancellationToken token);
  }

  public interface IAimed {
    public bool CanAim { get; }
    public void Aim(Vector2 direction);
  }

  public interface ISteered {
    public bool CanSteer { get; }
    public void Steer(Vector2 direction);
  }

  public interface IHeld {
    public bool CanRelease { get; }
    public void Release();
  }

  public interface IAbilityStartCondition {
    public bool Satisfied(AbilityManager abilityManager);
  }
}