using System.Collections.Generic;
using System;
using UnityEngine;

namespace ColbyDoan
{
    using Attributes;

    public class ItemLibrary : ScriptableObject
    {
        public static ItemLibrary universal;

        public Map<string, ItemData> map;

        void OnValidate()
        {
            if (universal == null)
            {
                universal = this;
            }
            else if (universal != this)
            {
                Debug.Log("Multiple item librarys present", this);
            }
        }
    }

    public struct ItemData : IComparable<ItemData>
    {
        [IconSprite] public Sprite image;
        public string name;
        public ItemRank rank;
        [TextArea] public string description;

        [ReadOnly] public ItemType type;
        [ReadOnly] public bool usesTarget;
        [ReadOnly] public string idName;

        public void SetID(string id)
        {

        }

        public int CompareTo(ItemData other)
        {
            return idName.CompareTo(other.idName);
        }
    }
}
