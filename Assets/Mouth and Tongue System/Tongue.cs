using UnityEngine;

public class Tongue : MonoBehaviour
{
  [Header("Child References")]
  [SerializeField] CapsuleCollider Collider;
  [SerializeField] PlasmaArc PlasmaArc;
  [SerializeField] Renderer[] DeformableWireRenderers;

  [Header("Resources")]
  [SerializeField] Material DeformableWireMaterial;

  [SerializeField] float WaveLength = 10;
  [SerializeField] float WaveSpeed = 10;
  [SerializeField] float AmplitudeDecayRate = 0.5f;
  [SerializeField] float _DampedOscillationFraction = 0.25f;

  float Offset = 0;
  float Amplitude = 0;
  Material DeformableWireRendererMaterialInstance;

  public void Vibrate(float amplitude)
  {
    Amplitude = amplitude;
  }

  void Start()
  {
    PlasmaArc.gameObject.SetActive(true);
    DeformableWireRendererMaterialInstance = new Material(DeformableWireMaterial);
    foreach (var renderer in DeformableWireRenderers)
    {
      renderer.sharedMaterial = DeformableWireRendererMaterialInstance;
    }
  }

  void OnDestroy()
  {
    Destroy(DeformableWireRendererMaterialInstance);
  }

  public void SetTongueEnd(Vector3 end)
  {
    var start = transform.position;
    var wireVector = end - start;
    var wireDirection = wireVector.normalized;
    var wireLength = wireVector.magnitude;
    var vibrationDirection = Vector3.Cross(Vector3.up, wireDirection);

    // Collider
    Collider.AlignCapsuleBetweenPoints(start, end);

    // Deformable Wires
    DeformableWireRendererMaterialInstance.SetVector("_WireStart", start);
    DeformableWireRendererMaterialInstance.SetVector("_WireEnd", end);
    DeformableWireRendererMaterialInstance.SetFloat("_WaveAmplitude", Amplitude);
    DeformableWireRendererMaterialInstance.SetFloat("_WaveLength", WaveLength);
    DeformableWireRendererMaterialInstance.SetFloat("_WaveSpeed", WaveSpeed);
    DeformableWireRendererMaterialInstance.SetFloat("_Offset", Offset);
    DeformableWireRendererMaterialInstance.SetFloat("_DampedOscillationFraction", _DampedOscillationFraction);

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