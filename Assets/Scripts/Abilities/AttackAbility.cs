using UnityEngine;
using Abilities;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface ICancellable {
  public bool Cancellable { get; set; }
}

public interface IAimable {
  public bool Aimable { get; set; }
}

public class AttackAbility : Ability, ICancellable, IAimable, ITypeAndTagProvider<BehaviorTag> {
  [SerializeField, InlineEditor] FrameBehaviors FrameBehaviors;
  [SerializeField] Hitbox Hitbox;

  UniTask Task;
  CancellationTokenSource CancellationTokenSource;

  public object Get(Type type, BehaviorTag tag) => (type, tag) switch {
    _ when type == typeof(Hitbox) => Hitbox,
    _ when type == typeof(ICancellable) => this,
    _ => AbilityManager.LocateComponent<FrameBehaviorProvider>().Get(type, tag)
  };

  [field:SerializeField] public bool Cancellable { get; set; }
  public override bool CanCancel => Cancellable;
  public override void Cancel() {
    CancellationTokenSource.Cancel();
    CancellationTokenSource.Dispose();
  }

  public override bool IsRunning =>
    Task.Status == UniTaskStatus.Pending
    && !CancellationTokenSource.IsCancellationRequested;
  public override bool CanRun => CharacterController.IsGrounded;
  public override void Run() {
    CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.destroyCancellationToken);
    Task = FrameBehaviors.RunInstance(this, LocalClock, CancellationTokenSource.Token);
  }

  [field:SerializeField]
  public bool Aimable { get; set; }
  public bool CanAim => Aimable;
  public void Aim(Vector2 direction) {
    if (direction.sqrMagnitude > 0) {
      CharacterController.Rotation.Set(Quaternion.LookRotation(direction.XZ()));
    }
  }
}