using UnityEngine;

public class Tornado : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] AirSpellSettings Settings;

  Collider[] Colliders = new Collider[256];

  private Vector3 driftDirection;
  private float currentSpeed;

  void Start() {
    driftDirection = Random.insideUnitSphere.XZ().normalized; // Random initial direction
    currentSpeed = Random.Range(Settings.MaxTornadoSpeed * 0.5f, Settings.MaxTornadoSpeed);   // Random initial speed
  }

  void FixedUpdate() {
    if (LocalClock.Frozen())
      return;

    DriftMovement(TimeManager.Instance.FixedFrame(), LocalClock.DeltaTime());
    ApplyTornadoEffects();
  }

  void DriftMovement(int seed, float dt) {
    var noiseAngle = (Mathf.PerlinNoise(seed * Settings.TornadoDriftNoiseScale, 0f) - 0.5f) * 360f; // Noise mapped to angle
    Quaternion targetRotation = Quaternion.Euler(0f, noiseAngle, 0f);
    driftDirection = Quaternion.RotateTowards(
        Quaternion.LookRotation(driftDirection),
        targetRotation,
        Settings.MaxTornadoTurningSpeed * Time.fixedDeltaTime
    ) * Vector3.forward;
    currentSpeed = Mathf.Lerp(currentSpeed, Settings.MaxTornadoSpeed * Mathf.PerlinNoise(seed * Settings.TornadoSpeedNoiseScale, 1f), 0.1f);
    // TODO: We move the parent which is the AirBall for now... I know this is sort of jank but I'm tired
    transform.parent.position += driftDirection.normalized * currentSpeed * Time.fixedDeltaTime;
  }

  void ApplyTornadoEffects() {
    var count = Physics.OverlapSphereNonAlloc(
        transform.position,
        Settings.TornadoOuterRadius,
        Colliders);

    for (var i = 0; i < count; i++) {
      var collider = Colliders[i];
      var spellAffected = collider.GetComponent<SpellAffected>();
      if (spellAffected) {
        var delta = (transform.position - collider.transform.position).XZ();
        var distance = delta.magnitude;
        var direction = delta.normalized;
        spellAffected.Push(Settings.TornadoSuction(distance) * direction);
      }
    }
  }
}