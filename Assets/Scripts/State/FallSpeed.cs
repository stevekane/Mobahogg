using UnityEngine;

namespace State {
  public class FallSpeed : AttributeFloat {
    [field:SerializeField] public override float Base { get; set; } = 0;
  }
}