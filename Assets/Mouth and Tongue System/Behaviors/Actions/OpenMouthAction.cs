using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Mouth Closes", story: "[Mouth] opens", category: "Action", id: "bf8cfdaff7dc66d96a8fa5466b431ecd")]
public partial class OpenMouthAction : Action
{
  [SerializeReference] public BlackboardVariable<Mouth> Mouth;

  protected override Status OnStart()
  {
    Debug.Log("Closed");
    return Status.Running;
  }

  protected override Status OnUpdate()
  {
    return Status.Success;
  }

  protected override void OnEnd()
  {
  }
}
