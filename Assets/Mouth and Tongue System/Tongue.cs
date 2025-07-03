using UnityEngine;

public class Tongue : MonoBehaviour
{
  [Header("Child References")]
  [SerializeField] CapsuleCollider Collider;
  [SerializeField] PlasmaArc PlasmaArc;
  [SerializeField] Renderer[] DeformableWireRenderers;

  [Header("Resources")]
  [SerializeField] Material DeformableWireMaterial;

  [SerializeField] float WaveLength = 7;
  [SerializeField] float WaveSpeed = 100;
  [SerializeField] float AmplitudeDecayRate = 3f;
  [SerializeField] float DampedOscillationFraction = 0.25f;
  [SerializeField] Color EnergizedEmissionColor = 3 * Color.white;

  float Offset = 0;
  float Amplitude = 0;

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
    var vibrationDirection = Vector3.Cross(Vector3.up, wireDirection);

    // Collider
    Collider.AlignCapsuleBetweenPoints(start, end);

    foreach (var renderer in DeformableWireRenderers)
    {
      var baseEmissionColor = renderer.material.GetColor("_BaseEmission");
      renderer.material.SetVector("_WireStart", start);
      renderer.material.SetVector("_WireEnd", end);
      renderer.material.SetFloat("_WaveAmplitude", Amplitude);
      renderer.material.SetFloat("_WaveLength", WaveLength);
      renderer.material.SetFloat("_WaveSpeed", WaveSpeed);
      renderer.material.SetFloat("_Offset", Offset);
      renderer.material.SetFloat("_DampedOscillationFraction", DampedOscillationFraction);
      renderer.material.SetColor("_Emission", Color.Lerp(baseEmissionColor, EnergizedEmissionColor, Amplitude));
    }

    // Plasma Arc
    var p0 = start;
    var p1 = start + 0.2f * wireVector + Vector3.up + Amplitude * vibrationDirection;
    var p2 = start + 0.8f * wireVector + Vector3.up - Amplitude * vibrationDirection;
    var p3 = end;
    PlasmaArc.SetPoints(p0, p1, p2, p3);

    // Update time-varying values
    Amplitude = Mathf.Max(0, Amplitude - Time.deltaTime * AmplitudeDecayRate);
    Offset += Time.deltaTime * WaveSpeed;
  }
}