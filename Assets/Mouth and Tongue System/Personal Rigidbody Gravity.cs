using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
class PersonalRigidbodyGravity : MonoBehaviour
{
  public float Gravity = -50;

  void FixedUpdate()
  {
    GetComponent<Rigidbody>().AddForce(Gravity * Vector3.up, ForceMode.Acceleration);
  }
}