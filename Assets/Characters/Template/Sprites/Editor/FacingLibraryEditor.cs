using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ColbyDoan
{
    // [CustomEditor(typeof(FacingSpritesLibrary))]
    public class FacingLibraryEditor : Editor
    {
        FacingSpritesLibrary library;

        void OnEnable()
        {
            library = target as FacingSpritesLibrary;
        }

        // bool 
        FacingDirections facing;
        string label = "c";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            facing = (FacingDirections)EditorGUILayout.EnumPopup("Facing", facing);
            label = EditorGUILayout.TextField("Label", label);

            GUILayout.Label("Number of items in facing: " + library.facingLibraries[(int)facing].ValuesList.Count);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(library.GetSprite(facing, label), typeof(Sprite), false);
            GUI.enabled = true;

            // columnsExpanded = EditorGUILayout.Foldout(columnsExpanded, "Label Names");
            // if (columnsExpanded)
            // {
            //     int arraySize = columnNames.Length;
            //     arraySize = EditorGUILayout.IntField("Size", arraySize);

            //     string[] newArray = new string[arraySize];

            //     for (int i = 0; i < arraySize; i++)
            //     {
            //         string value = i >= columnNames.Length ? "" : columnNames[i];
            //         value = EditorGUILayout.TextField("Element " + i, value);
            //         newArray[i] = value;
            //     }

            //     columnNames = newArray;
            // }
        }
    }
}
