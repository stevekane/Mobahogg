using UnityEngine;

public abstract class Spell : MonoBehaviour {
  public abstract void Cast(Vector3 position, Quaternion rotation, Player owner);
}