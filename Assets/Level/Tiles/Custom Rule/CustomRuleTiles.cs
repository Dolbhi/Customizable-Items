using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ColbyDoan
{
    [CreateAssetMenu(fileName = "New Custom Rule Tile", menuName = "Custom Assets/Tiles/Create Custom Rule Tile")]
    public class CustomRuleTiles : AutoDisplacingTiles, ICustomTile
    {
        public TileSprites tileSprites;
        public float durability;
        public TileBase downgradedTile;
        public bool traversible = true;
        public List<TileBase> connectingTiles;
        HashSet<TileBase> setOfConnectingTiles;

        public bool fullTile;

        public float Durability => durability;
        public TileBase Downgrade => downgradedTile;
        public bool Traversible => traversible;

        static readonly Vector3Int[] neighbourOffsets = new Vector3Int[]
        {
            Vector3Int.left + Vector3Int.up,    Vector3Int.up,      Vector3Int.right + Vector3Int.up,
            Vector3Int.left,                                        Vector3Int.right,
            Vector3Int.left + Vector3Int.down,  Vector3Int.down,    Vector3Int.right + Vector3Int.down
        };
        static readonly Vector3Int occludingOffset = new Vector3Int(0, -1, 1);

        private void OnValidate()
        {
            setOfConnectingTiles = new HashSet<TileBase>(connectingTiles);
            setOfConnectingTiles.Add(this);
        }

        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            base.RefreshTile(position, tilemap);
            foreach (Vector3Int offset in neighbourOffsets)
            {
                tilemap.RefreshTile(position + offset);
            }
            tilemap.RefreshTile(position - occludingOffset);
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);

            tileData.colliderType = Tile.ColliderType.Sprite;

            int neighbourMask = 0;

            // Debug.Log(tileData.flags);
            // tileData.flags = TileFlags.LockTransform;
            // tileData.flags = TileFlags.None;
            // tileData.transform = Matrix4x4.identity;//test;

            int offsetCount = 1;
            for (int o = position.z; o < 2; o++)
            {
                var occludingTile = tilemap.GetTile<CustomRuleTiles>(position + offsetCount * occludingOffset);
                if (occludingTile && occludingTile.fullTile)
                {
                    tileData.sprite = null;
                    // Debug.Log(position);
                    return;
                }
                offsetCount++;
            }

            for (int i = 0; i < 8; i++)
            {
                if (setOfConnectingTiles.Contains(tilemap.GetTile(position + neighbourOffsets[i])))
                {
                    neighbourMask += 1 << i;
                }
            }
            // Debug.Log(System.Convert.ToString(neighbourMask, 2));
            tileData.sprite = tileSprites[GetSpriteIndex(neighbourMask)];
        }

        virtual protected int GetSpriteIndex(int neighbourMask)
        {
            switch (neighbourMask)
            {
                case (0b11111111): // missing none
                    return 4;
                case (0b11011111): // missing bottom left
                    return 11;
                case (0b01111111): // missing bottom right
                    return 12;
                case (0b11111011): // missing top right
                    return 10;
                case (0b11111110): // missing top left
                    return 9;
            }
            if ((~neighbourMask & 0b11111000) == 0) return 1; // top half
            if ((~neighbourMask & 0b00011111) == 0) return 7; // bottom half
            if ((~neighbourMask & 0b11010110) == 0) return 3; // left half
            if ((~neighbourMask & 0b01101011) == 0) return 5; // right half

            if ((~neighbourMask & 0b01101000) == 0) return 2; // top right
            if ((~neighbourMask & 0b11010000) == 0) return 0; // top left
            if ((~neighbourMask & 0b00010110) == 0) return 6; // bottom left
            if ((~neighbourMask & 0b00001011) == 0) return 8; // bottom right

            return 13;
        }

        // public Texture2D a;
        // public Sprite test;

        // public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        // {
        //     int damage = tilemap.GetComponent<TilemapData>().GetDamageData(position);
        // }
    }

    [System.Serializable]
    public class TileSprites
    {
        [SerializeField]
        Sprite[] sprites = new Sprite[14];

        public Sprite this[int i]
        {
            get { return sprites[i]; }
            set { sprites[i] = value; }
        }
    }
}
