using UnityEngine;
using Unity.AI.Navigation;

[RequireComponent(typeof(NavMeshSurface))]
class DynamicNavMesh : MonoBehaviour
{
  void FixedUpdate()
  {
    GetComponent<NavMeshSurface>().BuildNavMesh();
  }
}