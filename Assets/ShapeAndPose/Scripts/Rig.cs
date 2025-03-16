using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace ShapeAndPose_ns
{
    public class Rig : MonoBehaviour
    {
        [Header("Source Rig Joints")]
        // The imported rig’s joints.
        public Transform[] sourceJoints;
        
        [Header("Limb Joints")]
        // Limb arrays (populated from the config file).
        public Joint[] arm_lft;
        public Joint[] arm_rgt;
        
        // This mesh will display the combined limb meshes.
        public Mesh bodyMesh;

        /// <summary>
        /// Main initialization method.
        /// Loads the config, attaches Joint components, and creates the body mesh.
        /// </summary>
        public void InitializeRig(string configPath)
        {
            CreateJoints(configPath);
            CreateBodyMeshes();
        }

        /// <summary>
        /// Loads the JSON config, finds the source joints by name, attaches Joint components,
        /// computes their vertex rings, and stores them in limb arrays.
        /// </summary>
        public void CreateJoints(string configPath)
        {
            if (!File.Exists(configPath))
            {
                Debug.LogError("Config file not found: " + configPath);
                return;
            }
            string json = File.ReadAllText(configPath);
            RigConfig config = JsonUtility.FromJson<RigConfig>(json);

            List<Joint> leftJoints = new List<Joint>();
            if (config.meshArmLeft != null)
            {
                foreach (string jointName in config.meshArmLeft)
                {
                    Transform found = FindSourceJointByName(jointName);
                    if (found != null)
                    {
                        Joint jnt = found.GetComponent<Joint>();
                        if (jnt == null)
                        {
                            jnt = found.gameObject.AddComponent<Joint>();
                        }
                        jnt.ComputeVertexRing();
                        leftJoints.Add(jnt);
                    }
                }
            }
            arm_lft = leftJoints.ToArray();

            List<Joint> rightJoints = new List<Joint>();
            if (config.meshArmRight != null)
            {
                foreach (string jointName in config.meshArmRight)
                {
                    Transform found = FindSourceJointByName(jointName);
                    if (found != null)
                    {
                        Joint jnt = found.GetComponent<Joint>();
                        if (jnt == null)
                        {
                            jnt = found.gameObject.AddComponent<Joint>();
                        }
                        jnt.ComputeVertexRing();
                        rightJoints.Add(jnt);
                    }
                }
            }
            arm_rgt = rightJoints.ToArray();
        }

        /// <summary>
        /// Searches the sourceJoints array for a Transform matching the given name.
        /// </summary>
        private Transform FindSourceJointByName(string jointName)
        {
            foreach (Transform t in sourceJoints)
            {
                if (t.name == jointName)
                    return t;
            }
            return null;
        }

        /// <summary>
        /// Creates a mesh for each limb (cylinder from the joints' vertex rings) and combines them into one body mesh.
        /// </summary>
        public void CreateBodyMeshes()
        {
            // Create a mesh for each limb.
            Mesh leftMesh = CreateMeshFromJoints(arm_lft);
            Mesh rightMesh = CreateMeshFromJoints(arm_rgt);

            // Combine the limb meshes into one body mesh.
            List<CombineInstance> combineInstances = new List<CombineInstance>();
            if (leftMesh != null)
            {
                combineInstances.Add(new CombineInstance { mesh = leftMesh, transform = Matrix4x4.identity });
            }
            if (rightMesh != null)
            {
                combineInstances.Add(new CombineInstance { mesh = rightMesh, transform = Matrix4x4.identity });
            }
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, false);
            bodyMesh = combinedMesh;

            // Assign the combined mesh to this GameObject.
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null)
            {
                mf = gameObject.AddComponent<MeshFilter>();
            }
            mf.mesh = bodyMesh;

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr == null)
            {
                mr = gameObject.AddComponent<MeshRenderer>();
            }
            if (mr.sharedMaterial == null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Standard"));
            }
        }

        /// <summary>
        /// Given an array of Joint components, creates a cylindrical mesh by connecting each joint's vertex ring.
        /// </summary>
        public Mesh CreateMeshFromJoints(Joint[] joints)
        {
            if (joints == null || joints.Length < 2)
                return null;

            int divisions = joints[0].vertexRing.Length;
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            // Add all vertex rings to the mesh vertices.
            for (int j = 0; j < joints.Length; j++)
            {
                meshVertices.AddRange(joints[j].vertexRing);
            }

            // Connect consecutive rings with triangles.
            for (int ring = 0; ring < joints.Length - 1; ring++)
            {
                int startCurrent = ring * divisions;
                int startNext = (ring + 1) * divisions;
                for (int i = 0; i < divisions; i++)
                {
                    int nextI = (i + 1) % divisions;
                    // Triangle 1
                    triangles.Add(startCurrent + i);
                    triangles.Add(startNext + i);
                    triangles.Add(startNext + nextI);
                    // Triangle 2
                    triangles.Add(startCurrent + i);
                    triangles.Add(startNext + nextI);
                    triangles.Add(startCurrent + nextI);
                }
            }
            Mesh mesh = new Mesh();
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
