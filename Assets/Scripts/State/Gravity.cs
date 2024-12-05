using UnityEngine;

namespace State {
  public class Gravity : AttributeFloat {
    [field:SerializeField]
    public override float Base { get; set; } = -10;
  }
}