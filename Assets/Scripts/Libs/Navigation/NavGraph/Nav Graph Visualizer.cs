using System.Collections.Generic;
using UnityEngine;

public enum ActiveNavGrid
{
  Cartesian,
  Hex
}

[ExecuteAlways]
public sealed class NavGraphVisualizer : MonoBehaviour
{
  [Header("Region")]
  public float CellSize = 0.5f;
  public float HalfWidth = 30f;
  public float HalfHeight = 24f;
  public Vector3 OriginOffset;

  [Header("Generate")]
  public int AreaMask = UnityEngine.AI.NavMesh.AllAreas;
  public bool AutoRebuildInEditor = true;

  [Header("Visualize")]
  public bool ShowCartesian = true;
  public bool ShowHex = true;
  public bool DrawWalk = true;
  public bool DrawVoid = false;

  [Header("Colors")]
  public Color WalkColor = new Color(0.2f, 0.9f, 0.2f, 1f);
  public Color VoidColor = new Color(0.35f, 0.35f, 0.35f, 1f);
  public Color IntersectColor = new Color(1f, 0.8f, 0.2f, 1f);

  [Header("Raycast Highlight")]
  public Transform RayTransform;
  public float RayDistance = 30f;
  public bool HighlightAllIntersected = true;
  public bool StopOnVoid = false;

  [Header("Jump Links (drawn only when selected)")]
  public ActiveNavGrid JumpLinkSource = ActiveNavGrid.Cartesian;
  public float MaxJumpDistance = 6f;
  public JumpDirections JumpDirs = JumpDirections.Octal;
  public float JumpArcHeight = 1.5f;
  public Color JumpLinkColor = new Color(0.3f, 0.7f, 1f, 1f);

  [Header("Mouse Hover (only used for selected jump-link display)")]
  public bool UseMouseHover = true;
  public float HoverPickMaxDistance = 200f;

  [Header("Style")]
  public float YOffset = 0.05f;
  public bool SolidIntersect = false;

  CartesianNavGraph _cart;
  HexagonalNavGraph _hex;
  JumpLinkField _jumpLinks;

  readonly HashSet<int> _cartRayCells = new HashSet<int>(2048);
  readonly HashSet<long> _hexRayCells = new HashSet<long>(2048);

  bool _hoverValid;
  int _hoverA, _hoverB;

  Vector3 Origin => transform.position + OriginOffset;

  void OnEnable()
  {
    EnsureBuilt(force: false);
  }

  void Start()
  {
    // In play mode, Start is a good time to ensure NavMesh is initialized.
    if (Application.isPlaying)
      EnsureBuilt(force: false);
  }

  void OnValidate()
  {
    CellSize = Mathf.Max(0.05f, CellSize);
    HalfWidth = Mathf.Max(0f, HalfWidth);
    HalfHeight = Mathf.Max(0f, HalfHeight);
    MaxJumpDistance = Mathf.Max(0f, MaxJumpDistance);
    JumpArcHeight = Mathf.Max(0f, JumpArcHeight);

    if (!Application.isPlaying && AutoRebuildInEditor)
      EnsureBuilt(force: true);
  }

  [ContextMenu("Rebuild Now")]
  public void RebuildNow()
  {
    EnsureBuilt(force: true);
  }

  void EnsureBuilt(bool force)
  {
    if (!force && _cart != null && _hex != null && _jumpLinks != null)
      return;

    _cart = NavGraphGenerators.BuildCartesianFromNavMesh(Origin, CellSize, HalfWidth, HalfHeight, AreaMask);
    _hex = NavGraphGenerators.BuildHexFromNavMesh(Origin, CellSize, HalfWidth, HalfHeight, AreaMask);
    NavGraph.Set(_cart, _hex, _cart);
    NavGraph.RebuildJumpField(MaxJumpDistance, JumpDirections.Sixteen);
    INavGrid src = JumpLinkSource == ActiveNavGrid.Cartesian ? (INavGrid)_cart : (INavGrid)_hex;
    _jumpLinks = (src != null)
      ? JumpLinkField.Build(src, MaxJumpDistance, JumpDirs)
      : null;
  }

  void OnDrawGizmos()
  {
    EnsureBuilt(force: false);

    BuildRaySets();

    if (ShowCartesian && _cart != null)
      DrawCartesianGizmos(_cart, DrawWalk, DrawVoid, YOffset, WalkColor, VoidColor, IntersectColor, _cartRayCells, SolidIntersect);

    if (ShowHex && _hex != null)
      DrawHexGizmos(_hex, DrawWalk, DrawVoid, YOffset, WalkColor, VoidColor, IntersectColor, _hexRayCells, SolidIntersect);

    DrawRayLine();
  }

