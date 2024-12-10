using UnityEngine;

public class EarthSpell : Spell {
  public override void Cast(Vector3 position, Quaternion rotation, Player owner) {
    Debug.Log("Earth Spell");
  }

  void FixedUpdate() {
    Destroy(gameObject, 2);
  }
}
