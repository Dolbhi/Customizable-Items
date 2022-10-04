using UnityEngine;
using UnityEditor;

namespace ColbyDoan
{
    [CustomEditor(typeof(MoveDecider))]
    public class MoveDeciderEditor : Editor
    {
        MoveDecider moveDecider;

        void OnEnable()
        {
            moveDecider = target as MoveDecider;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Populate Directions"))
            {
                moveDecider.PopulateDirections();
                SceneView.RepaintAll();
            }
        }
    }
}