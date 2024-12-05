using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {
  [SerializeField] SceneAsset MatchSceneAsset;

  // listen on all ports for StartMatch
  void Awake() {
    for (var i = 0; i < InputRouter.MAX_PORT_COUNT; i++) {
      InputRouter.Instance.TryListen("TitleScreen/StartMatch", i, StartMatch);
    }
  }

  void OnDestroy() {
    for (var i = 0; i < InputRouter.MAX_PORT_COUNT; i++) {
      InputRouter.Instance.TryUnlisten("TitleScreen/StartMatch", i, StartMatch);
    }
  }

  void StartMatch(PortAction portAction) {
    Debug.Log($"Match started from {portAction.PortIndex}");
    SceneManager.LoadScene(MatchSceneAsset.name);
  }
}