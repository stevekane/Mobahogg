using UnityEngine;

class Tongue : MonoBehaviour
{
  public static int MAX_COLLIDERS = 64;
  public static float TONGUE_RADIUS = 0.2f;

  [SerializeField] LayerMask ColliderLayerMask;

  Vector3 End;

  SphereCollider[] Colliders = new SphereCollider[0];
  LineRenderer LineRenderer;

  void Awake()
  {
    Colliders = new SphereCollider[MAX_COLLIDERS];
    for (var i = 0; i < MAX_COLLIDERS; i++)
    {
      Colliders[i] = new GameObject().AddComponent<SphereCollider>();
      Colliders[i].radius = TONGUE_RADIUS;
      Colliders[i].transform.SetParent(transform);
      Colliders[i].gameObject.layer = Mathf.RoundToInt(Mathf.Log(ColliderLayerMask.value, 2));
    }
    LineRenderer = gameObject.AddComponent<LineRenderer>();
    LineRenderer.startWidth = TONGUE_RADIUS;
    LineRenderer.endWidth = TONGUE_RADIUS;
  }

  public void SetTongueEnd(Vector3 end)
  {
    End = end;
    var tongueLength = Vector3.Distance(End, transform.position);
    var colliderCount = Mathf.Clamp(Mathf.FloorToInt(tongueLength), 0, MAX_COLLIDERS);
    var start = transform.position;
    for (var i = 0; i < colliderCount; i++)
    {
      var interpolant = (float)i / (colliderCount - 1);
      Colliders[i].transform.position = Vector3.Lerp(End, start, interpolant);
      Colliders[i].enabled = true;
    }
    for (var i = colliderCount; i < MAX_COLLIDERS; i++)
    {
      Colliders[i].enabled = false;
    }
    LineRenderer.SetPosition(0, transform.position);
    LineRenderer.SetPosition(1, End);
  }

  void OnDrawGizmos()
  {
    for (var i = 0; i < Colliders.Length; i++)
    {
      if (!Colliders[i].enabled) break;
      Gizmos.DrawWireSphere(Colliders[i].transform.position, 1);
    }
  }
}