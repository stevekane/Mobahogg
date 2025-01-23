using System;
using System.Threading;
using Abilities;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LightPowerUltimate : Ability {
  [SerializeField] LightPowerSettings Settings;

  CancellationTokenSource CancellationTokenSource;

  public override bool CanRun => true;
  public override void Run() {
    CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.destroyCancellationToken);
    AbilityTask(CancellationTokenSource.Token).Forget();
  }

  public override bool IsRunning => false;
  public override bool CanCancel => false;
  public override void Cancel() {
    if (CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested) {
      CancellationTokenSource.Cancel();
      CancellationTokenSource.Dispose();
    }
  }

  async UniTask AbilityTask(CancellationToken token) {
    try {
      Debug.Log("Began");
      await Tasks.EveryFrame(60, LocalClock, f => Debug.Log(f), token);
    } catch (Exception e) {
      Debug.LogWarning(e.Message);
    } finally {
      // N.B. THIS IS IMPORTANT LOL. Once you remove this, the ability itself is removed
      // from the ability manager lol...
      AbilityManager.GetComponent<SpellHolder>().Remove();
      Debug.Log("End");
    }
  }
}