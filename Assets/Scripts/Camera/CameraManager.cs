using UnityEngine;

public class CameraManager : SingletonBehavior<CameraManager> {
  public Camera Active;
  public Vector3 originalPosition;

  float shakeIntensity = 0f;
  float shakeDecayRate = 3f;

  void FixedUpdate() {
    if (Active == null) return;

    if (shakeIntensity > 0) {
      Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
      Active.transform.localPosition = originalPosition + shakeOffset;
      shakeIntensity = Mathf.MoveTowards(shakeIntensity, 0, shakeDecayRate * Time.fixedDeltaTime);
    } else {
      Active.transform.localPosition = originalPosition;
    }
  }

  public void Shake(float intensity, float decayRate = 3f) {
    if (intensity > shakeIntensity) {
      shakeIntensity = intensity;
      shakeDecayRate = decayRate;
      if (Active != null)
        originalPosition = Active.transform.localPosition;
    }
  }
}