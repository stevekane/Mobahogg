using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class ElectricalArc : MonoBehaviour
{
    public List<Vector3> controlPoints = new();

    [Header("Arc Settings")]
    public float jaggedness = 0.5f;
    public int subdivisions = 4;
    public float arcLifetime = 0.2f;

    [Header("Branching")]
    public int maxBranches = 3;
    public float branchProbability = 0.2f;
    public float branchLifetime = 0.15f;

    [Header("Width Animation")]
    public AnimationCurve widthOverLifetime = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float widthMultiplier = 0.2f;

    private LineRenderer mainLine;
    private float mainArcAge;

    private List<Vector3> mainArcPoints = new();
    private List<BranchArc> activeBranches = new();

    private class BranchArc
    {
        public LineRenderer renderer;
        public float age;
        public float lifetime;
        public Vector3 start, end;
    }

    void Awake()
    {
        mainLine = GetComponent<LineRenderer>();
        mainLine.positionCount = 0;
    }

    void Update()
    {
        mainArcAge += Time.deltaTime;

        if (mainArcAge >= arcLifetime)
        {
            GenerateMainArc();
            TrySpawnBranch();
        }

        UpdateMainArcWidth();
        UpdateBranchLifetimes();
    }

    void GenerateMainArc()
    {
        mainArcPoints.Clear();
        if (controlPoints.Count < 2) return;

        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector3 start = controlPoints[i];
            Vector3 end = controlPoints[i + 1];

            List<Vector3> arcSegment = GeneratePerturbedArc(start, end);
            if (i > 0) arcSegment.RemoveAt(0); // no dupes
            mainArcPoints.AddRange(arcSegment);
        }

        mainLine.positionCount = mainArcPoints.Count;
        mainLine.SetPositions(mainArcPoints.ToArray());
        mainArcAge = 0;
    }

    void TrySpawnBranch()
    {
        if (activeBranches.Count >= maxBranches || mainArcPoints.Count < 4) return;
        if (Random.value > branchProbability) return;

        int startIndex = Random.Range(0, mainArcPoints.Count - 2);
        int endIndex = startIndex + Random.Range(1, 3);
        endIndex = Mathf.Min(endIndex, mainArcPoints.Count - 1);

        Vector3 start = mainArcPoints[startIndex];
        Vector3 end = mainArcPoints[endIndex];

        GameObject branchGO = new GameObject("BranchArc");
        branchGO.transform.parent = transform;
        LineRenderer lr = branchGO.AddComponent<LineRenderer>();
        CopyLineRendererSettings(mainLine, lr);

        activeBranches.Add(new BranchArc
        {
            renderer = lr,
            age = 0,
            lifetime = branchLifetime,
            start = start,
            end = end
        });
    }

    void UpdateBranchLifetimes()
    {
        for (int i = activeBranches.Count - 1; i >= 0; i--)
        {
            BranchArc branch = activeBranches[i];
            branch.age += Time.deltaTime;

            float t = Mathf.Clamp01(branch.age / branch.lifetime);
            float width = widthOverLifetime.Evaluate(t) * widthMultiplier;
            branch.renderer.widthMultiplier = width;

            if (branch.age >= branch.lifetime)
            {
                Destroy(branch.renderer.gameObject);
                activeBranches.RemoveAt(i);
                continue;
            }

            var arc = GeneratePerturbedArc(branch.start, branch.end);
            branch.renderer.positionCount = arc.Count;
            branch.renderer.SetPositions(arc.ToArray());
        }
    }

    void UpdateMainArcWidth()
    {
        float t = Mathf.Clamp01(mainArcAge / arcLifetime);
        float width = widthOverLifetime.Evaluate(t) * widthMultiplier;
        mainLine.widthMultiplier = width;
    }

    List<Vector3> GeneratePerturbedArc(Vector3 start, Vector3 end)
    {
        List<Vector3> points = new();
        RecursiveDisplace(points, start, end, subdivisions);
        return points;
    }

    void RecursiveDisplace(List<Vector3> list, Vector3 a, Vector3 b, int depth)
    {
        if (depth == 0)
        {
            list.Add(a);
            list.Add(b);
            return;
        }

        Vector3 mid = (a + b) * 0.5f;
        Vector3 dir = (b - a).normalized;
        Vector3 offsetDir = Vector3.Cross(dir, Random.onUnitSphere).normalized;
        float displacement = jaggedness * Vector3.Distance(a, b) * (Random.value - 0.5f);
        mid += offsetDir * displacement;

        RecursiveDisplace(list, a, mid, depth - 1);
        list.RemoveAt(list.Count - 1);
        RecursiveDisplace(list, mid, b, depth - 1);
    }

    void CopyLineRendererSettings(LineRenderer source, LineRenderer dest)
    {
        dest.material = new Material(source.material);
        dest.widthMultiplier = source.widthMultiplier;
        dest.widthCurve = source.widthCurve;
        dest.colorGradient = source.colorGradient;
        dest.shadowCastingMode = source.shadowCastingMode;
        dest.receiveShadows = source.receiveShadows;
        dest.numCapVertices = source.numCapVertices;
        dest.numCornerVertices = source.numCornerVertices;
        dest.alignment = source.alignment;
        dest.textureMode = source.textureMode;
        dest.useWorldSpace = true;
    }
}
