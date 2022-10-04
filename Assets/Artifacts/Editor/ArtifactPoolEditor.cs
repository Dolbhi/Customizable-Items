using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ColbyDoan
{
    [CustomEditor(typeof(ArtifactPools))]
    public class ArtifactPoolEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Sort"))
            {
                (target as ArtifactPools).Sort();
            }

            if (GUILayout.Button("Duplicate"))
            {
                ArtifactPools orginal = target as ArtifactPools;
                ArtifactPools newPool = ArtifactPools.CreateInstance<ArtifactPools>();

                newPool.untargeted.triggers = orginal.untargeted.triggers.DeepClone();
                newPool.untargeted.effects = orginal.untargeted.effects.DeepClone();
                newPool.targeted.triggers = orginal.targeted.triggers.DeepClone();
                newPool.targeted.effects = orginal.targeted.effects.DeepClone();

                string path = AssetDatabase.GetAssetPath(target);
                AssetDatabase.CreateAsset(newPool, System.IO.Path.GetDirectoryName(path) + "/New Artifact Pool.asset");
            }
        }
    }

    //[CustomPropertyDrawer(typeof(RankedPools))] USELESS ALL OF IT
    public class RankedPoolsDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("unranked")) + poolsHeight + EditorGUIUtility.singleLineHeight;
        }

        float poolsHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            SerializedProperty pools = property.FindPropertyRelative("pools");
            SerializedProperty unranked = property.FindPropertyRelative("unranked");

            // label
            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label);

            EditorGUI.indentLevel += 2;

            // unranked pool
            Rect fieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUI.GetPropertyHeight(unranked));
            EditorGUI.PropertyField(fieldRect, unranked);

            // set array size
            if (pools.arraySize != 5) pools.arraySize = 5;
            // pools

            poolsHeight = 0;
            fieldRect.position = new Vector2(fieldRect.position.x, fieldRect.position.y + fieldRect.height);
            for (int i = 0; i < 5; i++)
            {
                SerializedProperty pool = pools.GetArrayElementAtIndex(i).FindPropertyRelative("pool");

                fieldRect.height = EditorGUI.GetPropertyHeight(pool);
                poolsHeight += fieldRect.height;

                EditorGUI.PropertyField(fieldRect, pool);//((ItemType)i).ToString() ));
                fieldRect.position = new Vector2(fieldRect.position.x, fieldRect.position.y + fieldRect.height);
            }
            EditorGUI.indentLevel -= 2;
            EditorGUI.EndProperty();
        }
    }
}