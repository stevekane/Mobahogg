using UnityEngine;

public class Oscillator : MonoBehaviour {
  [SerializeField] float Amplitude = 1;
  [SerializeField] float Freqency = 1;
  [SerializeField] Vector3 Axis = Vector3.up;

  Vector3 InitialPosition;

  void Start() {
    InitialPosition = transform.position;
  }

  void FixedUpdate() {
    var time = TimeManager.Instance.FixedFrame() / 60f;
    var offset = Mathf.Sin(2 * Mathf.PI * time * Freqency);
    transform.position = InitialPosition + Amplitude * offset * Axis.normalized;
  }
}