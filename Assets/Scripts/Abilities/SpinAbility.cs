using System;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class SpinAbility : AbstractAsyncAbility {
  [SerializeField] AbilitySettings AbilitySettings;
  [SerializeField] Player Player;
  [SerializeField] Animator Animator;

  public override bool CanRun() => !IsRunning && !Player.IsAttacking() && !Player.IsDashing();
  public override async UniTask Run(CancellationToken token) {
    try {
      Animator.SetBool("Spinning", true);
      await UniTask.DelayFrame(AbilitySettings.TotalSpinFrames, PlayerLoopTiming.FixedUpdate, token);
    } catch (OperationCanceledException e) {
      Debug.Log($"Canceled {e.Message}");
    } finally {
      Debug.Log($"{Animator} {Animator == null}");
      Animator.SetBool("Spinning", false);
    }
  }
}