// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{/// <summary> Misc. extensions </summary>
    public static class Extensions
    {
        static public Vector3Int GetXYOnly(this Vector3Int input)
        {
            return new Vector3Int(input.x, input.y, 0);
        }
        static public Vector3 ConvertToV3(this Vector2 vector, float z = 0)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        // components
        static public void SetZPosition(this Transform transform, float z)
        {
            Vector3 old = transform.position;
            old.z = z;
            transform.position = old;
        }
        static public Vector2 Get2DPos(this Transform transform)
        {
            return transform.position;
        }
        static public void SetSpriteAlpha(this SpriteRenderer spriteRenderer, float alpha)
        {
            Color old = spriteRenderer.color;
            old.a = alpha;
            spriteRenderer.color = old;
        }
        static public void SetSpriteAlpha(this UnityEngine.UI.Image image, float alpha)
        {
            Color old = image.color;
            old.a = alpha;
            image.color = old;
        }

        static public int Exclude(this LayerMask mask, int layer)
        {
            //Debug.Log(Convert.ToString(~(1 << layer), 2));
            return mask & ~(1 << layer);
        }
    }
}