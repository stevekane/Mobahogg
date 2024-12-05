using UnityEngine;

namespace State {
  public class AimDirection : AttributeVector3 {
    public override Vector3 Base { get; set; } = Vector3.zero;
  }
}