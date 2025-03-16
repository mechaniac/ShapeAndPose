#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ShapeAndPose_ns
{

    [CustomEditor(typeof(ShapeAndPose))]
    public class ShapeAndPoseEditor : Editor
    {


        void OnSceneGUI()
        {

        }
    }
}
#endif
