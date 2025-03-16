using UnityEngine;

namespace ShapeAndPose_ns
{
    public class Joint : MonoBehaviour
    {
        [Header("Vertex Ring Settings")]
        public float ringRadius = 0.05f;
        public int ringDivisions = 8;
        
        // The circular cross‐section computed at this joint.
        public Vector3[] vertexRing;
        
        // Direct child joints (set during initialization).
        public Joint[] children;

        /// <summary>
        /// Computes the vertex ring (circle of points) at this joint’s position.
        /// </summary>
        public void ComputeVertexRing()
        {
            vertexRing = new Vector3[ringDivisions];
            Vector3 center = transform.position;
            Quaternion rotation = transform.rotation;
            float angleStep = 360f / ringDivisions;
            for (int i = 0; i < ringDivisions; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                // Compute a point in the joint's local XZ plane.
                Vector3 localPoint = new Vector3(Mathf.Cos(angle) * ringRadius, 0, Mathf.Sin(angle) * ringRadius);
                vertexRing[i] = center + rotation * localPoint;
            }
        }
    }
}
