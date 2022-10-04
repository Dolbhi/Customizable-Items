// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ColbyDoan
{

    [CreateAssetMenu(fileName = "New 3D Tile", menuName = "Custom Assets/Tiles/Create Custom 3D Tile")]
    public class AutoDisplacingTiles : TileBase
    {
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.flags = TileFlags.LockTransform;
            Matrix4x4 transform = new Matrix4x4();
            transform.SetTRS(Vector3.up * position.z, Quaternion.identity, Vector3.one);
            tileData.transform = transform;
            // Debug.Log(tileData.transform);
        }
    }
}
