using UnityEngine;

public static class NavCornerSmoothing
{
  public struct CornerFillet
  {
    public bool Valid;
    public Vector3 P0, P1, P2;
    public Vector3 T1;      // tangent point on P0->P1
    public Vector3 T2;      // tangent point on P1->P2
    public Vector3 Center;  // circle center (on XZ plane)
    public float Radius;
    public float TurnSign;      // +1 = left turn, -1 = right turn (XZ)
    public float ArcAngleRad;   // angle swept along the arc from T1 to T2 (>=0)
    public float CornerSpeedMax; // max speed allowed on this arc (based on constraints)
  }

  static Vector2 XZ(Vector3 v) => new Vector2(v.x, v.z);

  static Vector2 LeftPerp(Vector2 v) => new Vector2(-v.y, v.x);

  /// <summary>
  /// Build a circular fillet (tangent arc) that replaces the hard corner P0->P1->P2.
  /// Assumes motion on the XZ plane (y ignored except for returning points at P1.y).
  ///
  /// wMaxRad is max turn rate (yaw) in rad/s.
  /// aMax is max acceleration magnitude (m/s^2). Used as lateral accel limit here.
  /// vMax is max linear speed (m/s).
  ///
  /// RDesired is an optional "comfort" radius; pass Mathf.Infinity to use maximum-fitting radius.
  /// minAngleDeg: below this, returns invalid (no meaningful corner).
  /// </summary>
  public static CornerFillet ComputeCornerFillet(
    Vector3 P0,
    Vector3 P1,
    Vector3 P2,
    float vMax,
    float aMax,
    float wMaxRad,
    float RDesired = Mathf.Infinity,
    float minAngleDeg = 1.0f)
  {
    CornerFillet outFillet = new CornerFillet
    {
      Valid = false,
      P0 = P0,
      P1 = P1,
      P2 = P2
    };

    Vector2 p0 = XZ(P0);
    Vector2 p1 = XZ(P1);
    Vector2 p2 = XZ(P2);

    Vector2 inDir = (p1 - p0);
    Vector2 outDir = (p2 - p1);

    float lenIn = inDir.magnitude;
    float lenOut = outDir.magnitude;
    if (lenIn < 1e-4f || lenOut < 1e-4f) return outFillet;

    inDir /= lenIn;   // direction toward corner along incoming segment
    outDir /= lenOut; // direction away from corner along outgoing segment

    // Angle between travel direction approaching the corner (-inDir) and leaving (+outDir)
    float cos = Mathf.Clamp(Vector2.Dot(-inDir, outDir), -1f, 1f);
    float theta = Mathf.Acos(cos); // radians in [0..pi]

    if (theta < minAngleDeg * Mathf.Deg2Rad) return outFillet;

    float tanHalf = Mathf.Tan(theta * 0.5f);
    if (Mathf.Abs(tanHalf) < 1e-4f) return outFillet;

    // Maximum radius that fits given finite segment lengths.
    float rFit = Mathf.Min(lenIn, lenOut) / tanHalf;

    // Choose radius to use
    float R = Mathf.Min(RDesired, rFit);
    if (R < 1e-4f) return outFillet;

    // Tangent offset distance from the corner along each segment
    float d = R * tanHalf;

    // Tangent points on the two segments
    Vector2 t1 = p1 - inDir * d;
    Vector2 t2 = p1 + outDir * d;

    // Determine left/right turn sign using 2D cross (inDir -> outDir)
    // cross2(a,b) = a.x*b.y - a.y*b.x
    float cross = inDir.x * outDir.y - inDir.y * outDir.x;
    float turnSign = (cross >= 0f) ? +1f : -1f;

    // Circle center: offset from T1 by +/- left normal of inDir by radius.
    // If turnSign=+1 (left), center is to the left of motion along incoming direction.
    Vector2 nInLeft = LeftPerp(inDir).normalized;
    Vector2 center = t1 + nInLeft * (turnSign * R);

    // Arc sweep angle: angle between (T1-center) and (T2-center)
    Vector2 v1 = (t1 - center).normalized;
    Vector2 v2 = (t2 - center).normalized;

    float arcCos = Mathf.Clamp(Vector2.Dot(v1, v2), -1f, 1f);
    float arcAngle = Mathf.Acos(arcCos); // [0..pi], positive magnitude

    // Max speed through this corner given turn-rate and lateral accel limits.
    float vByTurnRate = wMaxRad * R;
    float vByLatAccel = Mathf.Sqrt(Mathf.Max(0f, aMax * R));
    float vCornerMax = Mathf.Min(vMax, vByTurnRate, vByLatAccel);

    // Populate output on original Y plane (use P1.y for T1/T2/Center for convenience)
    float y = P1.y;
    outFillet.Valid = true;
    outFillet.T1 = new Vector3(t1.x, y, t1.y);
    outFillet.T2 = new Vector3(t2.x, y, t2.y);
    outFillet.Center = new Vector3(center.x, y, center.y);
    outFillet.Radius = R;
    outFillet.TurnSign = turnSign;
    outFillet.ArcAngleRad = arcAngle;
    outFillet.CornerSpeedMax = vCornerMax;

    return outFillet;
  }

