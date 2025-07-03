using System.Collections;
using UnityEngine;

public class WireWaveController : MonoBehaviour
{
  [Header("Child References")]
  [SerializeField] Transform WireStart;
  [SerializeField] Transform WireEnd;
  [SerializeField] Renderer[] Renderers;

  [Header("Resources")]
  [SerializeField] Material Material;

  [Header("Wire Vibration")]
  public float AmplitudeDecayRate = 1f;
  public float WaveLength = 1f;
  public float WaveSpeed = 2f;
  public float DampedOscillationFraction = 0.25f;
  public float EmissionIntensityMultiplier = 3;

  float Amplitude;
  float Offset;
  Material _mat;

  void Awake()
  {
    _mat = new Material(Material);
    foreach (var renderer in Renderers)
    {
      renderer.sharedMaterial = _mat;
    }
  }

  IEnumerator Start()
  {
    while (true)
    {
      Impulse(1);
      yield return new WaitForSeconds(3);
    }
  }

  void Oestroy()
  {
    Destroy(_mat);
  }

  void Update()
  {
    if (WireEnd == null) return;

    _mat.SetVector("_WireStart", WireStart.position);
    _mat.SetVector("_WireEnd", WireEnd.position);
    _mat.SetFloat("_WaveAmplitude", Amplitude);
    _mat.SetFloat("_WaveLength", WaveLength);
    _mat.SetFloat("_WaveSpeed", WaveSpeed);
    _mat.SetFloat("_Offset", Offset);
    _mat.SetFloat("_DampedOscillationFraction", DampedOscillationFraction);
    _mat.SetColor("_Emission", Amplitude * EmissionIntensityMultiplier * Color.white);
    Offset += Time.deltaTime * WaveSpeed;
    Amplitude = Mathf.Max(0, Amplitude - Time.deltaTime * AmplitudeDecayRate);
  }

  public void Impulse(float strength = 1)
  {
    Amplitude = Mathf.Max(Amplitude, strength);
  }
}