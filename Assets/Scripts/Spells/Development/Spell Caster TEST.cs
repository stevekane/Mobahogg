using UnityEngine;

public class SpellCasterTEST : MonoBehaviour {
  [SerializeField] int PortIndex;
  [SerializeField] EarthSpell EarthSpell;
  [SerializeField] FireSpell FireSpell;
  [SerializeField] AirSpell AirSpell;
  [SerializeField] WaterSpell WaterSpell;

  void Start() {
    Debug.Log("TEST CASTER START");
    InputRouter.Instance?.TryListen("Move", PortIndex, HandleTurn);
    InputRouter.Instance?.TryListen("Spin", PortIndex, HandleEarth);
    InputRouter.Instance?.TryListen("Cast Spell", PortIndex, HandleFire);
    InputRouter.Instance?.TryListen("Jump", PortIndex, HandleAir);
    InputRouter.Instance?.TryListen("Attack", PortIndex, HandleWater);
  }

  void OnDestroy() {
    Debug.Log("TEST CASTER DESTROY");
    InputRouter.Instance?.TryUnlisten("Move", PortIndex, HandleTurn);
    InputRouter.Instance?.TryUnlisten("Spin", PortIndex, HandleEarth);
    InputRouter.Instance?.TryUnlisten("Cast Spell", PortIndex, HandleFire);
    InputRouter.Instance?.TryUnlisten("Jump", PortIndex, HandleAir);
    InputRouter.Instance?.TryUnlisten("Attack", PortIndex, HandleWater);
  }

  public void HandleEarth(PortAction action) {
    Instantiate(EarthSpell).Cast(transform.position,transform.rotation,null);
  }

  public void HandleFire(PortAction action) {
    Instantiate(FireSpell).Cast(transform.position,transform.rotation,null);
  }

  public void HandleAir(PortAction action) {
    Instantiate(AirSpell).Cast(transform.position,transform.rotation,null);
  }

  public void HandleWater(PortAction action) {
    Instantiate(WaterSpell).Cast(transform.position,transform.rotation,null);
  }

  public void HandleTurn(PortAction action) {
    var input = action.Value;
    var direction = new Vector3(input.x, 0, input.y);
    if (direction.magnitude > 0)
      transform.rotation = Quaternion.LookRotation(direction);
  }
}