using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {
  const string BUTTON_NAME = "TitleScreen/StartMatch";

  [SerializeField] SceneAsset MatchSceneAsset;

  void Awake() {
    for (var i = 0; i < InputRouter.MAX_PORT_COUNT; i++) {
      InputRouter.Instance.TryListenButton(BUTTON_NAME, ButtonState.JustDown, i, StartMatch);
    }
  }

  void OnDestroy() {
    for (var i = 0; i < InputRouter.MAX_PORT_COUNT; i++) {
      InputRouter.Instance.TryUnlistenButton(BUTTON_NAME, ButtonState.JustDown, i, StartMatch);
    }
  }

  void StartMatch(PortButtonState buttonState) {
    SceneManager.LoadScene(MatchSceneAsset.name);
  }
}