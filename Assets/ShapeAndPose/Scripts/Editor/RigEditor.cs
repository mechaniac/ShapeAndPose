#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ShapeAndPose_ns
{

    [CustomEditor(typeof(Rig))]
    public class RigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector.
            DrawDefaultInspector();

            // Add a button that calls InitializeRig() when clicked.
            Rig r = (Rig)target;
            if (GUILayout.Button("Initialize Rig"))
            {
                // r.UpdateAllVertices();
            }

            if (GUILayout.Button("Load Config and Create Mesh"))
            {
                // Assuming your JSON config file is located at Assets/Configs/human_01.json
                string configPath = Path.Combine(Application.dataPath, "ShapeAndPose/Scripts/human_01.json");
                r.InitializeRig(configPath);
            }
        }

        void OnSceneGUI()
        {

        }
    }
}
#endif