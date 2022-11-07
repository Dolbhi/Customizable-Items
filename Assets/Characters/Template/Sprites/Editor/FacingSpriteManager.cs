using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace ColbyDoan
{
    public class FacingSpriteManager : EditorWindow
    {
        static readonly string[] facings = { "rb", "br", "bl", "lb", "lf", "fl", "fr", "rf" };
        static readonly string[] strides = { "I", "i", "C", "D", "X", "U", "c", "d", "x", "u" };

        [MenuItem("Window/Facing Sprite Editing Window")]
        public static void ShowWindow()
        {
            GetWindow(typeof(FacingSpriteManager));
        }

        Vector2Int spriteSize = new Vector2Int(14, 24);
        Vector2Int offset = new Vector2Int(5, 5);
        Vector2Int padding = new Vector2Int(5, 5);
        Vector2 pivot = new Vector2(0.5f, 0);
        Texture2D myTexture;

        Vector2Int totalSpcaing => spriteSize + padding;

        // string[] rowNames = facings;
        FacingDirections initialFacing = FacingDirections.fl;
        bool anticlockwise = false;
        int facingsCount = 8;
        string[] columnNames = strides;
        bool rowsExpanded;
        bool columnsExpanded;

        FacingSpritesLibrary spriteLibrary;

        void OnGUI()
        {
            // get selected
            Object obj = Selection.activeObject;
            if (obj && obj.GetType() == typeof(Texture2D))
            {
                myTexture = obj as Texture2D;
            }
            else
            {
                myTexture = null;
            }

            initialFacing = (FacingDirections)EditorGUILayout.EnumPopup("Facing of first row", initialFacing);
            anticlockwise = EditorGUILayout.Toggle("Facing turn direction", anticlockwise);

            // label names
            if (GUILayout.Button("Reset names"))
            {
                // rowNames = facings;
                columnNames = strides;
            }
            // rowsExpanded = EditorGUILayout.Foldout(rowsExpanded, "Row Names");
            // if (rowsExpanded)
            // {
            //     int arraySize = rowNames.Length;
            //     arraySize = EditorGUILayout.IntField("Size", arraySize);

            //     string[] newArray = new string[arraySize];

            //     for (int i = 0; i < arraySize; i++)
            //     {
            //         string value = i >= rowNames.Length ? "" : rowNames[i];
            //         value = EditorGUILayout.TextField("Element " + i, value);
            //         newArray[i] = value;
            //     }

            //     rowNames = newArray;
            // }
            columnsExpanded = EditorGUILayout.Foldout(columnsExpanded, "Label Names");
            if (columnsExpanded)
            {
                int arraySize = columnNames.Length;
                arraySize = EditorGUILayout.IntField("Size", arraySize);

                string[] newArray = new string[arraySize];

                for (int i = 0; i < arraySize; i++)
                {
                    string value = i >= columnNames.Length ? "" : columnNames[i];
                    value = EditorGUILayout.TextField("Element " + i, value);
                    newArray[i] = value;
                }

                columnNames = newArray;
            }

            spriteSize = EditorGUILayout.Vector2IntField("Sprite Size", spriteSize);
            offset = EditorGUILayout.Vector2IntField("Offset", offset);
            padding = EditorGUILayout.Vector2IntField("Padding", padding);
            pivot = EditorGUILayout.Vector2Field("Pivot", pivot);

            spriteLibrary = EditorGUILayout.ObjectField("Sprite Library to merge", spriteLibrary, typeof(FacingSpritesLibrary), false) as FacingSpritesLibrary;

            // The following is only enabled while a texture is selected
            GUI.enabled = myTexture;
            //if (GUILayout.Button("Set Texture Settings"))
            //{
            //    myTexture.filterMode = FilterMode.Point;
            //    myTexture.
            //}
            if (GUILayout.Button("Slice Texture"))
            {
                SliceSprite();
            }
            if (GUILayout.Button(spriteLibrary ? "Merge with library" : "Create Library"))
            {
                if (spriteLibrary)
                {
                    AddSpritesToLibrary(spriteLibrary);
                }
                else
                {
                    CreateLibrary();
                }
            }
            GUI.enabled = true;
        }
        // slice sprites and name them "[facing]-[label]" where facing is an int from 0-8, and label is user defined
        void SliceSprite()
        {
            Undo.RecordObject(myTexture, "Slice sprites");

            string path = AssetDatabase.GetAssetPath(myTexture);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

            // AssetDatabase.DeleteAsset(path);

            ti.isReadable = true;

            // ti.spritesheet = new SpriteMetaData[0];
            // AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            // int rows = rowNames.Length;
            int columns = columnNames.Length;

            List<SpriteMetaData> newData = new List<SpriteMetaData>(facingsCount * columns);

            int rotation = anticlockwise ? 1 : -1;

            for (int i = 0; i < facingsCount; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    SpriteMetaData smd = new SpriteMetaData();
                    smd.pivot = pivot;
                    smd.alignment = 9;
                    smd.name = ((int)initialFacing + i * rotation + facingsCount) % facingsCount + "-" + columnNames[j];
                    smd.rect = new Rect(offset.x + j * totalSpcaing.x, offset.y + i * totalSpcaing.y, spriteSize.x, spriteSize.y);

                    newData.Add(smd);
                }
            }

            // update spritesheet
            ti.spritesheet = newData.ToArray();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            ti.isReadable = false;
        }
        void AddSpritesToLibrary(FacingSpritesLibrary targetLibrary)
        {
            Undo.RecordObject(spriteLibrary, "Add new sprites to library");

            string path = AssetDatabase.GetAssetPath(myTexture);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);

            // pick out sprites
            Sprite[] sprites = new Sprite[objects.Length];

            int n = 0;
            foreach (Object obj in objects)
            {
                if (obj.GetType() == typeof(Sprite))
                {
                    sprites[n] = obj as Sprite;
                    n++;
                }
            }
            //Debug.Log(n);

            // Place sprite in library using the first 2 letters of its name as the category and the remaining letters from the 4th letter onwards as the label
            foreach (Sprite sprite in sprites)
            {
                if (sprite == null) continue;
                var categoryAndLabel = sprite.name.Split('-');
                // Debug.Log(sprite.name);
                targetLibrary.AddSprite(sprite, (FacingDirections)int.Parse(categoryAndLabel[0]), categoryAndLabel[1]);
            }
        }
        void CreateLibrary()
        {
            FacingSpritesLibrary newLibrary = CreateInstance<FacingSpritesLibrary>();
            string path = AssetDatabase.GetAssetPath(myTexture);
            AssetDatabase.CreateAsset(newLibrary, System.IO.Path.GetDirectoryName(path) + "/Character Sprites.asset");
            AddSpritesToLibrary(newLibrary);
        }

        //string ExtractLabel(string name)
        //{
        //    string output = "";

        //    int length = name.Length;
        //    for (int i = 0; i < length; i++)
        //    {
        //        if (name[i] != '_') continue;
        //        name.Substring()
        //    }
        //}
    }
}
