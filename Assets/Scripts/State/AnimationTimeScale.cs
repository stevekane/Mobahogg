using UnityEngine;

namespace State {
  public class AnimationTimeScale : AttributeFloat {
    [field:SerializeField]
    public override float Base { get; set; } = 1;
  }
}