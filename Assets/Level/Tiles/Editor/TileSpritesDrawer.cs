using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ColbyDoan
{
    [CustomPropertyDrawer(typeof(TileSprites))]
    public class TileSpritesDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Don't make child indented
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            SerializedProperty array = property.FindPropertyRelative("sprites");

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Rect rect = new Rect(position.x + i * 69, position.y + j * 69, 64, 64);
                    int index = i + j * 3;
                    array.GetArrayElementAtIndex(index).objectReferenceValue = EditorGUI.ObjectField(rect, array.GetArrayElementAtIndex(index).objectReferenceValue, typeof(Sprite), false);
                }
            }

            Vector2 newPos = new Vector2(position.x + 3 * 69 + 20, position.y);
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Rect rect = new Rect(newPos.x + i * 69, newPos.y + j * 69, 64, 64);
                    int index = i + j * 2 + 9;
                    array.GetArrayElementAtIndex(index).objectReferenceValue = EditorGUI.ObjectField(rect, array.GetArrayElementAtIndex(index).objectReferenceValue, typeof(Sprite), false);
                }
            }

            Rect lastRect = new Rect(newPos.x, newPos.y + 2 * 69, 64, 64);
            array.GetArrayElementAtIndex(13).objectReferenceValue = EditorGUI.ObjectField(lastRect, array.GetArrayElementAtIndex(13).objectReferenceValue, typeof(Sprite), false);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 69 * 3;
        }
    }
}
