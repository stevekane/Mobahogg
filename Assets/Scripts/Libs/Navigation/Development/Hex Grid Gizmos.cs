using UnityEngine;

[ExecuteAlways]
public class HexGridGizmos : MonoBehaviour
{
  [Header("Grid (half-extents)")]
  [Tooltip("Number of hex columns to extend to the right AND left of center. Total columns = 1 + 2*halfColumns.")]
  [SerializeField, Min(0)] int halfColumns = 5;

  [Tooltip("Number of hex rows to extend up AND down of center. Total rows = 1 + 2*halfRows.")]
  [SerializeField, Min(0)] int halfRows = 5;

  [Tooltip("Hex circumradius (center -> corner), in Unity units.")]
  [SerializeField, Min(0.0001f)] float hexRadius = 1f;

  [Tooltip("Visual shrink amount applied to hexRadius when drawing (prevents outlines overlapping).")]
  [SerializeField, Min(0f)] float drawPadding = 0.03f;

  [Header("Query / Highlight")]
  [SerializeField] Transform target;
  [SerializeField, Min(0f)] float circleRadius = 3f;

  [Header("Gizmo Colors")]
  [SerializeField] Color baseColor = Color.green;     // no overlap
  [SerializeField] Color hitColor = Color.gray;       // within circle (except center)
  [SerializeField] Color centerColor = Color.white;   // tile containing target

  void OnDrawGizmos()
  {
    if (hexRadius <= 0f) return;

    float size = hexRadius;
    float drawSize = Mathf.Max(0f, hexRadius - drawPadding);

    // Pointy-top, "odd-r" offset rows (classic watertight layout):
    // Horizontal step between columns:
    float colStepX = Mathf.Sqrt(3f) * size;
    // Vertical step between rows:
    float rowStepZ = 1.5f * size;
    // Half-column offset applied to every other row:
    float halfShiftX = 0.5f * colStepX;

    bool hasTarget = target != null;
    Vector3 targetWorld = default;
    Vector3 targetLocal = default;

    if (hasTarget)
    {
      targetWorld = target.position;
      targetLocal = transform.InverseTransformPoint(targetWorld);
    }

    // Find closest tile (within generated grid) to targetLocal in XZ.
    float bestSqr = float.PositiveInfinity;
    int bestI = 0, bestJ = 0;
    bool bestValid = false;

    if (hasTarget)
    {
      for (int j = -halfRows; j <= halfRows; j++)
      {
        float rowOffsetX = ((j & 1) == 0) ? 0f : halfShiftX; // <-- KEY FIX (0 / +halfShift), not (+/-)
        for (int i = -halfColumns; i <= halfColumns; i++)
        {
          Vector3 c = new Vector3(i * colStepX + rowOffsetX, 0f, j * rowStepZ);
          float dx = targetLocal.x - c.x;
          float dz = targetLocal.z - c.z;
          float sqr = dx * dx + dz * dz;

          if (sqr < bestSqr)
          {
            bestSqr = sqr;
            bestI = i;
            bestJ = j;
            bestValid = true;
          }
        }
      }
    }

    // Optional: draw the query circle in world space (for clarity)
    if (hasTarget && circleRadius > 0f)
    {
      Gizmos.color = new Color(1f, 1f, 1f, 0.25f);
      Gizmos.DrawWireSphere(targetWorld, circleRadius);
    }

    Matrix4x4 oldMatrix = Gizmos.matrix;
    Gizmos.matrix = transform.localToWorldMatrix;

    for (int j = -halfRows; j <= halfRows; j++)
    {
      float rowOffsetX = ((j & 1) == 0) ? 0f : halfShiftX;

      for (int i = -halfColumns; i <= halfColumns; i++)
      {
        Vector3 centerLocal = new Vector3(i * colStepX + rowOffsetX, 0f, j * rowStepZ);

        Color c = baseColor;

        if (hasTarget)
        {
          if (bestValid && i == bestI && j == bestJ)
          {
            c = centerColor;
          }
          else
          {
            Vector3 centerWorld = transform.TransformPoint(centerLocal);
            float d = Vector3.Distance(centerWorld, targetWorld);
            if (d <= circleRadius) c = hitColor;
          }
        }

        Gizmos.color = c;
        DrawHexOutlineLocalXZ(centerLocal, drawSize);
      }
    }

    Gizmos.matrix = oldMatrix;
  }

  // Draw a pointy-top hex outline on the local XZ plane.
  static void DrawHexOutlineLocalXZ(Vector3 centerLocal, float radius)
  {
    if (radius <= 0f) return;

    Vector3 prev = Corner(centerLocal, radius, 0);
    for (int i = 1; i <= 6; i++)
    {
      Vector3 cur = Corner(centerLocal, radius, i % 6);
      Gizmos.DrawLine(prev, cur);
      prev = cur;
    }
  }

  // Pointy-top corners: angle = 60*i - 30 degrees
  static Vector3 Corner(Vector3 c, float radius, int i)
  {
    float angleDeg = 60f * i - 30f;
    float rad = angleDeg * Mathf.Deg2Rad;
    float x = c.x + radius * Mathf.Cos(rad);
    float z = c.z + radius * Mathf.Sin(rad);
    return new Vector3(x, c.y, z);
  }
}