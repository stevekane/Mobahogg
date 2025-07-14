using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WobblyMesh : MonoBehaviour
{
    public float springForce = 20f;
    public float damping = 2.5f;
    public float impactRadius = 0.5f;

    private Mesh _deformedMesh;
    private Vector3[] _originalVertices;
    private Vector3[] _deformedVertices;
    private Vector3[] _velocities;
    private Transform _transform;

    void Start()
    {
        _transform = transform;

        var meshFilter = GetComponent<MeshFilter>();
        _deformedMesh = Instantiate(meshFilter.mesh); // Clone the shared mesh
        meshFilter.mesh = _deformedMesh;

        _originalVertices = _deformedMesh.vertices;
        _deformedVertices = new Vector3[_originalVertices.Length];
        _velocities = new Vector3[_originalVertices.Length];

        for (int i = 0; i < _originalVertices.Length; i++)
            _deformedVertices[i] = _originalVertices[i];
    }

    void Update()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < _deformedVertices.Length; i++)
        {
            Vector3 displacement = _deformedVertices[i] - _originalVertices[i];
            Vector3 acceleration = -springForce * displacement - damping * _velocities[i];

            _velocities[i] += acceleration * dt;
            _deformedVertices[i] += _velocities[i] * dt;
        }

        _deformedMesh.vertices = _deformedVertices;
        _deformedMesh.RecalculateNormals();
    }

    /// <summary>
    /// Apply an impulse to the mesh at a world-space position and direction.
    /// </summary>
    public void ApplyImpulse(Vector3 worldPoint, Vector3 direction, float magnitude)
    {
        Vector3 localPoint = _transform.InverseTransformPoint(worldPoint);
        direction = _transform.InverseTransformDirection(direction.normalized);

        for (int i = 0; i < _deformedVertices.Length; i++)
        {
            float dist = Vector3.Distance(_deformedVertices[i], localPoint);
            if (dist < impactRadius)
            {
                float falloff = 1f - (dist / impactRadius);
                _velocities[i] += direction * magnitude * falloff;
            }
        }
    }
}