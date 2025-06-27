using Melee;
using UnityEngine;

class Sphere : MonoBehaviour
{
  public GameObject ImpactVFXPrefab;
  public float ImpactStrength = 1000;
  public float Radius = 1.5f;

  public void OnHurt(MeleeAttackEvent meleeAttackEvent)
  {
    GetComponent<Rigidbody>().AddForce(
      ImpactStrength * meleeAttackEvent.ToVictim.XZ().normalized,
      ForceMode.Impulse);
  }
}