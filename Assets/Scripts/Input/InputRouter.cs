using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ButtonState {
  Up,
  JustDown,
  Down,
  JustUp
}

public struct PortValue {
  public readonly string ActionName;
  public readonly int PortIndex;
  public readonly Vector2 Value;
  public PortValue(string actionName, int portIndex, Vector2 value) {
    ActionName = actionName;
    PortIndex = portIndex;
    Value = value;
  }
}

public struct PortButtonState {
  public readonly string ActionName;
  public readonly int PortIndex;
  public readonly ButtonState ButtonState;
  public PortButtonState(string actionName, int portIndex, ButtonState buttonState) {
    ActionName = actionName;
    PortIndex = portIndex;
    ButtonState = buttonState;
  }
}

public class InputPort {
  readonly Inputs Inputs;
  readonly Dictionary<InputAction, int> JustDownBuffer = new();
  readonly Dictionary<InputAction, int> BufferFrameDurations = new();
  readonly Dictionary<InputAction, EventSource<PortButtonState>> Up = new();
  readonly Dictionary<InputAction, EventSource<PortButtonState>> JustDown = new();
  readonly Dictionary<InputAction, EventSource<PortButtonState>> Down = new();
  readonly Dictionary<InputAction, EventSource<PortButtonState>> JustUp = new();
  readonly Dictionary<InputAction, EventSource<PortValue>> Value = new();
  readonly Dictionary<InputAction, ButtonState> Buttons = new();
  readonly Dictionary<InputAction, Vector2> Values = new();
  readonly int PortIndex;

  public InputPort(Inputs inputs, int portIndex) {
    Inputs = inputs;
    PortIndex = portIndex;
    foreach (var actionMap in Inputs.asset.actionMaps) {
      foreach (var action in actionMap.actions) {
        if (action.type == InputActionType.Value) {
          Values.Add(action, new());
          Value.Add(action, new());
        } else if (action.type == InputActionType.Button) {
          Buttons.Add(action, new());
          Up.Add(action, new());
          JustDown.Add(action, new());
          Down.Add(action, new());
          JustUp.Add(action, new());
          BufferFrameDurations.Add(action, 12);
        }
      }
    }
  }

  Dictionary<InputAction, EventSource<PortButtonState>> MapForButtonState(ButtonState state) =>
    state switch {
      ButtonState.JustDown => JustDown,
      ButtonState.Down => Down,
      ButtonState.JustUp => JustUp,
      _ => Up
    };

  public bool TryListenButtonState(string actionName, ButtonState state, Action<PortButtonState> cb) {
    var action = Inputs.FindAction(actionName);
    var map = MapForButtonState(state);
    if (map.TryGetValue(action, out var source)) {
      source.Listen(cb);
      return true;
    } else {
      return false;
    }
  }

  public bool TryUnlistenButtonState(string actionName, ButtonState state, Action<PortButtonState> cb) {
    var action = Inputs.FindAction(actionName);
    var map = MapForButtonState(state);
    if (map.TryGetValue(action, out var source)) {
      source.Unlisten(cb);
      return true;
    } else {
      return false;
    }
  }

  public bool TryGetButtonState(string actionName, out ButtonState value) {
    return Buttons.TryGetValue(Inputs.FindAction(actionName), out value);
  }

  public void SetButtonState(string actionName, bool pressed) {
    var action = Inputs.FindAction(actionName);
    if (Buttons.TryGetValue(action, out var currentState)) {
      var nextState = (pressed, currentState) switch {
        (true, ButtonState.Up) => ButtonState.JustDown,
        (true, ButtonState.JustUp) => ButtonState.JustDown,
        (false, ButtonState.Down) => ButtonState.JustUp,
        (false, ButtonState.JustDown) => ButtonState.JustUp,
        _ => pressed ? ButtonState.Down : ButtonState.Up
      };
      Buttons[action] = nextState;
      if (nextState == ButtonState.JustDown) {
        JustDownBuffer[action] = TimeManager.Instance.FixedFrame();
      }
    } else {
      Buttons[action] = pressed ? ButtonState.Down : ButtonState.Up;
    }
  }

