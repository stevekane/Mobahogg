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
  public void SetBattleIndex(int activeBattleIndex, int max) {
    // Set active for all cells based on max
    for (var i = 0; i < BattleFieldMapCells.Length; i++) {
      var battleIndex = i-max;
      BattleFieldMapCells[i].gameObject.SetActive(Mathf.Abs(battleIndex) <= max);
    }
    // Color the cells
    for (var i = 0; i < BattleFieldMapCells.Length; i++) {
      BattleFieldMapCells[i].color = i == activeBattleIndex+max ? Color.white : Color.gray;
    }
  }
}