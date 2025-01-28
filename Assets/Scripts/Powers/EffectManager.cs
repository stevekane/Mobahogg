using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class EffectManager : MonoBehaviour {
  [SerializeField] Transform EffectsContainer;

  List<Effect> Effects = new();

  public void Register(Effect effect) {
    effect.EffectManager = this;
    effect.transform.SetParent(EffectsContainer, false);
    Effects.Add(effect);
  }

  public void Unregister(Effect effect) {
    effect.EffectManager = null;
    effect.DestroyGameObject();
    Effects.Remove(effect);
  }
}