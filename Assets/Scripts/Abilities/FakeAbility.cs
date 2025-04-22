using System;
using UnityEngine;
using AbilityStateMachine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(FakeAbility))]
class FakeAbilityDrawer : Editor {
  public override void OnInspectorGUI() {
    DrawDefaultInspector();
    var ability = target as FakeAbility;
    if (EditorApplication.isPlaying) {
      GUILayout.BeginHorizontal("box");
      if (GUILayout.Button("Run")) {
        ability.Run();
      }
      if (GUILayout.Button("Interupt")) {
        ability.Interupt();
      }
      if (GUILayout.Button("Stop")) {
        ability.Stop();
      }
      GUILayout.EndHorizontal();
    }
  }
}
#endif

class FakeAbility : MonoBehaviour, IAbilityStateMachine<FakeAbility> {
  [SerializeField] internal ReadyState ReadyState;
  [SerializeField] internal WindupState WindupState;
  [SerializeField] internal ChannelingState ChannelingState;
  [SerializeField] internal RecoveryState RecoveryState;
  public AbilityState<FakeAbility> CurrentState { get; set; }

  void Awake() {
    ReadyState.StateMachine = this;
    WindupState.StateMachine = this;
    ChannelingState.StateMachine = this;
    RecoveryState.StateMachine = this;
    CurrentState = ReadyState;
  }

  public void TransitionTo(AbilityState<FakeAbility> state) {
    TimeManager.Instance.Log($"{CurrentState.GetType()} -> {state.GetType()}");
    CurrentState.Exit();
    CurrentState = state;
    CurrentState.Enter();
  }

  [ContextMenu("Run")]
  public void Run() {
    if (CurrentState is IRunnable runnable) {
      runnable.Run();
    }
  }

  [ContextMenu("Interupt")]
  public void Interupt() {
    if (CurrentState is IInteruptible interuptible) {
      interuptible.Interupt();
    }
  }

  [ContextMenu("Stop")]
  public void Stop() {
    if (CurrentState is IStoppable stoppable) {
      stoppable.Stop();
    }
  }

  void FixedUpdate() {
    if (CurrentState is ITickable tickable) {
      tickable.Tick();
    }
  }
}

[Serializable]
class ReadyState : AbilityState<FakeAbility>, IRunnable {
  public void Run() => StateMachine.TransitionTo(StateMachine.WindupState);
}

[Serializable]
class WindupState : AbilityState<FakeAbility>, IInteruptible, ITickable {
  int Frames;
  [SerializeField]
  int TotalFrames;
  public override void Enter() => Frames = TotalFrames;
  public override void Exit() => Frames = 0;
  public void Interupt() => StateMachine.TransitionTo(StateMachine.ReadyState);
  public void Tick() {
    if (--Frames <= 0) {
      StateMachine.TransitionTo(StateMachine.ChannelingState);
    }
  }
}

[Serializable]
class ChannelingState : AbilityState<FakeAbility>, IInteruptible, IStoppable, ITickable {
  public void Interupt() => StateMachine.TransitionTo(StateMachine.ReadyState);
  public void Stop() => StateMachine.TransitionTo(StateMachine.RecoveryState);
  public void Tick() {}
}

[Serializable]
class RecoveryState : AbilityState<FakeAbility>, IInteruptible, ITickable {
  int Frames;
  [SerializeField]
  int TotalFrames;
  public override void Enter() => Frames = TotalFrames;
  public override void Exit() => Frames = 0;
  public void Interupt() => StateMachine.TransitionTo(StateMachine.ReadyState);
  public void Tick() {
    if (--Frames <= 0) {
      StateMachine.TransitionTo(StateMachine.ReadyState);
    }
  }
}

namespace AbilityStateMachine {
  interface IAbilityState<T> where T : IAbilityStateMachine<T> {
    public T StateMachine { get; set; }
    public void Enter() {}
    public void Exit() {}
  }

  abstract class AbilityState<T> : IAbilityState<T> where T : IAbilityStateMachine<T> {
    public T StateMachine { get; set; }
    public virtual void Enter() {}
    public virtual void Exit() {}
  }

  interface IAbilityStateMachine<T> where T : IAbilityStateMachine<T> {
    public AbilityState<T> CurrentState { get; set; }
    public void TransitionTo(AbilityState<T> state);
  }

  interface IInteruptible {
    public void Interupt();
  }

  interface IStoppable {
    public void Stop();
  }

  interface IRunnable {
    public void Run();
  }

  interface ITickable {
    public void Tick();
  }
}