using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PreBattleOverlay : MonoBehaviour {
  [SerializeField] TextMeshProUGUI NameText;
  [SerializeField] TextMeshProUGUI CountdownText;
  [SerializeField] Image[] BattleFieldMapCells;

  public void SetName(string name) {
    NameText.text = name;
  }

  public void SetCountdown(float raw) {
    CountdownText.text = Mathf.CeilToInt(raw).ToString();
  }

  public void SetBattleIndex(int activeBattleIndex, int count) {
    for (var i = 0; i < BattleFieldMapCells.Length; i++) {
      BattleFieldMapCells[i].gameObject.SetActive(i < count);
    }
    for (var i = 0; i < BattleFieldMapCells.Length; i++) {
      BattleFieldMapCells[i].color = i == activeBattleIndex ? Color.white : Color.gray;
    }
  }
}