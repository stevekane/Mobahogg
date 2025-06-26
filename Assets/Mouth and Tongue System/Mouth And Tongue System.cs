using UnityEngine;

/*
Here are the key parts:

Mouth
Tongue Cannon
Tongue
Claw
Sphere

Mouth closes when tongue dies
Mouth opens to fire new tongue
Tongue cannon just origin of the tongue
Tongue cannon fires the claw
Tongue is strung between claw and tongue origin
Claw tracks the sphere and converges on it as its lateral distance goes to zero
Tongue has rigidity and tries to stay straight
Tongues exert force on the sphere
*/

public class MouthAndTongueSystem : MonoBehaviour
{
  [SerializeField] Sphere Sphere;
  [SerializeField] Mouth LeftMouth;
  [SerializeField] Mouth RightMouth;

  void FixedUpdate()
  {
    var toLeftMouth = (LeftMouth.transform.position - Sphere.transform.position).normalized;
    var toRightMouth = (RightMouth.transform.position - Sphere.transform.position).normalized;
    LeftMouth.Tongue.SetTongueEnd(Sphere.transform.position + toLeftMouth*Sphere.Radius);
    RightMouth.Tongue.SetTongueEnd(Sphere.transform.position + toRightMouth*Sphere.Radius);
  }
}