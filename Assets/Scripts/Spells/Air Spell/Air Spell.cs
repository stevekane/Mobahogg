using UnityEngine;

public class AirSpell : Spell {
  public override void Cast(Vector3 position, Quaternion rotation, Player owner) {
    Debug.Log("Air Spell");
  }

  void FixedUpdate() {
    Destroy(gameObject, 2);
  }
}
