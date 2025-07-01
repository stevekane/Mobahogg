using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SlerpLocalX", story: "Rotate [Transform] on local x axis to [Degrees] degrees over [Duration]", category: "Action", id: "222a2114cac270fe7e932c68758dbb40")]
public partial class SlerpLocalXAction : Action
{
  [SerializeReference] public BlackboardVariable<Transform> Transform;
  [SerializeReference] public BlackboardVariable<float> Degrees;
  [SerializeReference] public BlackboardVariable<float> Duration;

  Quaternion InitialRotation;
  Quaternion TargetRotation;
  float Elapsed;

  protected override Status OnStart()
  {
    InitialRotation = Transform.Value.localRotation;
    TargetRotation = Quaternion.Euler(
      Degrees.Value,
      Transform.Value.localEulerAngles.y,
      Transform.Value.localEulerAngles.z);
    Elapsed = 0;
    return Status.Running;
  }

  protected override Status OnUpdate()
  {
    Elapsed += Time.deltaTime;
    Transform.Value.localRotation = Quaternion.Slerp(
      InitialRotation,
      TargetRotation,
      Elapsed / Duration.Value);
    return Elapsed >= Duration.Value
      ? Status.Success
      : Status.Running;
  }
}