using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputActionType {
  JustDown,
  JustUp,
  Always,  // Triggers every frame, most useful with a parameter (e.g. Move with the Vector2 axis value).
}

[Serializable]
public class InputToAbilityMapping {
  // Note: ActionRef seems to refer directly to an asset on disk, not an instance that is tied to a particular Inputs instance.
  // This means we can't access it directly in order to react to button presses, because that won't be restricted to the Inputs
  // device list. To fix this, we lookup and cache the Action instance referred to by ActionRef.
  public InputActionReference ActionRef;
  [NonSerialized] public InputAction Action;
  public InputActionType Type;
  public Ability Ability;
}

// Maps InputActions to Abilities.
[DefaultExecutionOrder(-1)]
public class InputMappings : MonoBehaviour {
  [SerializeField] Timeval BufferDuration = Timeval.FromTicks(6);
  [SerializeField] AbilityManager AbilityManager;
  [SerializeField] InputToAbilityMapping Move;
  [SerializeField] List<InputToAbilityMapping> Mappings;

  Dictionary<(InputAction, InputActionType), int> Buffer = new();

  Inputs Inputs;

  public void AssignDevices(InputDevice[] devices) {
    Inputs.devices = devices;
  }

  void Awake() {
    Inputs = new();
    Inputs.Enable();
    Move.Action = Inputs.FindAction(Move.ActionRef.name);
    Move.Action.Enable();
    Mappings.ForEach(m => {
      m.Action = Inputs.FindAction(m.ActionRef.name);
      m.Action.Enable();
    });
  }

  void OnDestroy() {
    Inputs.Disable();
    Inputs.Dispose();
  }

  void FixedUpdate() {
    AbilityManager.TryRun(Move.Ability, Move.Action.ReadValue<Vector2>());

    // TODO: This is just a skeleton. Features to add/consider:
    // - buffering
    Mappings.ForEach(m => {
      if (CheckInputBuffer(m)) {
        if (AbilityManager.TryRun(m.Ability))
          ConsumeInputBuffer(m);
      }
    });

    Timeval.TickCount++;  // TODO: Doesn't belong here
  }

  bool CheckInputBuffer(InputToAbilityMapping m) {
    Func<bool> activated = m.Type switch {
      InputActionType.JustDown => m.Action.WasPressedThisFrame,
      InputActionType.JustUp => m.Action.WasReleasedThisFrame,
      _ => null
    };
    if (activated())
      Buffer[(m.Action, m.Type)] = Timeval.TickCount;
    return (Buffer.TryGetValue((m.Action, m.Type), out int tickCount) && Timeval.TickCount - tickCount <= BufferDuration.Ticks);
  }

  void ConsumeInputBuffer(InputToAbilityMapping m) {
    Buffer.Remove((m.Action, m.Type));
  }
}