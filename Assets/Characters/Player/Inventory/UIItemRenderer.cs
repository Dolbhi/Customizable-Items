// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColbyDoan
{
    public class UIItemRenderer : MonoBehaviour
    {
        public Image image;
        public MaterialLibrary outlines;

        public void SetItem(Sprite icon, ItemRank rank)
        {
            if (!icon)
                Debug.LogWarning("Item has no icon");

            image.enabled = true;
            image.sprite = icon;
            image.material = outlines[(int)rank];
        }
        public void SetItem(Item item)
        {
            if (!item)
            {
                Debug.LogWarning("Item is null");
                return;
            }

            SetItem(item.image, item.rank);
        }

        public void SetEmpty()
        {
            image.enabled = false;
        }
    }
}
