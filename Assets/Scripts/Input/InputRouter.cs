using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public struct PortAction {
  public readonly int PortIndex;
  public readonly Vector2 Value;
  public PortAction(int portIndex) {
    PortIndex = portIndex;
    Value = default;
  }
  public PortAction(int portIndex, Vector2 value) {
    PortIndex = portIndex;
    Value = value;
  }
}

public class InputPort {
  readonly Inputs Inputs;
  readonly Dictionary<InputAction, EventSource<PortAction>> Pushed;
  readonly Dictionary<InputAction, Vector2> Values;
  readonly int PortIndex;

  public InputPort(Inputs inputs, int portIndex) {
    Inputs = inputs;
    Pushed = new();
    Values = new();
    PortIndex = portIndex;
    foreach (var actionMap in Inputs.asset.actionMaps) {
      foreach (var action in actionMap.actions) {
        Pushed.Add(action, new());
        Values.Add(action, new());
      }
    }
  }

  public bool TryListen(string actionName, Action<PortAction> cb) {
    if (Pushed.TryGetValue(Inputs.FindAction(actionName), out EventSource<PortAction> source)) {
      source.Listen(cb);
      return true;
    } else {
      return false;
    }
  }

  public bool TryUnlisten(string actionName, Action<PortAction> cb) {
    if (Pushed.TryGetValue(Inputs.FindAction(actionName), out EventSource<PortAction> source)) {
      source.Unlisten(cb);
      return true;
    } else {
      return false;
    }
  }

  public bool TrySetValue(string actionName, Vector2 value) {
    var action = Inputs.FindAction(actionName);
    if (Values.ContainsKey(action)) {
      Values[action] = value;
      return true;
    } else {
      return false;
    }
  }

  public bool TrySend(string actionName) {
    var action = Inputs.FindAction(actionName);
    if (Pushed.TryGetValue(action, out EventSource<PortAction> source)) {
      source.Fire(new(PortIndex, Values.GetValueOrDefault(action)));
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
  [SerializeField] float StickDeadZone = .125f;

  public bool TryListen(string actionName, int portIndex, Action<PortAction> cb) {
    return InputPorts[portIndex].TryListen(actionName, cb);
  }

  public bool TryUnlisten(string actionName, int portIndex, Action<PortAction> cb) {
    return InputPorts[portIndex].TryUnlisten(actionName, cb);
  }

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
    foreach (var actionMap in Inputs.asset.actionMaps) {
      foreach (var action in actionMap.actions) {
        if (action.type != InputActionType.Value) {
          action.performed += OnButtonAction;
        }
      }
    }
  }

  /*
  Somehow, we need to track inputs from multiple controllers in a sensible way.
  Possibly look at how it's done in the old Vapor Game?
  */
  void FixedUpdate() {
    foreach (var actionMap in Inputs.asset.actionMaps) {
      foreach (var action in actionMap.actions) {
        if (action.type == InputActionType.Value) {
          for (var i = 0; i < InputPorts.Length; i++) {
            // TODO: Read value from each device here
            InputPorts[i].TrySetValue(action.name, action.ReadValue<Vector2>());
            InputPorts[i].TrySend(action.name);
          }
        }
      }
    }
  }

  void OnButtonAction(InputAction.CallbackContext ctx) {
    if (DeviceToPortMap.TryGetValue(ctx.control.device, out int portIndex)) {
      Debug.Log($"FRAME:{Time.frameCount} FIXED:{TimeManager.Instance.FixedFrame()} {ctx.control.device} value {ctx.action.name}");
      InputPorts[portIndex].TrySend(ctx.action.name);
    }
  }

  // void OnValueAction(InputAction.CallbackContext ctx) {
  //   if (DeviceToPortMap.TryGetValue(ctx.control.device, out int portIndex)) {
  //     var a = Inputs.FindAction(ctx.action.name);
  //     var v = a.ReadValue<Vector2>();
  //     v = v.magnitude > StickDeadZone ? v : default;
  //     InputPorts[portIndex].TrySetValue(ctx.action.name, v);
  //     Debug.Log($"FRAME:{Time.frameCount} FIXED:{TimeManager.Instance.FixedFrame()} {ctx.control.device} value {v}");
  //   }
  // }

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
      GUILayout.Label($"Port {kvp.Value}: {kvp.Key.displayName} ({kvp.Key.deviceId})");
    }
    GUILayout.EndVertical();
  }
}