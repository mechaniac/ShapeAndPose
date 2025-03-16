#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ShapeAndPose_ns
{
    [InitializeOnLoad]
    public static class RigEditorUpdater
    {
        static RigEditorUpdater()
        {
            // Subscribe to the editor update event.
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            // Find all Rig instances in the scene.
            foreach (Rig r in Object.FindObjectsOfType<Rig>())
            {
                r.UpdateRig();
            }
        }
    }
}
#endif
