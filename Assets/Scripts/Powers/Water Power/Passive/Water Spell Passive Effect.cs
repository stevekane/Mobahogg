using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaterSpellPassiveEffect : PowerEffect {
  [SerializeField] WaterSpellSettings Settings;

  LocalClock LocalClock;
  SpellAffected SpellAffected;

  void Start() {
    LocalClock = EffectManager.GetComponent<LocalClock>();
    SpellAffected = EffectManager.GetComponent<SpellAffected>();
    Run(this.destroyCancellationToken).Forget();
  }

  async UniTask Run(CancellationToken token) {
    while (true) {
      await Tasks.Delay(Settings.PassiveHealCooldown.Ticks, LocalClock, token);
      SpellAffected.ChangeHealth(Settings.PassiveHealAmount);
    }
  }
}