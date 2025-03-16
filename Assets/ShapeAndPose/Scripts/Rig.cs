using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

namespace ShapeAndPose_ns
{
    public class Rig : MonoBehaviour
    {
        public Transform[] joints;
        private int previousChecksum = 0;


        public List<Vector3> vertices = new List<Vector3>();

        // Start is called before the first frame update
        void Start()
        {
            InitializeRig();
        }

        public void InitializeRig()
        {
            vertices.Clear();
            foreach (Transform t in joints)
            {
                Vector3[] points = CreatePointsAtPosition(t, Vector3.zero, Vector3.zero, .1f, 6);
                vertices.AddRange(points);
                // Debug.Log($"updating rig");
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdateRig();
        }

        public void UpdateRig()
        {
            int currentChecksum = ComputeJointsChecksum();
            if (currentChecksum != previousChecksum)
            {
                InitializeRig();
                previousChecksum = currentChecksum;
            }
        }


        /// <summary>
        /// Creates an array of points arranged in a circle local to a given position.
        /// </summary>
        /// <param name="position">Center of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="divisions">Number of points to generate (evenly spaced).</param>
        /// <returns>Array of Vector3 positions.</returns>
        public Vector3[] CreatePointsAtPosition(Transform parent, Vector3 localOffset, Vector3 localEulerOrientation, float radius, int divisions)
        {
            if (divisions < 1)
                return new Vector3[0];

            Vector3[] points = new Vector3[divisions];

            // Compute the circle center in world space from the parent's local offset.
            Vector3 center = parent.TransformPoint(localOffset);

            // Combine the parent's rotation with the local Euler orientation to get the circle's rotation.
            Quaternion circleRotation = parent.rotation * Quaternion.Euler(localEulerOrientation);

            float angleStep = 360f / divisions;
            for (int i = 0; i < divisions; i++)
            {
                // Calculate the angle in radians for each division.
                float angle = angleStep * i * Mathf.Deg2Rad;

                // Create a point on a circle in the local XZ plane.
                Vector3 localPoint = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                // Rotate the point using the circle rotation.
                Vector3 rotatedPoint = circleRotation * localPoint;

                // Translate the point into world space.
                points[i] = center + rotatedPoint;
            }

            return points;
        }

        /// <summary>
        /// Computes a checksum from all joints’ positions.
        /// </summary>
        private int ComputeJointsChecksum()
        {
            int checksum = 17;
            if (joints == null)
                return checksum;

            foreach (Transform t in joints)
            {
                checksum = checksum * 31 + t.position.GetHashCode();
                checksum = checksum * 31 + t.rotation.GetHashCode();
                checksum = checksum * 31 + t.localScale.GetHashCode();
            }
            return checksum;
        }


        // OnDrawGizmos draws gizmos regardless of whether the GameObject is selected.
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            float sphereSize = 0.02f;
            foreach (Vector3 vertex in vertices)
            {
                Gizmos.DrawSphere(vertex, sphereSize);
            }
        }

    }
}