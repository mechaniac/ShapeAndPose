using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace ShapeAndPose_ns
{
    public class Rig : MonoBehaviour
    {
        [Header("Source Rig Hierarchy")]
        // Holds the root of the entire joint hierarchy in the scene.
        public Transform sourceRig;

        private int previousChecksum = 0;

        [Header("Limb Data")]
        // Each limb is stored as an array of Joint; limbs are determined from the config.
        public List<Joint[]> limbs = new List<Joint[]>();

        // The final combined body mesh.
        public Mesh bodyMesh;

        /// <summary>
        /// Main initialization method.
        /// Loads the config, creates the Joint components from the sourceRig hierarchy, and builds the body mesh.
        /// </summary>
        /// <param name="configPath">Full path to the JSON config file.</param>
        public void InitializeRig()
        {
            string configPath = Path.Combine(Application.dataPath, "ShapeAndPose/Scripts/human_01.json");
            CreateJoints(configPath);
            CreateBodyMeshes();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateRig();
        }

        public void UpdateRig()
        {
            int currentChecksum = ComputeRigChecksum(sourceRig);
            if (currentChecksum != previousChecksum)
            {
                
                InitializeRig();
                previousChecksum = currentChecksum;
            }
        }


        /// <summary>
        /// Loads the JSON config and for each limb defined there, finds matching joints in the sourceRig hierarchy.
        /// Attaches Joint components (if needed), computes their vertex rings, and stores them in the limbs list.
        /// Also assigns each Joint’s direct children.
        /// </summary>
        /// <param name="configPath">Full path to the JSON config file.</param>
        public void CreateJoints(string configPath)
        {
            if (!File.Exists(configPath))
            {
                Debug.LogError("Config file not found: " + configPath);
                return;
            }
            string json = File.ReadAllText(configPath);
            RigConfig config = JsonUtility.FromJson<RigConfig>(json);

            // Clear any previously stored limbs.
            limbs.Clear();

            // Iterate over each limb defined in the config.
            foreach (LimbConfig limbConfig in config.limbs)
            {
                List<Joint> limbJoints = new List<Joint>();
                foreach (string jointName in limbConfig.joints)
                {
                    Transform found = FindChildRecursive(sourceRig, jointName);
                    if (found != null)
                    {
                        Joint jnt = found.GetComponent<Joint>();
                        if (jnt == null)
                        {
                            jnt = found.gameObject.AddComponent<Joint>();
                        }
                        jnt.ComputeVertexRing();
                        limbJoints.Add(jnt);
                    }
                    else
                    {
                        Debug.LogWarning("Joint not found: " + jointName);
                    }
                }
                limbs.Add(limbJoints.ToArray());
            }

            // After collecting limbs, assign direct children for each Joint.
            foreach (Joint[] limb in limbs)
            {
                AssignDirectChildren(limb);
            }
        }

        /// <summary>
        /// Recursively searches the hierarchy starting at 'parent' for a Transform with the given name.
        /// </summary>
        private Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;
            foreach (Transform child in parent)
            {
                Transform result = FindChildRecursive(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// For each Joint in the provided array, assigns its direct child joints (if present).
        /// </summary>
        private void AssignDirectChildren(Joint[] joints)
        {
            if (joints == null)
                return;
            foreach (Joint j in joints)
            {
                List<Joint> directChildren = new List<Joint>();
                foreach (Transform child in j.transform)
                {
                    Joint childJoint = child.GetComponent<Joint>();
                    if (childJoint != null)
                    {
                        directChildren.Add(childJoint);
                    }
                }
                j.children = directChildren.ToArray();
            }
        }

        /// <summary>
        /// Creates a mesh for each limb by connecting the vertex rings of its Joint components,
        /// then combines all limb meshes into one body mesh attached to this GameObject.
        /// </summary>
        public void CreateBodyMeshes()
        {
            List<CombineInstance> combineInstances = new List<CombineInstance>();

            foreach (Joint[] limb in limbs)
            {
                Mesh limbMesh = CreateMeshFromJoints(limb);
                if (limbMesh != null)
                {
                    combineInstances.Add(new CombineInstance { mesh = limbMesh, transform = Matrix4x4.identity });
                }
            }
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, false);
            bodyMesh = combinedMesh;

            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null)
                mf = gameObject.AddComponent<MeshFilter>();
            mf.mesh = bodyMesh;

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr == null)
                mr = gameObject.AddComponent<MeshRenderer>();
            if (mr.sharedMaterial == null)
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
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

            // Append all vertex rings.
            for (int i = 0; i < joints.Length; i++)
            {
                meshVertices.AddRange(joints[i].vertexRing);
            }

            // Create triangles between consecutive rings.
            for (int ring = 0; ring < joints.Length - 1; ring++)
            {
                int startCurrent = ring * divisions;
                int startNext = (ring + 1) * divisions;
                for (int i = 0; i < divisions; i++)
                {
                    int nextI = (i + 1) % divisions;
                    // Triangle 1.
                    triangles.Add(startCurrent + i);
                    triangles.Add(startNext + i);
                    triangles.Add(startNext + nextI);
                    // Triangle 2.
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

        /// <summary>
        /// Computes a checksum from all joints’ positions.
        /// </summary>
        private int ComputeRigChecksum(Transform root)
        {
            int checksum = 17;
            if (root == null)
                return checksum;

            // Combine the current transform's properties.
            checksum = checksum * 31 + root.position.GetHashCode();
            checksum = checksum * 31 + root.rotation.GetHashCode();
            checksum = checksum * 31 + root.localScale.GetHashCode();

            // Recurse through all children.
            foreach (Transform child in root)
            {
                checksum = checksum * 31 + ComputeRigChecksum(child);
            }
            return checksum;
        }
    }
}
