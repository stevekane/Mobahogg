using UnityEngine;

namespace State {
  public class MoveDirection : AttributeVector3 {
    public override Vector3 Base { get; set; } = Vector3.zero;
  }
}