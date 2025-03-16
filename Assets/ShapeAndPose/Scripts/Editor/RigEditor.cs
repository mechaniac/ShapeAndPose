#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

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
                r.InitializeRig();
            }
        }

        void OnSceneGUI()
        {

        }
    }
}
#endif