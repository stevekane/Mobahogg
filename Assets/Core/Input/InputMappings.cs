using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputActionType {
  JustDown,
  JustUp,
  Always,  // Triggers every frame, most useful with a parameter (e.g. Move with the Vector2 axis value).
}

[Serializable]
public struct InputToAbilityMapping {
  public InputActionReference Action;
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

  void Awake() {
    Inputs = new();
    Inputs.Enable();
    Move.Action.asset.Enable();
    Mappings.ForEach(m => m.Action.asset.Enable());
  }

  void OnDestroy() {
    Inputs.Disable();
    Inputs.Dispose();
  }

  void FixedUpdate() {
    AbilityManager.TryRun(Move.Ability, Move.Action.action.ReadValue<Vector2>());

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
      InputActionType.JustDown => m.Action.action.WasPressedThisFrame,
      InputActionType.JustUp => m.Action.action.WasReleasedThisFrame,
      _ => null
    };
    if (activated())
      Buffer[(m.Action.action, m.Type)] = Timeval.TickCount;
    return (Buffer.TryGetValue((m.Action.action, m.Type), out int tickCount) && Timeval.TickCount - tickCount <= BufferDuration.Ticks);
  }

  void ConsumeInputBuffer(InputToAbilityMapping m) {
    Buffer.Remove((m.Action.action, m.Type));
  }
}