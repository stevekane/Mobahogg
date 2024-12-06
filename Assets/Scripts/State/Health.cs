using UnityEngine;

namespace State {
  public class Health : AttributeInt {
    [field:SerializeField] public override int Base { get; set; } = 9;
  }
}