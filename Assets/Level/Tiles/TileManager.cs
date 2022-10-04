using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

namespace ColbyDoan
{
    public class TileManager : Singleton<TileManager>
    {
        public Tilemap mainTilemap;

        public Tilemap[] colliderMaps = new Tilemap[3];
        // public Color[] tileColors = new Color[3];

        public Tilemap FloorTiles => colliderMaps[0];
        public Tilemap WallTiles => colliderMaps[1];
        public Tilemap DoubleTiles => colliderMaps[2];

        public AudioSource CrumbleSoundProp;

        public static event Action<Vector3Int, TileBase> OnTileChange = delegate { };

        public Dictionary<Vector3Int, Action<TileChange>> TileChangeCallbacks = new Dictionary<Vector3Int, Action<TileChange>>();

        void LateUpdate()
        {
            accumulativeDamage.Clear();
        }

        /// <summary> get the max level of tiles at a point, corresponds to actual world z level </summary>
        /// <param name="position"></param>
        /// <returns> tile height, pits return -1 and three tiles return 2 </returns>
        public int GetTerrainHeight(Vector2 position)
        {
            for (int i = 2; i >= 0; i--)
            {
                if ((GetTile(position, i) as ICustomTile)?.Traversible ?? false)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary> Raycasts for lack of solids one level lower </summary>
        /// <returns> Distance to pit, returns distance if none is found </returns>
        public float PitRaycast(Vector3 startPos, Vector2 direction, float distance, float depth)
        {
            direction.Normalize();
            int level = (int)depth;
            for (float pitCheckDist = .1f; pitCheckDist < distance; pitCheckDist += .1f)
            {
                // incement point to check in direction
                Vector2 point = direction * pitCheckDist + (Vector2)startPos;
                // check for pit
                if (GetTerrainHeight(point) < level)
                {
                    return pitCheckDist;
                }
            }
            return distance;
        }
        public float PitRaycast(Vector3 startPos, Vector2 direction, float distance)
        {
            return PitRaycast(startPos, direction, distance, startPos.z);
        }
        /// <summary> Linecasts for lack of solids one level lower </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns> Distance to pit, returns distance between start and end if none is found </returns>
        public float PitLinecast(Vector3 startPos, Vector2 endPos, float depth)
        {
            Vector2 direction = endPos - (Vector2)startPos;
            return PitRaycast(startPos, direction, direction.magnitude, depth);
        }
        public float PitLinecast(Vector3 startPos, Vector2 endPos)
        {
            return PitLinecast(startPos, endPos, startPos.z);
        }

        public void SetTile(Vector3Int tilePos, TileBase tile)
        {
            mainTilemap.SetTile(tilePos, tile);
            colliderMaps[tilePos.z].SetTile(tilePos.GetXYOnly(), tile);
            OnTileChange.Invoke(tilePos, tile);
        }
        public TileBase GetTile(Vector3Int tilePos)
        {
            return mainTilemap.GetTile(tilePos);
        }
        public TileBase GetTile(Vector2 position, int height) { return mainTilemap.GetTile(ConvertPos(position, height)); }

        // removes tile and the tiles it supports. also plays sfx
        public void DestroyTile(Vector3Int tilePos)
        {
            do
            {
                SetTile(tilePos, null);
                Instantiate(CrumbleSoundProp, tilePos, Quaternion.identity);
                tilePos.z++;
            }
            while (tilePos.z < 3);
        }
        public void DestroyTile(Vector2 position, int height) { DestroyTile(ConvertPos(position, height)); }

        Dictionary<Vector3Int, float> accumulativeDamage = new Dictionary<Vector3Int, float>();
        public float DamageTile(Vector3Int tilePos, float damage)
        {
            ICustomTile targetTile = GetTile(tilePos) as ICustomTile;

            // target is not a rule tile
            if (targetTile == null) return 0;

            // accumulate damage
            if (!accumulativeDamage.ContainsKey(tilePos)) accumulativeDamage.Add(tilePos, 0);

            accumulativeDamage[tilePos] += damage;

            // target gets destroyed
            if (accumulativeDamage[tilePos] > targetTile.Durability)
            {
                // deduct damage points used to destroy
                accumulativeDamage[tilePos] -= targetTile.Durability;

                if (targetTile.Downgrade)
                {
                    // target durability overcomed, downgrade target
                    SetTile(tilePos, targetTile.Downgrade);
                    // check new tile if it can be destroyed with leftover damage
                    damage = accumulativeDamage[tilePos];
                    accumulativeDamage[tilePos] = 0;
                    return DamageTile(tilePos, damage);
                }
                else
                {
                    // target has no downgrade, destroy target
                    DestroyTile(tilePos);
                    // return leftover
                    damage = accumulativeDamage[tilePos];
                    accumulativeDamage[tilePos] = 0;
                    return damage;
                }
            }

            return damage;
        }
        public float DamageTile(Vector2 position, int height, float damage) { return DamageTile(ConvertPos(position, height), damage); }

        public Vector3Int ConvertPos(Vector2 position, int height)
        {
            var pos = mainTilemap.LocalToCell(position);
            return new Vector3Int(pos.x, pos.y, height);
        }
        // public TilePosInfo ConvertPos(Vector3 position)
        // {
        //     int height = Mathf.CeilToInt(position.z);
        //     position.z = 0;
        //     return ConvertPos(position, height);
        // }
        // public struct TilePosInfo
        // {
        //     public int height;
        //     public Vector3Int position;
        // }

        public enum TileChange { Destroyed, Placed }

        // #if UNITY_EDITOR
        //         [ContextMenu("Update colliders")]
        //         void UpdateColliders()
        //         {
        //             foreach (Tilemap tilemap in tilemaps)
        //             {
        //                 TilemapCollider2D collider = tilemap.GetComponent<TilemapCollider2D>();
        //                 collider.enabled = false;
        //                 collider.enabled = true;
        //             }
        //         }
        // #endif
#if UNITY_EDITOR
        [ContextMenu("Copy collider tiles to mainMap")]
        void CopyCollTilesToMain()
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(new Tilemap[4] { mainTilemap, colliderMaps[0], colliderMaps[1], colliderMaps[2] }, "Undo collider copy to main");

            mainTilemap.ClearAllTiles();

            CopyMapToMain(colliderMaps[0], 0);
            CopyMapToMain(colliderMaps[1], 1);
            CopyMapToMain(colliderMaps[2], 2);
        }
        void CopyMapToMain(Tilemap map, int height)
        {
            var bounds = map.cellBounds;
            bounds.zMax = height;
            bounds.zMin = height;
            foreach (var pos in bounds.allPositionsWithin)
            {
                var temp = pos;
                temp.z = height;
                SetTile(temp, map.GetTile(pos));
            }
        }
        [ContextMenu("Set colliders")]
        void UpdateColliders()
        {
            colliderMaps[0].ClearAllTiles();
            colliderMaps[1].ClearAllTiles();
            colliderMaps[2].ClearAllTiles();

            var bounds = mainTilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                SetTile(pos, mainTilemap.GetTile(pos));
            }
        }
#endif

    }
    public interface ICustomTile
    {
        float Durability { get; }
        TileBase Downgrade { get; }
        bool Traversible { get; }
    }
}