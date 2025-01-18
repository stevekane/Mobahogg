using UnityEngine;

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

  public interface IAbilityStartCondition {
    public bool Satisfied(AbilityManager abilityManager);
  }
}