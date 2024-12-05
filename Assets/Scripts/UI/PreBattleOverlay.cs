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

  // N.B. This index is -Max -> Max since that more naturally expresses which team is winning
  public void SetBattleIndex(int index, int max) {
    for (var i = 0; i < BattleFieldMapCells.Length; i++) {
      BattleFieldMapCells[i].color = i == index+max ? Color.white : Color.gray;
    }
  }
}