  public bool TrySendBufferedButtonState(string actionName) {
    var action = Inputs.FindAction(actionName);
    if (JustDown.TryGetValue(action, out var source) &&
        JustDownBuffer.TryGetValue(action, out var bufferedFrame) &&
        BufferFrameDurations.TryGetValue(action, out var bufferFrameDuration) &&
        TimeManager.Instance.FixedFrame()-bufferedFrame <= bufferFrameDuration) {
      source.Fire(new(actionName, PortIndex, ButtonState.JustDown));
      return true;
    } else {
      return false;
    }
  }

  public void ConsumeBufferedButtonState(string actionName) {
    var action = Inputs.FindAction(actionName);
    JustDownBuffer.Remove(action);
  }

  public bool TrySendButtonState(string actionName) {
    var action = Inputs.FindAction(actionName);
    if (Buttons.TryGetValue(action, out var state) &&
        MapForButtonState(state).TryGetValue(action, out var source)) {
      source.Fire(new(actionName, PortIndex, state));
      return true;
    } else {
      return false;
    }
  }

  public bool TryListenValue(string actionName, Action<PortValue> cb) {
    if (Value.TryGetValue(Inputs.FindAction(actionName), out var source)) {
      source.Listen(cb);
      return true;
    } else {
      return false;
    }
  }

  public bool TryUnlistenValue(string actionName, Action<PortValue> cb) {
    if (Value.TryGetValue(Inputs.FindAction(actionName), out var source)) {
      source.Unlisten(cb);
      return true;
    } else {
      return false;
    }
  }

  public bool TryGetValue(string actionName, out Vector2 value) {
    return Values.TryGetValue(Inputs.FindAction(actionName), out value);
  }

  public void SetValue(string actionName, Vector2 value) {
    Values[Inputs.FindAction(actionName)] = value;
  }

  public bool TrySendValue(string actionName) {
    var action = Inputs.FindAction(actionName);
    if (Value.TryGetValue(action, out var source) &&
        Values.TryGetValue(action, out var value)) {
        source.Fire(new(actionName, PortIndex, value));
        return true;
    } else {
      return false;
    }
  }
}

[DefaultExecutionOrder((int)ExecutionGroups.Input)]
public class InputRouter : SingletonBehavior<InputRouter> {
  public static int MAX_PORT_COUNT = 8;

  public readonly EventSource<int> DeviceConnected = new();
  public readonly EventSource<int> DeviceDisconnected = new();
  public readonly Dictionary<InputDevice, int> DeviceToPortMap = new();
  Inputs Inputs;
  InputPort[] InputPorts = new InputPort[MAX_PORT_COUNT];

  [Header("Debug")]
  [SerializeField] bool ShowDebug;

  [Header("Configuration")]
  [SerializeField] float StickDeadZone = 0.125f;

  public bool HasConnectedDevice(int portIndex) =>
    DeviceToPortMap.Values.Contains(portIndex);

  public bool TryListenValue(string actionName, int portIndex, Action<PortValue> cb) =>
    InputPorts[portIndex].TryListenValue(actionName, cb);

  public bool TryListenButton(string actionName, ButtonState buttonState, int portIndex, Action<PortButtonState> cb) =>
    InputPorts[portIndex].TryListenButtonState(actionName, buttonState, cb);

  public bool TryUnlistenValue(string actionName, int portIndex, Action<PortValue> cb) =>
    InputPorts[portIndex].TryUnlistenValue(actionName, cb);

  public bool TryUnlistenButton(string actionName, ButtonState buttonState, int portIndex, Action<PortButtonState> cb) =>
    InputPorts[portIndex].TryUnlistenButtonState(actionName, buttonState, cb);

  public void ConsumeButton(string actionName, int portIndex) =>
    InputPorts[portIndex].ConsumeBufferedButtonState(actionName);

  public bool TryGetValue(string actionName, int portIndex, out Vector2 value) =>
    InputPorts[portIndex].TryGetValue(actionName, out value);

  public bool TryGetButtonState(string actionName, int portIndex, out ButtonState buttonState) =>
    InputPorts[portIndex].TryGetButtonState(actionName, out buttonState);

  bool ValidDevice(InputDevice device) => device is Gamepad;

