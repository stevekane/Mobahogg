using UnityEngine;
using TMPro;

public class PostBattleOverlay : MonoBehaviour {
  [SerializeField] TextMeshProUGUI WinningTeamText;

  public void SetWinner(string resultText) {
    WinningTeamText.text = resultText;
  }
}