using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace ColbyDoan
{
    public class SpriteManager : EditorWindow
    {
        static readonly string[] facings = { "fl", "lf", "lb", "bl", "br", "rb", "rf", "fr" };
        static readonly string[] strides = { "I", "i", "C", "D", "X", "U", "c", "d", "x", "u" };

        [MenuItem("Window/Sprite Editing Window")]
        public static void ShowWindow()
        {
            GetWindow(typeof(SpriteManager));
        }

        Vector2Int spriteSize = new Vector2Int(14, 24);
        Vector2Int offset = new Vector2Int(5, 5);
        Vector2Int padding = new Vector2Int(5, 5);
        Vector2 pivot = new Vector2(0.5f, 0);
        Texture2D myTexture;

        Vector2Int totalSpcaing => spriteSize + padding;

        string[] rowNames = facings;
        string[] columnNames = strides;
        bool rowsExpanded;
        bool columnsExpanded;

        UnityEngine.U2D.Animation.SpriteLibraryAsset spriteLibrary;

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

            // row and column names
            if (GUILayout.Button("Reset names"))
            {
                rowNames = facings;
                columnNames = strides;
            }
            rowsExpanded = EditorGUILayout.Foldout(rowsExpanded, "Row Names");
            if (rowsExpanded)
            {
                int arraySize = rowNames.Length;
                arraySize = EditorGUILayout.IntField("Size", arraySize);

                string[] newArray = new string[arraySize];

                for (int i = 0; i < arraySize; i++)
                {
                    string value = i >= rowNames.Length ? "" : rowNames[i];
                    value = EditorGUILayout.TextField("Element " + i, value);
                    newArray[i] = value;
                }

                rowNames = newArray;
            }
            columnsExpanded = EditorGUILayout.Foldout(columnsExpanded, "Column Names");
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

            spriteLibrary = EditorGUILayout.ObjectField("Sprite Library to merge", spriteLibrary, typeof(UnityEngine.U2D.Animation.SpriteLibraryAsset), false) as UnityEngine.U2D.Animation.SpriteLibraryAsset;

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

        void SliceSprite()
        {
            Undo.RecordObject(myTexture, "Slice sprites");

            string path = AssetDatabase.GetAssetPath(myTexture);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

            ti.isReadable = true;

            List<SpriteMetaData> newData = new List<SpriteMetaData>();

            int rows = rowNames.Length;
            int columns = columnNames.Length;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    SpriteMetaData smd = new SpriteMetaData();
                    smd.pivot = pivot;
                    smd.alignment = 9;
                    smd.name = rowNames[i] + "-" + columnNames[j];
                    smd.rect = new Rect(offset.x + j * totalSpcaing.x, offset.y + i * totalSpcaing.y, spriteSize.x, spriteSize.y);

                    newData.Add(smd);
                }
            }

            // update spritesheet
            ti.spritesheet = newData.ToArray();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            ti.isReadable = false;
        }
        void AddSpritesToLibrary(UnityEngine.U2D.Animation.SpriteLibraryAsset targetLibrary)
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
                targetLibrary.AddCategoryLabel(sprite, categoryAndLabel[0], categoryAndLabel[1]);
            }
        }
        void CreateLibrary()
        {
            UnityEngine.U2D.Animation.SpriteLibraryAsset newLibrary = CreateInstance<UnityEngine.U2D.Animation.SpriteLibraryAsset>();
            AddSpritesToLibrary(newLibrary);
            string path = AssetDatabase.GetAssetPath(myTexture);
            AssetDatabase.CreateAsset(newLibrary, System.IO.Path.GetDirectoryName(path) + "/Character Sprites.asset");
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
