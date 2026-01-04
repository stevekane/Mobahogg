using UnityEngine.UI;
using UnityEngine;

class LoadBattleOverlay : MonoBehaviour
{
  [SerializeField] Slider ProgressBar;

  public void SetCompletionFraction(float interpolant)
  {
    ProgressBar.value = interpolant;
  }
}