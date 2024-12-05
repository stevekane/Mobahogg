using UnityEngine;

namespace State {
  public class MaxFallSpeed : AttributeFloat {
    [field:SerializeField]
    public override float Base { get; set; } = -10;
  }
}