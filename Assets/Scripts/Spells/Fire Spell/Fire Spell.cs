using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// N.B. Owner reference is volatile. Don't forget to check
public class FireSpell : Spell {
  [SerializeField] GameObject FireballPrefab;
  [SerializeField] float SpreadAngle = 60;
  [SerializeField] float Speed = 10;
  [SerializeField] float MinSpeedScale = 0.25f;
  [SerializeField] float MaxSpeedScale = 1.0f;
  [SerializeField] int Count = 5;

  List<GameObject> Fireballs = new(5);

  public override void Cast(Vector3 position, Quaternion rotation, Player owner) {
    var forward = rotation * Vector3.forward;
    var left = Quaternion.LookRotation(Quaternion.Euler(0, -SpreadAngle / 2, 0) * forward);
    var right = Quaternion.LookRotation(Quaternion.Euler(0, SpreadAngle / 2, 0) * forward);
    for (var i = 0; i < Count; i++) {
      var direction = Quaternion.Slerp(left, right, i / (float)(Count-1)) * Vector3.forward;
      var angleFromForward = Vector3.Angle(forward, direction);
      var angleAlignment = angleFromForward / (SpreadAngle / 2);
      var speedScale = Mathf.Lerp(MaxSpeedScale, MinSpeedScale, angleAlignment);
      var fireball = Instantiate(FireballPrefab, position, Quaternion.LookRotation(direction), transform);
      fireball.GetComponent<Rigidbody>().AddForce(speedScale * Speed * direction, ForceMode.VelocityChange);
      Fireballs.Add(fireball);
    }
  }

  void FixedUpdate() {
    if (Fireballs.All(f => f == null)) {
      Destroy(gameObject);
    }
  }
}