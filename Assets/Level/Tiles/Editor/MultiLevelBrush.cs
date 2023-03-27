// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// using UnityEditor.ShortcutManagement;
using UnityEngine.Tilemaps;
using UnityEditor.Tilemaps;

namespace ColbyDoan
{
    [CustomGridBrush(false, true, false, "Multi-level brush")]
    [CreateAssetMenu(fileName = "New Multi Brush", menuName = "Custom Assets/Tiles/Create New Multi Brush")]
    public class MultiLevelBrush : GridBrushBase
    {
        public TileManager TileMan => FindObjectOfType<TileManager>();
        public TileBase[] tiles;
        [Range(0, 3)]
        public int level;
        public bool targetSingleLevel;

        const int FILL_LIMIT = 1000;

        void RegisterUndo(string label)
        {
            Object[] maps = new Object[4] { TileMan.mainTilemap, TileMan.colliderMaps[0], TileMan.colliderMaps[1], TileMan.colliderMaps[2] };
            Undo.RegisterCompleteObjectUndo(maps, label);
        }

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            RegisterUndo("Multilevel Painting");
            Paint(position);
        }
        void Paint(Vector3Int apparentPos)
        {
            PaintTrue(apparentPos + 2 * (level - 1) * Vector3Int.down);
        }
        void PaintTrue(Vector3Int truePos)
        {
            truePos.z = level - 1;

            TileManager tileManager = TileMan;
            if (targetSingleLevel)
            {
                tileManager.SetTile(truePos, tiles[truePos.z]);
                return;
            }
            for (; truePos.z >= 0; truePos.z--)
            {
                tileManager.SetTile(truePos, tiles[truePos.z]);
                // Undo.RegisterCompleteObjectUndo(tileManager.tilemaps[i].GetComponent<TilemapCollider2D>(), "Multilevel Painting");
            }
            for (truePos.z = level; truePos.z < 3; truePos.z++)
            {
                tileManager.SetTile(truePos, null);
                // Undo.RegisterCompleteObjectUndo(tileManager.tilemaps[i].GetComponent<TilemapCollider2D>(), "Multilevel Erasing");
            }
        }

        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            RegisterUndo("Multilevel box fill");
            for (int x = position.xMin; x < position.xMax; x++)
            {
                for (int y = position.yMin; y < position.yMax; y++)
                {
                    Paint(new Vector3Int(x, y, 0));
                }
            }
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            position.z = 0;
            position.y -= 2 * level;
            // Undo.RegisterCompleteObjectUndo(TileMan.tilemaps, "Multilevel Erasing");
            // if (targetSingleLevel)
            // {
            //     int index = level - 1;
            //     TileMan.tilemaps[index].SetTile(position, tiles[index]);
            //     return;
            // }
            for (int i = level; i < 3; i++)
            {
                TileMan.colliderMaps[i].SetTile(position, null);
                // Undo.RegisterCompleteObjectUndo(TileMan.tilemaps[i].GetComponent<TilemapCollider2D>(), "Multilevel Erasing");
            }
        }

        HashSet<Vector3Int> filledPos = new HashSet<Vector3Int>();
        public override void FloodFill(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            filledPos.Clear();

            position.y -= 4;
            RegisterUndo("Multilevel fill");

            // look for tiles starting from top most
            TileBase chosenTile = null;
            for (position.z = 2; position.z >= 0; position.z--)
            {
                chosenTile = TileMan.GetTile(position);
                if (chosenTile != null) break;
                position.y += 2;
            }

            // when all tiles are null
            if (chosenTile == null) position.z = 0;

            // start flood
            DoFloodFill(chosenTile, position);
        }
        void DoFloodFill(TileBase chosenTile, Vector3Int truePos)
        {
            // Debug.Log($"count:{filledPos.Count}");
            // fail conditions: too many iterations, already filled, tile at pos is not chosen tile, tiles above are not null
            if (filledPos.Count > FILL_LIMIT || filledPos.Contains(truePos) || TileMan.GetTile(truePos) != chosenTile) return;
            // fail if targeting single level and the tile on that level is the same as the tile to paint
            if (targetSingleLevel && TileMan.GetTile(new Vector3Int(truePos.x, truePos.y, level - 1)) == tiles[level - 1]) return;
            var copy = truePos;
            for (copy.z++; copy.z < 3; copy.z++)
            {
                // Debug.Log($"testing:{truePos}");
                if (TileMan.GetTile(copy) != null) return;
            }

            Paint(truePos + Vector3Int.up * 2 * (level - 1));
            filledPos.Add(truePos);

            DoFloodFill(chosenTile, truePos + Vector3Int.up);
            DoFloodFill(chosenTile, truePos + Vector3Int.down);
            DoFloodFill(chosenTile, truePos + Vector3Int.left);
            DoFloodFill(chosenTile, truePos + Vector3Int.right);
        }

        // [MenuItem("Assets/Create/Custom Assets/Create Multi-Level Brush")]
        // public static void CreateBrush()
        // {
        //     string path = EditorUtility.SaveFilePanelInProject("Save Multi-level Brush", "New Multi-level Brush", "Asset", "Save Multi-level Brush", "Assets");
        //     if (path == "")
        //         return;
        //     AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MultiLevelBrush>(), path);
        // }
    }

    [CustomEditor(typeof(MultiLevelBrush))]
    public class MultiLevelBrushEditor : GridBrushEditorBase
    {
        MultiLevelBrush Brush { get { return target as MultiLevelBrush; } }
        public override GameObject[] validTargets => new GameObject[] { Brush.TileMan?.gameObject };
        // bool wasExecuting = false;
        public override bool canChangeZPosition => false;

        public void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }
        public void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }
        // updates composite collider
        private void UndoRedoPerformed()
        {
            foreach (Tilemap tilemap in Brush.TileMan.colliderMaps)
            {
                tilemap.gameObject.SetActive(false);
                tilemap.gameObject.SetActive(true);
            }
        }

        // [Shortcut("Adjust brush height", typeof(TerrainToolShortcutContext), KeyCode.LeftBracket)]
        // static void BrushHeightShortcut(ShortcutArguments args)
        // {

        //     Brush.level--;
        // }

        BoundsInt lastBounds;
        public override void OnPaintSceneGUI(GridLayout grid, GameObject brushTarget, BoundsInt position, GridBrushBase.Tool tool, bool executing)
        {
            MultiLevelBrush brush = Brush;

            // account for visual displacement
            position.min -= 2 * (brush.level - 1) * Vector3Int.up;
            position.max -= 2 * (brush.level - 1) * Vector3Int.up;
            TileManager tileManager = brush.TileMan;

            // Debug.Log("min: " + position.min + " max: " + position.min);

            if (position != lastBounds)
            {
                tileManager.mainTilemap.ClearAllEditorPreviewTiles();
                // tileManager.colliderMaps[0].ClearAllEditorPreviewTiles();
                // tileManager.colliderMaps[1].ClearAllEditorPreviewTiles();
                // tileManager.colliderMaps[2].ClearAllEditorPreviewTiles();

                lastBounds = position;
            }

            // hoverPos = position.position;
            // botTile = tileManager.GetTile((Vector2Int)hoverPos, 0)?.name ?? "";
            // midTile = tileManager.GetTile((Vector2Int)hoverPos, 1)?.name ?? "";
            // topTile = tileManager.GetTile((Vector2Int)hoverPos, 2)?.name ?? "";
            // Debug.Log($"BOUNDS:{hoverPos} bot:{botTile} mid:{midTile} top:{topTile}");


            // List<TilemapCollider2D> colliders = new List<TilemapCollider2D>();
            // if (tool == GridBrushBase.Tool.Erase)
            // {
            // For erasing
            // foreach (Tilemap tilemap in tileManager.tilemaps)
            //     colliders.Add(tilemap.GetComponent<TilemapCollider2D>());
            // if (executing) Undo.RegisterCompleteObjectUndo(colliders.ToArray(), "Multilevel Erasing");
            // }
            // else
            // {

            // For other painting forms
            for (int i = brush.level - 1; i >= 0; i--)
            {
                // colliders.Add(currentMap.GetComponent<TilemapCollider2D>());

                foreach (Vector3Int pos in position.allPositionsWithin)
                {
                    var temp = pos;
                    temp.z = i;
                    tileManager.mainTilemap.SetEditorPreviewTile(temp, brush.tiles[temp.z]);
                }
                if (brush.targetSingleLevel) break;
            }
            // }
        }
        public Vector3Int hoverPos;
        public string topTile;
        public string midTile;
        public string botTile;

        // public override void OnInspectorGUI()
        // {
        //     DrawDefaultInspector();
        //     GUI.enabled = false;
        //     EditorGUILayout.Vector3IntField("Hover position", hoverPos);
        //     EditorGUILayout.LabelField(topTile);
        //     EditorGUILayout.LabelField(midTile);
        //     EditorGUILayout.LabelField(botTile);

        //     GUI.enabled = true;
        // }

        // Clear preview
        public override void OnMouseLeave()
        {
            Brush.TileMan.mainTilemap.ClearAllEditorPreviewTiles();
            // Debug.Log("Test");

            base.OnMouseLeave();
        }

        // public override void OnInspectorGUI()
        // {
        //     base.OnInspectorGUI();
        //     if (GUILayout.Button("Reset colliders"))
        //     {
        //         foreach (Tilemap tilemap in Brush.TileMan.tilemaps)
        //         {
        //             tilemap.gameObject.SetActive(false);
        //             tilemap.gameObject.SetActive(true);
        //         }
        //     }
        // }
    }
}