using State;
using TMPro;
using UnityEngine;

public class WorldSpacePlayerUI : MonoBehaviour {
  [Header("Reads From")]
  [SerializeField] Health Health;
  [SerializeField] Player Player;

  [Header("Prefab References")]
  [SerializeField] GameObject FullPipPrefab;
  [SerializeField] GameObject MissingPipPrefab;

  [Header("Child References")]
  [SerializeField] Transform PipContainer;
  [SerializeField] TMP_Text NameText;

  void Start() {
    NameText.text = Player.Name;
    OnChange();
    Health.OnChange.Listen(OnChange);
  }

  void OnDestroy() {
    Health.OnChange.Unlisten(OnChange);
  }

  void OnChange() {
    for (var i = 0; i < PipContainer.childCount; i++) {
      Destroy(PipContainer.GetChild(i).gameObject);
    }
    for (var i = 0; i < Health.MaxValue; i++) {
      Instantiate(i < Health.CurrentValue ? FullPipPrefab : MissingPipPrefab, PipContainer);
    }
  }
}