using UnityEngine;

class ExplodingFragments : MonoBehaviour
{
  [SerializeField] float CameraShakeIntensity = 10;
  [SerializeField] float MinExplosionForce = 10;
  [SerializeField] float MaxExplosionForce = 25;
  [SerializeField] float MaxTorque = 10;
  [SerializeField] Rigidbody[] Bodies;

  void Start()
  {
    CameraManager.Instance.Shake(CameraShakeIntensity);
    foreach (var body in Bodies)
    {
      var x = Random.Range(-1f, 1f);
      var z = Random.Range(-1f, 1f);
      var y = Random.Range(0.25f, 1f);
      var direction = new Vector3(x, y, z).normalized;
      body.AddForce(Random.Range(MinExplosionForce, MaxExplosionForce) * direction, ForceMode.Impulse);
      body.AddTorque(Random.Range(-MaxTorque, MaxTorque) * Random.onUnitSphere, ForceMode.Impulse);
    }
  }
}