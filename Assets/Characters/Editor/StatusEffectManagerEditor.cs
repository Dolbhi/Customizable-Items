using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ColbyDoan
{
    [CustomEditor(typeof(StatusEffectsManager))]
    public class StatusEffectManagerEditor : Editor
    {
        bool showPosition;
        public override void OnInspectorGUI()
        {
            var activeEffects = (target as StatusEffectsManager).activeEffects;
            showPosition = EditorGUILayout.BeginFoldoutHeaderGroup(showPosition, "Statuses");
            if (showPosition)
            {
                foreach (IStatusEffect effect in activeEffects)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(effect.Name);
                    GUILayout.Label(effect.StackCount.ToString());
                    GUILayout.Label(effect.IsDebuff.ToString());
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
