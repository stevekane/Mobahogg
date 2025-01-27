using UnityEngine;

namespace State {
  public class Team : MonoBehaviour {
    public static bool SameTeam(Component c1, Component c2) {
      if (c1.TryGetComponent(out Team t1) && c2.TryGetComponent(out Team t2)) {
        return t1.TeamType == t2.TeamType;
      } else {
        return false;
      }
    }

    public TeamType TeamType;
  }
}