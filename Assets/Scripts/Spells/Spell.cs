using UnityEngine;

public abstract class Spell : MonoBehaviour {
  public GameObject SpellStaffChargePrefab;
  public abstract void Cast(Vector3 position, Quaternion rotation, Player owner);
}