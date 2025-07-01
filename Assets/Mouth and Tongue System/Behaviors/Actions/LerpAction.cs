using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Lerp", story: "Move [Transform] in WorldSpace: [WorldSpace] from current position to [destination] over [Duration]", category: "Action", id: "091cd7f526b049c9d39824d4c9d7bf9f")]
public partial class LerpAction : Action
{
  [SerializeReference] public BlackboardVariable<Transform> Transform;
  [SerializeReference] public BlackboardVariable<bool> WorldSpace;
  [SerializeReference] public BlackboardVariable<Vector3> Destination;
  [SerializeReference] public BlackboardVariable<float> Duration;

  Vector3 InitialPosition;
  float Elapsed;

  protected override Status OnStart()
  {
    InitialPosition = WorldSpace
      ? Transform.Value.position
      : Transform.Value.localPosition;
    Elapsed = 0;
    return Status.Running;
  }

  protected override Status OnUpdate()
  {
    Elapsed += Time.deltaTime;
    if (WorldSpace)
    {
      Transform.Value.position = Vector3.Lerp(
        InitialPosition,
        Destination.Value,
        Elapsed / Duration.Value);
    }
    else
    {
      Transform.Value.localPosition = Vector3.Lerp(
        InitialPosition,
        Destination.Value,
        Elapsed / Duration.Value);
    }
    return Elapsed >= Duration.Value
      ? Status.Success
      : Status.Running;
  }
}