  protected override void AwakeSingleton() {
    InputSystem.devices.Where(ValidDevice).ForEach(RegisterDevice);
    InputSystem.disconnectedDevices.Where(ValidDevice).ForEach(RegisterDevice);
    InputSystem.onDeviceChange += OnDeviceChange;
    Inputs = new();
    Inputs.Enable();
    for (var i = 0; i < InputPorts.Length; i++) {
      InputPorts[i] = new(Inputs, i);
    }
  }

  void FixedUpdate() {
    foreach (var actionMap in Inputs.asset.actionMaps) {
      foreach (var action in actionMap.actions) {
        foreach (var control in action.controls) {
          if (DeviceToPortMap.TryGetValue(control.device, out var portIndex)) {
            if (action.type == InputActionType.Value) {
              var value = ReadValue(control);
              value = value.magnitude < StickDeadZone ? default : value;
              InputPorts[portIndex].SetValue(action.name, value);
              InputPorts[portIndex].TrySendValue(action.name);
            }
            if (action.type == InputActionType.Button) {
              InputPorts[portIndex].SetButtonState(action.name, control.IsPressed());
              if (!InputPorts[portIndex].TrySendBufferedButtonState(action.name)) {
                InputPorts[portIndex].TrySendButtonState(action.name);
              }
            }
          }
        }
      }
    }
  }

  unsafe Vector2 ReadValue(InputControl control) {
    byte* buffer = stackalloc byte[control.valueSizeInBytes];
    control.ReadValueIntoBuffer(buffer, control.valueSizeInBytes);
    float x = *((float*)buffer);
    float y = *((float*)(buffer + sizeof(float)));
    return new Vector2(x, y);
  }

  // scan the ports checking for the first that isn't occupied
  // TODO: This will not add the device at all if every port is occupied... maybe not desired
  void RegisterDevice(InputDevice device) {
    for (var i = 0; i < MAX_PORT_COUNT; i++) {
      if (!DeviceToPortMap.Values.Contains(i)) {
        Debug.Log($"Device Registered: {device.displayName}");
        DeviceToPortMap.Add(device, i);
        DeviceConnected.Fire(i);
        break;
      }
    }
  }

  void UnregisterDevice(InputDevice device) {
    if (DeviceToPortMap.TryGetValue(device, out int port)) {
      DeviceToPortMap.Remove(device);
      DeviceDisconnected.Fire(port);
      Debug.Log($"Device Unregistered: {device.displayName}");
    } else {
      Debug.LogError($"Unrecognized device Unregistered: {device.displayName}");
    }
  }

  void OnDeviceChange(InputDevice device, InputDeviceChange change) {
     if (ValidDevice(device)) {
      Action<InputDevice> handler = change switch {
        InputDeviceChange.Added => RegisterDevice,
        InputDeviceChange.Removed => UnregisterDevice,
        _ => OnDeviceEvent
      };
      handler(device);
     }
  }

  void OnDeviceEvent(InputDevice inputDevice) {
    Debug.Log("Default InputDeviceChange Handled");
  }

  void OnGUI() {
    if (!ShowDebug)
      return;

    GUILayout.BeginVertical("box");
    GUILayout.Label("Device to Port Map:");

    foreach (var kvp in DeviceToPortMap) {
      var portIndex = kvp.Value;
      GUILayout.Label($"Port {portIndex}: {kvp.Key.displayName} ({kvp.Key.deviceId})");

      GUILayout.BeginHorizontal();
      GUILayout.Label("Value Inputs:", GUILayout.Width(100));
      GUILayout.EndHorizontal();

      foreach (var actionMap in Inputs.asset.actionMaps) {
        foreach (var action in actionMap.actions) {
          if (action.type == InputActionType.Value && InputPorts[portIndex].TryGetValue(action.name, out var value)) {
            GUILayout.Label($"  {action.name}: {value}");
          }
        }
      }

      GUILayout.BeginHorizontal();
      GUILayout.Label("Button Inputs:", GUILayout.Width(100));
      GUILayout.EndHorizontal();

      foreach (var actionMap in Inputs.asset.actionMaps) {
        foreach (var action in actionMap.actions) {
          if (action.type == InputActionType.Button && InputPorts[portIndex].TryGetButtonState(action.name, out var buttonState)) {
            GUILayout.Label($"  {action.name}: {buttonState}");
          }
        }
      }
    }

    GUILayout.EndVertical();
  }
}