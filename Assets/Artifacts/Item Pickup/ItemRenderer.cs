// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class ItemRenderer : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        public MaterialLibrary outlines;

        public void SetItem(Item item)
        {
            if (!item)
            {
                Debug.LogWarning("Item is null");
                return;
            }
            if (!item.image)
                Debug.LogWarning("Item has no icon");

            spriteRenderer.sprite = item.image;
            spriteRenderer.material = outlines[(int)item.rank];
        }

        public void SetEmpty()
        {
            spriteRenderer.sprite = null;
        }
    }
}
