using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abilities {
  public class AbilityManager : MonoBehaviour {
    public Transform AbilityContainer;
    public List<RegisteredAbility> RegisteredAbilities;
    public LocalClock LocalClock;
    public Animator Animator;
    public AnimatorCallbackHandler AnimatorCallbackHandler;
    public KCharacterController CharacterController;

    public EventSource<Ability> OnRegisterAbility = new();
    public EventSource<Ability> OnUnregisterAbility = new();

    void Start() {
      AbilityContainer.GetComponentsInChildren<Ability>().ForEach(Register);
    }

    void OnDestroy() {
      AbilityContainer.GetComponentsInChildren<Ability>().ForEach(Unregister);
    }

    public T LocateComponent <T>() {
      return GetComponentInChildren<T>();
    }

    public void Register(Ability ability) {
      var registeredAbility = RegisteredAbility.From(ability);
      registeredAbility.Ability.Register(this);
      RegisteredAbilities.Add(registeredAbility);
      OnRegisterAbility.Fire(ability);
      ability.transform.SetParent(AbilityContainer, false);
    }

    public void Unregister(Ability ability) {
      Unregister(ability, true);
    }

    public void Unregister(Ability ability, bool destroy) {
      var registeredAbility = RegisteredAbilities.First(ra => ra.Ability == ability);
      RegisteredAbilities.Remove(registeredAbility);
      OnUnregisterAbility.Fire(ability);
      if (destroy)
        Destroy(ability.gameObject);
    }

    public bool CanRun(Ability ability) {
      var matchingAbility = RegisteredAbilities.First(ra => ra.Ability == ability);
      var shouldRun = true;
      foreach (var registeredAbility in RegisteredAbilities) {
        if (registeredAbility.Ability.IsRunning) {
          var doesBlock = TagSet.Overlap(registeredAbility.AbilityTags.BlockAbilitiesWith, matchingAbility.AbilityTags.Tags);
          var shouldCancel = TagSet.Overlap(registeredAbility.AbilityTags.Tags, matchingAbility.AbilityTags.CancelAbilitiesWith);
          var canCancel = registeredAbility.Ability.CanCancel;
          shouldRun = !doesBlock || (shouldCancel && canCancel);
        }
      }
      return shouldRun && matchingAbility.Ability.CanRun;
    }

    public bool TryRun(Ability ability) {
      if (CanRun(ability)) {
        Run(ability);
        return true;
      } else {
        return false;
      }
    }

    public void Run(Ability ability) {
      var matchingAbility = RegisteredAbilities.FirstOrDefault(ra => ra.Ability == ability);
      if (matchingAbility == null)
        return;

      foreach (var registeredAbility in RegisteredAbilities) {
        var shouldCancel = TagSet.Overlap(registeredAbility.AbilityTags.Tags, matchingAbility.AbilityTags.CancelAbilitiesWith);
        var canCancel = registeredAbility.Ability.CanCancel;
        if (shouldCancel && canCancel) {
          registeredAbility.Ability.Cancel();
        }
      }
      ability.Run();
    }
  }

  [Serializable]
  public class RegisteredAbility {
    public Ability Ability;
    public AbilityTags AbilityTags;
    public static RegisteredAbility From(Ability ability) {
      return new RegisteredAbility {
        Ability = ability,
        AbilityTags = ability.GetComponentInChildren<AbilityTags>()
      };
    }
  }

}