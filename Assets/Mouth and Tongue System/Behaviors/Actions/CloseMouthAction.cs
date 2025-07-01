using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Close Mouth", story: "[Mouth] closes", category: "Action", id: "00df0c8b4674f4649ca4a686f1d2b4bb")]
public partial class CloseMouthAction : Action
{
  [SerializeReference] public BlackboardVariable<Mouth> Mouth;
  Vector3 InitialPosition;
  Quaternion InitialRotation;

  protected override Status OnStart()
  {
    var mouth = Mouth.Value;
    mouth.Tongue.gameObject.SetActive(false);
    mouth.Claw.gameObject.SetActive(false);
    var shatteredClawVFX = GameObject.Instantiate(
      original: mouth.ShatteredClawPrefab,
      position: mouth.Claw.transform.position,
      rotation: mouth.Claw.transform.rotation);
    GameObject.Destroy(shatteredClawVFX.gameObject, 3);
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