using Melee;
using UnityEngine;

class Sphere : MonoBehaviour
{
  public GameObject ImpactVFXPrefab;
  public float ImpactStrength = 1000;
  public float Radius = 1.5f;
  public Vector3 DirectVelocity;

  public void OnHurt(MeleeAttackEvent meleeAttackEvent)
  {
    Debug.Log("You hit the sphere danny stahpppp");
  }

  void FixedUpdate() {
    var velocity = DirectVelocity;
    GetComponent<Rigidbody>().MovePosition(transform.position+Time.fixedDeltaTime * velocity);
    DirectVelocity = Vector3.zero;
  }
}