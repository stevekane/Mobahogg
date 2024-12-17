using UnityEngine;

public class RopeSolver : MonoBehaviour {
  public int nodeCount = 5;          // Number of rope nodes
  public float segmentLength = 1.0f; // Desired distance between nodes
  public float stiffness = 0.2f;     // Elastic stiffness (0 = loose, 1 = very stiff)
  public float damping = 0.9f;       // Reduces jittering (0 = no movement, 1 = full movement)

  private Vector3[] nodes;           // Positions of the rope nodes
  private Vector3[] velocities;      // Velocities of the nodes
  private Vector3[] forces;          // Forces acting on the nodes

  public Transform leader;           // The leader object the rope follows
  private Vector3 leaderDirection;   // The original "straight back" direction

  public void Simulate(float dt) {
    if (leader == null) return;

    UpdateLeaderDirection();
    SimulateRope(dt);
    ApplyOrientationConstraint();
  }

  public Vector3 GetNodePosition(int index) {
    if (index < 0 || index >= nodeCount) {
      Debug.LogWarning("Invalid node index!");
      return Vector3.zero;
    }
    return nodes[index];
  }

  public Vector3 GetNodeForward(int index) {
    if (index < 0 || index >= nodeCount) {
      Debug.LogWarning("Invalid node index!");
      return Vector3.forward; // Default forward direction
    }

    // If not the last node, compute direction to the next node
    if (index < nodeCount - 1) {
      return (nodes[index + 1] - nodes[index]).normalized;
    }

    // For the last node, use the direction from the previous node
    if (index > 0) {
      return (nodes[index] - nodes[index - 1]).normalized;
    }

    // Fallback for single-node rope
    return leader.forward; // Align with leader's forward if no better direction
  }


  void Start() {
    InitializeRope();
  }

  private void InitializeRope() {
    nodes = new Vector3[nodeCount];
    velocities = new Vector3[nodeCount];
    forces = new Vector3[nodeCount];

    // Initialize rope nodes in a straight line behind the leader
    leaderDirection = -leader.forward;
    for (int i = 0; i < nodeCount; i++) {
      nodes[i] = leader.position + leaderDirection * (i * segmentLength);
      velocities[i] = Vector3.zero;
    }
  }

  private void UpdateLeaderDirection() {
    // The leaderDirection always points backward relative to the leader
    leaderDirection = -leader.forward;
  }

  private void SimulateRope(float dt) {
    // Step 1: Leader node follows the leader's position
    nodes[0] = leader.position;

    // Step 2: Apply elastic constraints to maintain segment length
    for (int i = 1; i < nodeCount; i++) {
      Vector3 delta = nodes[i] - nodes[i - 1];
      float distance = delta.magnitude;
      float error = distance - segmentLength;

      // Apply correction
      Vector3 correction = delta.normalized * (error * 0.5f);
      nodes[i - 1] += correction * stiffness;
      nodes[i] -= correction * stiffness;

      // Apply damping to velocities
      velocities[i] *= damping;
    }

    // Step 3: Update positions using velocities
    for (int i = 1; i < nodeCount; i++) {
      velocities[i] += forces[i] * dt; // Apply forces if any
      nodes[i] += velocities[i] * dt;
      forces[i] = Vector3.zero; // Clear forces
    }
  }

  private void ApplyOrientationConstraint() {
    // Make the rope nodes attempt to align "straight back" in leader's direction
    for (int i = 1; i < nodeCount; i++) {
      Vector3 targetPosition = nodes[i - 1] + leaderDirection * segmentLength;
      Vector3 offset = targetPosition - nodes[i];

      // Apply a small "snap" force to bring the node toward the target position
      nodes[i] += offset * stiffness * 0.5f;
    }
  }

  // Draw the rope for debugging
  void OnDrawGizmos() {
    if (nodes == null || nodes.Length < 2) return;

    Gizmos.color = Color.green;
    for (int i = 0; i < nodeCount - 1; i++) {
      Gizmos.DrawLine(nodes[i], nodes[i + 1]);
      Gizmos.DrawSphere(nodes[i], 0.1f);
    }
    Gizmos.DrawSphere(nodes[nodeCount - 1], 0.1f);
  }
}
