using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WireWaveController : MonoBehaviour
{
  [Header("Wire Settings")]
  public Transform WireStart;
  public Transform WireEnd;
  public float AmplitudeDecayRate = 1f;
  public float WaveLength = 1f;
  public float WaveSpeed = 2f;

  public float Amplitude;

  private float Offset;
  private Material _mat;

  void Awake()
  {
    _mat = GetComponent<Renderer>().material;
  }

  IEnumerator Start()
  {
    while (true)
    {
      Impulse(1);
      yield return new WaitUntil(() => Amplitude <= 0);
      yield return new WaitForSeconds(1);
    }
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
    Offset += Time.deltaTime * WaveSpeed;
    Amplitude = Mathf.Max(0, Amplitude - Time.deltaTime * AmplitudeDecayRate);
  }

  public void Impulse(float strength = 1)
  {
    Amplitude = Mathf.Max(Amplitude, strength);
  }
}