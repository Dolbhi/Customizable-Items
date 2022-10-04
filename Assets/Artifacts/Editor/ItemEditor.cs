using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ColbyDoan
{
    [CustomEditor(typeof(Item))]
    public class ItemEditor : Editor
    {
        //List<string> nameList = null;

        //void OnTypeChange(SerializedPropertyChangeEvent evt)
        //{
        //    nameList = ArtifactFactory.factoryFromItemType[(ItemType)evt.changedProperty.enumValueIndex]?.GetItemNames() ?? null;
        //}

        //public override VisualElement CreateInspectorGUI()
        //{
        //    var baseContainer = new VisualElement();

        //    SerializedProperty rank = serializedObject.FindProperty("rank");
        //    SerializedProperty usesTarget = serializedObject.FindProperty("usesTarget");
        //    SerializedProperty type = serializedObject.FindProperty("type");
        //    SerializedProperty name = serializedObject.FindProperty("idName");

        //    // Rank field
        //    var rankField = new PropertyField(rank);
        //    var targetField = new PropertyField(usesTarget);
        //    var typeField = new PropertyField(type);
        //    typeField.RegisterValueChangeCallback(OnTypeChange);

        //    baseContainer.Add(rankField);
        //    baseContainer.Add(targetField);
        //    baseContainer.Add(typeField);

        //    // Name field
        //    //var nameField = new PopupField<string>("Name", nameList, name.stringValue);
        //    ////nameField.BindProperty(name);
        //    //baseContainer.Add(nameField);


        //    return baseContainer;
        //}

        int nameIndex;
        List<string> itemNames;
        SerializedProperty type;
        //SerializedProperty idName;

        private void OnEnable()
        {
            type = serializedObject.FindProperty("type");
            //idName = serializedObject.FindProperty("idName");
        }

        public override void OnInspectorGUI()
        {
            Item item = target as Item;
            //Debug.Log("before idName: " + item.idName + " rank: " + item.rank + " type: " + item.type + " targeted: " + item.usesTarget);

            // default draws
            DrawDefaultInspector();
            //EditorGUILayout.PropertyField(usesTarget);

            // get corresponding name list based on type
            itemNames = ((ItemType)type.enumValueIndex) switch
            {
                ItemType.Effect => ArtifactFactory.factoryFromItemType[ItemType.Effect].GetItemNames(),
                ItemType.Trigger => ArtifactFactory.factoryFromItemType[ItemType.Trigger].GetItemNames(),
                _ => null,
            };

            // // Modifier
            // if (item.type == ItemType.Effect)
            // {
            //     item.effectModifier = (EffectModifier)EditorGUILayout.EnumPopup("Effect Modifier", item.effectModifier);
            // }

            // Item Name
            if (itemNames == null || itemNames.Count == 0)
            {
                EditorGUILayout.LabelField("There exists no items of this type");
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                nameIndex = itemNames.FindIndex((id) => id == item.idName);
                nameIndex = EditorGUILayout.Popup("Item name", nameIndex, itemNames.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    item.idName = itemNames[nameIndex];
                    item.OnValidate();
                }
                //Debug.Log("after " + ((Item)target).idName);
            }

            serializedObject.ApplyModifiedProperties();

            //Debug.Log("after idName: " + item.idName + " rank: " + item.rank + " type: " + item.type + " targeted: " + item.usesTarget);
        }
    }
}