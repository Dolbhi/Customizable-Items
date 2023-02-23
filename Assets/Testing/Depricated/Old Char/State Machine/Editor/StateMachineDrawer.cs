using UnityEngine;
using UnityEditor;

namespace ColbyDoan
{
    [CustomPropertyDrawer(typeof(StateMachine))]
    public class StateMachineDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.LabelField(position, label.text + " current state: " + property.FindPropertyRelative("currentStateName").stringValue);

            EditorGUI.EndProperty();
        }
    }
}