  void OnDrawGizmosSelected()
  {
    // Jump arcs only when selected -> keeps scene clean and avoids hover weirdness generally.
    EnsureBuilt(force: false);

    if (_jumpLinks == null) return;
    if (!UseMouseHover) return;

    INavGrid grid = JumpLinkSource == ActiveNavGrid.Cartesian ? (INavGrid)_cart : (INavGrid)_hex;
    if (grid == null) return;

    if (!TryUpdateHover(grid, out _hoverA, out _hoverB))
      return;

    if (!_jumpLinks.TryGetCellIndex(_hoverA, _hoverB, out int idx))
      return;

    var prev = Gizmos.color;
    Gizmos.color = JumpLinkColor;

    Vector3 from = grid.CellCenterWorld(_hoverA, _hoverB) + Vector3.up * (YOffset * 2f);

    _jumpLinks.ForEachFromIndex(idx, e =>
    {
      Vector3 to = grid.CellCenterWorld(e.ToA, e.ToB) + Vector3.up * (YOffset * 2f);
      DrawArc(from, to, JumpArcHeight);
      DrawArrowHead(to, (to - from).normalized);
      return true;
    });

    Gizmos.color = prev;

    Gizmos.DrawSphere(from, grid.CellSize * 0.12f);
  }

  bool TryUpdateHover(INavGrid grid, out int a, out int b)
  {
    a = 0; b = 0;

    Vector3 p;
    if (!TryGetMousePlanePoint(out p))
      return false;

    if (!grid.TryWorldToCell(p, out a, out b))
      return false;

    _hoverValid = true;
    return true;
  }

  bool TryGetMousePlanePoint(out Vector3 worldPoint)
  {
    worldPoint = default;

    Plane plane = new Plane(Vector3.up, Origin);

    if (Application.isPlaying)
    {
      var cam = Camera.main;
      if (cam == null) return false;

      Ray ray = cam.ScreenPointToRay(Input.mousePosition);
      if (!plane.Raycast(ray, out float t)) return false;
      if (t < 0f || t > HoverPickMaxDistance) return false;

      worldPoint = ray.GetPoint(t);
      return true;
    }
    else
    {
      var cam = Camera.current;
      if (cam == null) return false;

      Vector3 mp = Vector3.zero;
      if (Event.current != null)
      {
        mp = Event.current.mousePosition;
        mp.y = cam.pixelHeight - mp.y;
      }
      else
      {
        return false;
      }

      Ray ray = cam.ScreenPointToRay(mp);
      if (!plane.Raycast(ray, out float t)) return false;
      if (t < 0f || t > HoverPickMaxDistance) return false;

      worldPoint = ray.GetPoint(t);
      return true;
    }
  }

  void BuildRaySets()
  {
    _cartRayCells.Clear();
    _hexRayCells.Clear();

    if (RayTransform == null) return;

    Vector3 from = RayTransform.position;
    Vector3 dir = RayTransform.forward;
    dir.y = 0f;
    if (dir.sqrMagnitude < 1e-6f) return;
    dir.Normalize();

    if (HighlightAllIntersected)
    {
      if (_cart != null)
      {
        _cart.RayWalk(from, dir, RayDistance, (x, z) =>
        {
          _cartRayCells.Add(z * _cart.Width + x);
          return true;
        });
      }

      if (_hex != null)
      {
        _hex.RayWalk(from, dir, RayDistance, (q, r) =>
        {
          _hexRayCells.Add(Pack(q, r));
          return true;
        });
      }
    }
    else
    {
      System.Func<NavCellTag, bool> stop = StopOnVoid ? (t => t == NavCellTag.Void) : (t => t == NavCellTag.Walk);

      if (_cart != null)
      {
        var hit = RaycastSingle(_cart, from, dir, RayDistance, stop);
        if (hit.hit)
          _cartRayCells.Add(hit.b * _cart.Width + hit.a);
      }

      if (_hex != null)
      {
        var hit = RaycastSingle(_hex, from, dir, RayDistance, stop);
        if (hit.hit)
          _hexRayCells.Add(Pack(hit.a, hit.b));
      }
    }
  }

  static (bool hit, int a, int b) RaycastSingle(INavGrid grid, Vector3 from, Vector3 dir, float dist, System.Func<NavCellTag, bool> stop)
  {
    bool found = false;
    int ha = 0, hb = 0;

    grid.RayWalk(from, dir, dist, (a, b) =>
    {
      if (!grid.TryGetTag(a, b, out var tag)) tag = NavCellTag.Void;
      if (stop(tag))
      {
        found = true;
        ha = a;
        hb = b;
        return false;
      }
      return true;
    });

    return (found, ha, hb);
  }

