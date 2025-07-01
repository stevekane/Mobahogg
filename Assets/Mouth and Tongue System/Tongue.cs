using UnityEngine;

public class Tongue : MonoBehaviour
{
  [Header("Child References")]
  [SerializeField] LineRenderer LineRenderer;
  [SerializeField] PlasmaArc PlasmaArc;
  [SerializeField] CapsuleCollider Collider;

  [SerializeField] Timeval TimeSinceLastDamageBeforeHealing = Timeval.FromSeconds(2);
  [SerializeField] Timeval HealTickPeriod = Timeval.FromSeconds(0.5f);
  [SerializeField] float WaveLength = 10;
  [SerializeField] float WaveSpeed = 10;
  [SerializeField] float AmplitudeDecayRate = 0.5f;

  float Offset = 0;
  float Amplitude = 0;
  int Health = 3;

  public bool IsDead => Health <= 0;

  public void SetHealth(int health)
  {
    Health = health;
  }

  public void Damage(int damage)
  {
    Health -= damage;
  }

  public void Vibrate(float amplitude)
  {
    Amplitude = amplitude;
  }

  void Start()
  {
    PlasmaArc.gameObject.SetActive(true);
  }

  public void SetTongueEnd(Vector3 end)
  {
    var start = transform.position;
    var wireVector = end - start;
    var wireDirection = wireVector.normalized;
    var wireLength = wireVector.magnitude;
    var vibrationDirection = Vector3.Cross(Vector3.up, wireDirection);
    Collider.AlignCapsuleBetweenPoints(start, end);
    LineRenderer.SetPosition(0, start);
    LineRenderer.SetPosition(LineRenderer.positionCount - 1, end);
    for (var i = 1; i < LineRenderer.positionCount - 1; i++)
    {
      var fraction = (float)i / (LineRenderer.positionCount - 1);
      var x = fraction * wireLength;
      var z = Amplitude * Mathf.Sin(2 * Mathf.PI * (x / WaveLength + Offset));
      var vibrationPosition = fraction * wireVector + z * vibrationDirection;
      LineRenderer.SetPosition(i, start + vibrationPosition);
    }
    var p0 = start;
    var p1 = start + 0.2f * wireVector + Vector3.up + Amplitude * vibrationDirection;
    var p2 = start + 0.8f * wireVector + Vector3.up - Amplitude * vibrationDirection;
    var p3 = end;
    PlasmaArc.SetPoints(p0, p1, p2, p3);
    Amplitude = Mathf.Max(0, Amplitude - Time.deltaTime * AmplitudeDecayRate);
    Offset += Time.deltaTime * WaveSpeed;
  }
}