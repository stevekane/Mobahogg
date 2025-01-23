using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ChimeEffect : Effect {
  [SerializeField] LightPowerSettings Settings;

  void Start() {
    Run(this.destroyCancellationToken).Forget();
  }

  async UniTask Run(CancellationToken token) {
    try {
      var localClock = EffectManager.GetComponent<LocalClock>();
      var spellAffected = EffectManager.GetComponent<SpellAffected>();
      var duration = Settings.ChimeSpeedSurgeDuration.Ticks;
      var maxSpeed = Settings.ChimeSpeedSurgeAmount;
      spellAffected.ChangeHealth(1);
      for (var i = 0; i < duration; i++) {
        var interpolant = i/(float)duration;
        var curved = Settings.ChimeSpeedCurve.Evaluate(interpolant);
        var speed = Mathf.Lerp(0, maxSpeed, curved);
        spellAffected.AddSpeed(speed);
        await Tasks.Delay(1, localClock, token);
      }
    } finally {
      EffectManager.Unregister(this);
    }
  }
}