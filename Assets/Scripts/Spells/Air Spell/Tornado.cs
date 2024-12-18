using UnityEngine;

public class Tornado : MonoBehaviour {
  [SerializeField] AirSpellSettings Settings;

  Collider[] Colliders = new Collider[256];

  void FixedUpdate() {
    var count = Physics.OverlapSphereNonAlloc(
        transform.position,
        Settings.TornadoOuterRadius,
        Colliders);
    for (var i = 0; i < count; i++) {
      var collider = Colliders[i];
      var controller = collider.GetComponent<KCharacterController>();
      var player = collider.GetComponent<Player>();
      if (controller && player) {
        var delta = (transform.position-controller.transform.position).XZ();
        var distance = delta.magnitude;
        var direction = delta.normalized;
        controller.Acceleration += Settings.TornadoSuction(distance) * direction;
      }
    }
  }
}