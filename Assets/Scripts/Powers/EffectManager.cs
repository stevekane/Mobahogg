using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class EffectManager : MonoBehaviour {
  [SerializeField] Transform EffectsContainer;

  List<Effect> Effects = new();

  public void Register(Effect effect) {
    effect.EffectManager = this;
    Effects.Add(effect);
    effect.transform.SetParent(EffectsContainer, false);
  }

  public void Unregister(Effect effect) {
    effect.EffectManager = null;
    Effects.Remove(effect);
    Destroy(effect.gameObject);
  }
}