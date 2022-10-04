using UnityEngine;
using UnityEditor;

using ColbyDoan.Attributes;

namespace ColbyDoan
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

    [CustomPropertyDrawer(typeof(IconSpriteAttribute))]
    public class IconSpriteDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 64;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            // Debug.Log(EditorGUIUtility.labelWidth);
            EditorGUI.LabelField(position, label);
            position.min += (position.width - 64 - EditorGUIUtility.labelWidth) * Vector2.right;
            EditorGUI.ObjectField(position, property, typeof(Sprite));

            EditorGUI.EndProperty();
        }
    }

    // [CustomPropertyDrawer(typeof(CopyObjectAttribute))]
    // public class AttributeDrawers: PropertyDrawer
    // {
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         EditorGUI.ObjectField(position, property, property.GetType());
    //         property.objectReferenceValue = 
    //     }
    // }
}