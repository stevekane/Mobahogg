using UnityEngine;

namespace State {
  public class LocalGravity : AttributeBool {
    [field:SerializeField] public override bool Default { get; set; } = true;
  }
}