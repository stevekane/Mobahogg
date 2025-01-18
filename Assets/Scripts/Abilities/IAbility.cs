using UnityEngine;

namespace Abilities {
  public abstract class Ability : MonoBehaviour, IAbility {
    protected AbilityManager AbilityManager;
    protected LocalClock LocalClock => AbilityManager.LocalClock;
    protected Animator Animator => AbilityManager.Animator;
    protected AnimatorCallbackHandler AnimatorCallbackHandler => AbilityManager.AnimatorCallbackHandler;
    protected KCharacterController CharacterController => AbilityManager.CharacterController;

    public void Register(AbilityManager abilityManager) {
      AbilityManager = abilityManager;
    }
    public virtual bool CanRun { get; }
    public virtual bool CanStop { get; }
    public virtual bool IsRunning { get; }
    public virtual void Run() {}
    public virtual void Stop() {}
  }

  public interface IAbility {
    public bool CanRun { get; }
    public bool CanStop { get; }
    public bool IsRunning { get; }
    public void Run();
    public void Stop();
  }

  public interface IAbility<T> {
    public bool CanRun { get; }
    public bool CanStop { get; }
    public bool IsRunning { get; }
    public void Run(T t);
    public void Stop();
  }

  public interface IAbilityStartCondition {
    public bool Satisfied(AbilityManager abilityManager);
  }
}