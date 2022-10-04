// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ColbyDoan
{
    [CreateAssetMenu(fileName = "New Gameobject Tile", menuName = "Custom Assets/Create Gameobject Tile")]
    public class GameobjectTile : Tile
    {
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);

            tileData.flags = TileFlags.LockTransform;
            Matrix4x4 transform = new Matrix4x4();
            transform.SetTRS(Vector3.up * position.z, Quaternion.identity, Vector3.one);
            tileData.transform = transform;
        }
    }
}
