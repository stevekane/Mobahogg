using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {
  [SerializeField] SceneAsset MatchSceneAsset;

  // listen on all ports for StartMatch
  void Awake() {
    for (var i = 0; i < InputRouter.MAX_PORT_COUNT; i++) {
      InputRouter.Instance.TryListenButton("TitleScreen/StartMatch", ButtonState.JustDown, i, StartMatch);
    }
  }

  void OnDestroy() {
    for (var i = 0; i < InputRouter.MAX_PORT_COUNT; i++) {
      InputRouter.Instance.TryUnlistenButton("TitleScreen/StartMatch", ButtonState.JustDown, i, StartMatch);
    }
  }

  void StartMatch(PortButtonState buttonState) {
    Debug.Log($"Match started from {buttonState.PortIndex}");
    SceneManager.LoadScene(MatchSceneAsset.name);
  }
}