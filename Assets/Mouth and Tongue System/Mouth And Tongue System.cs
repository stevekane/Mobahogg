using System.Collections;
using UnityEngine;

public class MouthAndTongueSystem : MonoBehaviour
{
  [SerializeField] Sphere Sphere;
  [SerializeField] Mouth LeftMouth;
  [SerializeField] Mouth RightMouth;
  [SerializeField] Timeval PrefireDuration = Timeval.FromSeconds(1);

  void Start()
  {
    StartCoroutine(InitializationAnimation());
  }

  IEnumerator InitializationAnimation() {
    LeftMouth.Close();
    RightMouth.Close();
    yield return new WaitForSeconds(PrefireDuration.Seconds);
    LeftMouth.Open();
    RightMouth.Open();
    yield return new WaitForSeconds(PrefireDuration.Seconds);
    LeftMouth.Fire(Sphere);
    RightMouth.Fire(Sphere);
  }

  void FixedUpdate()
  {
    var toLeftMouth = (LeftMouth.transform.position - Sphere.transform.position).normalized;
    var toRightMouth = (RightMouth.transform.position - Sphere.transform.position).normalized;
    var sphereRB = Sphere.GetComponent<Rigidbody>();
    sphereRB.AddForce(LeftMouth.Tongue.PullingStrength * toLeftMouth, ForceMode.Force);
    sphereRB.AddForce(RightMouth.Tongue.PullingStrength * toRightMouth, ForceMode.Force);
  }
}