  /// <summary>
  /// Sample a point along the arc from T1 to T2, with parameter u in [0..1].
  /// Uses fillet.Center/Radius and turns in the correct direction.
  /// </summary>
  public static Vector3 SampleArc(in CornerFillet f, float u)
  {
    u = Mathf.Clamp01(u);
    Vector2 c = XZ(f.Center);
    Vector2 t1 = XZ(f.T1);

    Vector2 r0 = (t1 - c); // radius vector at start
    float startAng = Mathf.Atan2(r0.y, r0.x);

    // We need a signed sweep. Magnitude is ArcAngleRad, direction given by TurnSign.
    float sweep = f.ArcAngleRad * f.TurnSign;

    float ang = startAng + sweep * u;
    Vector2 p = c + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * f.Radius;

    return new Vector3(p.x, f.Center.y, p.y);
  }
}

public static class CornerFilletGizmos
{
  /// <summary>
  /// Draws a CornerFillet (from NavCornerSmoothing.ComputeCornerFillet) in the Scene view.
  /// Call from OnDrawGizmos or OnDrawGizmosSelected.
  /// </summary>
  public static void Draw(in NavCornerSmoothing.CornerFillet f, int arcSegments = 32)
  {
    if (!f.Valid) return;

    // Raw path
    Gizmos.DrawLine(f.P0, f.P1);
    Gizmos.DrawLine(f.P1, f.P2);

    // Key points
    DrawPoint(f.T1, 0.08f);
    DrawPoint(f.T2, 0.08f);
    DrawPoint(f.Center, 0.06f);

    // Radius indicators
    Gizmos.DrawLine(f.Center, f.T1);
    Gizmos.DrawLine(f.Center, f.T2);

    // Arc polyline
    arcSegments = Mathf.Max(4, arcSegments);
    Vector3 prev = f.T1;
    for (int i = 1; i <= arcSegments; i++)
    {
      float u = (float)i / arcSegments;
      Vector3 p = NavCornerSmoothing.SampleArc(f, u);
      Gizmos.DrawLine(prev, p);
      prev = p;
    }

    // Optional: little tick in the middle of the arc so you can see direction
    Vector3 mid = NavCornerSmoothing.SampleArc(f, 0.5f);
    DrawPoint(mid, 0.05f);

#if UNITY_EDITOR
    // Labels (Editor-only)
    UnityEditor.Handles.Label(f.T1, "T1");
    UnityEditor.Handles.Label(f.T2, "T2");
    UnityEditor.Handles.Label(f.Center, "C");
#endif
  }

  static void DrawPoint(Vector3 p, float r)
  {
    Gizmos.DrawWireSphere(p, r);
    Gizmos.DrawSphere(p, r * 0.35f);
  }
}