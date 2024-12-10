using UnityEngine;

// N.B. Owner reference is volatile. Don't forget to check
public class FireSpell : Spell {
  [SerializeField] GameObject FireballPrefab;

  GameObject FireballInstance;

  public override void Cast(Vector3 position, Quaternion rotation, Player owner) {
    FireballInstance = Instantiate(FireballPrefab, position, rotation, transform);
  }

  void FixedUpdate() {
    if (!FireballInstance)
      Destroy(gameObject);
  }
}