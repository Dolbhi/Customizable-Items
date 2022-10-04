using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ColbyDoan
{
    [CustomEditor(typeof(DisplaceWithDepth))]
    public class DisplacerEditor : Editor
    {
        DisplaceWithDepth displacingSprite;

        void OnEnable()
        {
            displacingSprite = target as DisplaceWithDepth;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Update displacement"))
            {
                displacingSprite.LateUpdate();
            }
        }
    }
}