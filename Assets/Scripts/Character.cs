using UnityEngine;

public class Character : MonoBehaviour {
  public float Speed = 10;

  void Update() {
    var l = Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
    var r = Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
    var u = Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
    var d = Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
    transform.position = transform.position + Speed * Time.deltaTime * new Vector3(l+r, 0, u+d);
  }
}