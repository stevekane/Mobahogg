using UnityEngine;
using TMPro;

public class MatchSetupPlayerGridCard : MonoBehaviour {
  [SerializeField] GameObject NoControllerElement;
  [SerializeField] GameObject NotJoinedElement;
  [SerializeField] GameObject JoinedElement;
  [SerializeField] GameObject ReadyElement;
  [SerializeField] GameObject Team1Element;
  [SerializeField] GameObject Team2Element;
  [SerializeField] TextMeshProUGUI NameText;

  public void Render(PotentialPlayer player) {
    NoControllerElement.SetActive(player.State == PotentialPlayerState.Disconnected);
    NotJoinedElement.SetActive(player.State == PotentialPlayerState.Connected);
    JoinedElement.SetActive(player.State == PotentialPlayerState.Joined || player.State == PotentialPlayerState.Ready);
    ReadyElement.SetActive(player.State == PotentialPlayerState.Ready);
    Team1Element.SetActive(player.TeamType == TeamType.Turtles);
    Team2Element.SetActive(player.TeamType == TeamType.Robots);
    NameText.text = player.Name;
  }
}