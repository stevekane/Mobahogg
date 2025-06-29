using UnityEngine;

class Tongue : MonoBehaviour
{
  public static void AlignCapsuleBetweenPoints(CapsuleCollider capsuleCollider, Vector3 pointA, Vector3 pointB)
  {
    var transform = capsuleCollider.transform;
    var direction = pointB - pointA;
    var distance = direction.magnitude;
    var midPoint = (pointA + pointB) * 0.5f;
    transform.position = midPoint;
    transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
    capsuleCollider.direction = 1; // 0 = X, 1 = Y, 2 = Z (default is Y)
    capsuleCollider.height = distance;
    capsuleCollider.height = Mathf.Max(capsuleCollider.height, 2f * capsuleCollider.radius);
  }

  [SerializeField] Timeval TimeSinceLastDamageBeforeHealing = Timeval.FromSeconds(2);
  [SerializeField] Timeval HealTickPeriod = Timeval.FromSeconds(0.5f);
  [SerializeField] LineRenderer LineRenderer;
  [SerializeField] CapsuleCollider Collider;
  [SerializeField] float WaveLength = 10;
  [SerializeField] float WaveSpeed = 10;
  [SerializeField] float AmplitudeDecayRate = 0.5f;

  float Offset = 0;
  float Amplitude = 0;
  int Health = 3;

  public bool IsDead => Health <= 0;

  public void SetHealth(int health) {
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

  public void SetTongueEnd(Vector3 end)
  {
    AlignCapsuleBetweenPoints(Collider, end, transform.position);
    var start = transform.position;
    var wireVector = end - start;
    var wireDirection = wireVector.normalized;
    var wireLength = wireVector.magnitude;
    LineRenderer.SetPosition(0, start);
    LineRenderer.SetPosition(LineRenderer.positionCount-1, end);
    for (var i = 1; i < LineRenderer.positionCount-1; i++)
    {
      var fraction = (float)i / (LineRenderer.positionCount - 1);
      var x = fraction * wireLength;
      var z = Amplitude * Mathf.Sin(2 * Mathf.PI * (x / WaveLength + Offset));
      var vibrationPosition = fraction * wireVector + z * Vector3.Cross(Vector3.up, wireDirection);
      LineRenderer.SetPosition(i, start + vibrationPosition);
    }
    Amplitude = Mathf.Max(0, Amplitude - Time.deltaTime * AmplitudeDecayRate);
    Offset += Time.deltaTime * WaveSpeed;
  }
}