using UnityEngine;

namespace Grass
{
  [DefaultExecutionOrder((int)ExecutionGroups.Managed)]
  [ExecuteAlways]
  public class GrassRenderer : MonoBehaviour
  {
    [SerializeField] Mesh Mesh;

    public void OnEnable()
    {
      if (Mesh)
        GrassManager.Instance.UpdateGrass(Mesh);
    }

    public void OnDisable()
    {

    }

    public void OnDestroy()
    {

    }
  }
}