  void DrawRayLine()
  {
    if (RayTransform == null) return;

    Vector3 from = RayTransform.position;
    Vector3 dir = RayTransform.forward;
    dir.y = 0f;
    if (dir.sqrMagnitude < 1e-6f) return;
    dir.Normalize();

    var prev = Gizmos.color;
    Gizmos.color = IntersectColor;
    Gizmos.DrawLine(from, from + dir * Mathf.Max(0f, RayDistance));
    Gizmos.color = prev;
  }

  public static void DrawCartesianGizmos(
    CartesianNavGraph g,
    bool drawWalk,
    bool drawVoid,
    float yOffset,
    Color walkColor,
    Color voidColor,
    Color intersectColor,
    HashSet<int> rayCells,
    bool solidIntersect)
  {
    float cs = g.CellSize;
    Vector3 size = new Vector3(cs, 0.02f, cs);

    var prev = Gizmos.color;

    for (int z = 0; z < g.Height; z++)
    for (int x = 0; x < g.Width; x++)
    {
      int id = z * g.Width + x;

      Vector3 c = g.CellCenterWorld(x, z) + Vector3.up * yOffset;
      var tag = g.TryGetTag(x, z, out var t) ? t : NavCellTag.Void;

      bool isRay = rayCells != null && rayCells.Contains(id);

      if (isRay) Gizmos.color = intersectColor;
      else if (tag == NavCellTag.Walk) Gizmos.color = walkColor;
      else Gizmos.color = voidColor;

      if (tag == NavCellTag.Walk && !drawWalk && !isRay) continue;
      if (tag == NavCellTag.Void && !drawVoid && !isRay) continue;

      if (isRay && solidIntersect) Gizmos.DrawCube(c, size);
      else Gizmos.DrawWireCube(c, size);
    }

    Gizmos.color = prev;
  }

  public static void DrawHexGizmos(
    HexagonalNavGraph g,
    bool drawWalk,
    bool drawVoid,
    float yOffset,
    Color walkColor,
    Color voidColor,
    Color intersectColor,
    HashSet<long> rayCells,
    bool solidIntersect)
  {
    var prev = Gizmos.color;

    g.ForEachCell((q, r, c0, tag) =>
    {
      Vector3 c = c0 + Vector3.up * yOffset;
      long key = Pack(q, r);

      bool isRay = rayCells != null && rayCells.Contains(key);

      if (isRay) Gizmos.color = intersectColor;
      else if (tag == NavCellTag.Walk) Gizmos.color = walkColor;
      else Gizmos.color = voidColor;

      if (tag == NavCellTag.Walk && !drawWalk && !isRay) return true;
      if (tag == NavCellTag.Void && !drawVoid && !isRay) return true;

      if (isRay && solidIntersect) DrawSolidHex(c, g.CellSize);
      else DrawWireHex(c, g.CellSize);

      return true;
    });

    Gizmos.color = prev;
  }

  static void DrawWireHex(Vector3 center, float size)
  {
    Vector3[] pts = new Vector3[6];
    for (int i = 0; i < 6; i++)
    {
      float a = Mathf.Deg2Rad * (60f * i - 30f);
      pts[i] = center + new Vector3(size * Mathf.Cos(a), 0f, size * Mathf.Sin(a));
    }
    for (int i = 0; i < 6; i++)
      Gizmos.DrawLine(pts[i], pts[(i + 1) % 6]);
  }

  static void DrawSolidHex(Vector3 center, float size)
  {
    float w = 1.7320508075688772f * size;
    float h = 1.5f * size;
    Gizmos.DrawCube(center, new Vector3(w, 0.03f, h));
  }

  static void DrawArc(Vector3 from, Vector3 to, float height)
  {
    const int STEPS = 16;

    Vector3 mid = (from + to) * 0.5f + Vector3.up * height;
    Vector3 prev = from;

    for (int i = 1; i <= STEPS; i++)
    {
      float t = (float)i / STEPS;
      Vector3 a = Vector3.Lerp(from, mid, t);
      Vector3 b = Vector3.Lerp(mid, to, t);
      Vector3 p = Vector3.Lerp(a, b, t);
      Gizmos.DrawLine(prev, p);
      prev = p;
    }
  }

  static void DrawArrowHead(Vector3 tip, Vector3 dir)
  {
    float s = 0.3f;
    Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
    Vector3 back = -dir;

    Gizmos.DrawLine(tip, tip + (back + right) * s);
    Gizmos.DrawLine(tip, tip + (back - right) * s);
  }

  static long Pack(int a, int b)
  {
    unchecked { return ((long)a << 32) ^ (uint)b; }
  }
}
