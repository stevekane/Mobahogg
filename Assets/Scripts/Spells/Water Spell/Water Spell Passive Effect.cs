using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaterSpellPassiveEffect : SpellPassiveEffect {
  [SerializeField] WaterSpellSettings Settings;

  void Start() {
    Run(this.destroyCancellationToken).Forget();
  }

  async UniTask Run(CancellationToken token) {
    while (true) {
      await Tasks.Delay(Settings.PassiveHealCooldown.Ticks, LocalClock, token);
      SpellAffected.ChangeHealth(Settings.PassiveHealAmount);
    }
  }
}