using UnityEngine;

public class SpellCasterTEST : MonoBehaviour {
  [SerializeField] int PortIndex;
  [SerializeField] EarthSpell EarthSpell;
  [SerializeField] FireSpell FireSpell;
  [SerializeField] AirSpell AirSpell;
  [SerializeField] WaterSpell WaterSpell;

  void Start() {
    Debug.Log("TEST CASTER START");
    InputRouter.Instance?.TryListenValue("Move", PortIndex, HandleTurn);
    InputRouter.Instance?.TryListenButton("Spin", ButtonState.JustDown, PortIndex, HandleEarth);
    InputRouter.Instance?.TryListenButton("Ultimate", ButtonState.JustDown, PortIndex, HandleFire);
    InputRouter.Instance?.TryListenButton("Jump", ButtonState.JustDown, PortIndex, HandleAir);
    InputRouter.Instance?.TryListenButton("Attack", ButtonState.JustDown, PortIndex, HandleWater);
  }

  void OnDestroy() {
    Debug.Log("TEST CASTER DESTROY");
    InputRouter.Instance?.TryUnlistenValue("Move", PortIndex, HandleTurn);
    InputRouter.Instance?.TryUnlistenButton("Spin", ButtonState.JustDown, PortIndex, HandleEarth);
    InputRouter.Instance?.TryUnlistenButton("Ultimate", ButtonState.JustDown, PortIndex, HandleFire);
    InputRouter.Instance?.TryUnlistenButton("Jump", ButtonState.JustDown, PortIndex, HandleAir);
    InputRouter.Instance?.TryUnlistenButton("Attack", ButtonState.JustDown, PortIndex, HandleWater);
  }

  public void HandleEarth(PortButtonState buttonState) {
    Instantiate(EarthSpell, transform.position, transform.rotation);
    InputRouter.Instance.ConsumeButton("Spin", PortIndex);
  }

  public void HandleFire(PortButtonState buttonState) {
    Instantiate(FireSpell, transform.position, transform.rotation);
    InputRouter.Instance.ConsumeButton("Ultimate", PortIndex);
  }

  public void HandleAir(PortButtonState buttonState) {
    Instantiate(AirSpell, transform.position,transform.rotation);
    InputRouter.Instance.ConsumeButton("Jump", PortIndex);
  }

  public void HandleWater(PortButtonState buttonState) {
    Instantiate(WaterSpell, transform.position,transform.rotation);
    InputRouter.Instance.ConsumeButton("Attack", PortIndex);
  }

  public void HandleTurn(PortValue action) {
    var input = action.Value;
    var direction = new Vector3(input.x, 0, input.y);
    if (direction.magnitude > 0)
      transform.rotation = Quaternion.LookRotation(direction);
  }
}