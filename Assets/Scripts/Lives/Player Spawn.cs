using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
[RequireComponent(typeof(Team))]
public class PlayerSpawn : MonoBehaviour {
  void Start() => SpawnManager.Active.Add(this);
  void OnDestroy() => SpawnManager.Active.Add(this);
}