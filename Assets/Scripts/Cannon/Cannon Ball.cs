using UnityEngine;

public class CannonBall : MonoBehaviour {
  [SerializeField] GameObject ExplosionPrefab;
  [SerializeField] float BounceCameraShakeIntensity = 0.25f;
  [SerializeField] float ExplosionCameraShakeIntensity = 1;
  [SerializeField] float ExplosionRadius = 5;
  [SerializeField] int HealthChange = -2;
  [SerializeField] int Knockback = 10;

  Collider[] Colliders = new Collider[32];

  void OnCollisionEnter(Collision collision) {
    if (!collision.gameObject.CompareTag("Ground")) {
      var overlapCount = Physics.OverlapSphereNonAlloc(transform.position, ExplosionRadius, Colliders);
      for (var i = 0; i < overlapCount; i++) {
        var collider = Colliders[i];
        if (collider.TryGetComponent(out SpellAffected spellAffected)) {
          spellAffected.ChangeHealth(HealthChange);
          spellAffected.Knockback(Knockback * transform.forward.XZ());
        }
      }
      CameraManager.Instance.Shake(ExplosionCameraShakeIntensity);
      Destroy(Instantiate(ExplosionPrefab, transform.position, transform.rotation, null), 3);
      Destroy(gameObject);
    } else {
      CameraManager.Instance.Shake(BounceCameraShakeIntensity);
    }
  }
}