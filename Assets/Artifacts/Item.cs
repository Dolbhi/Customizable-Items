using System;
using UnityEngine;

using ColbyDoan.Attributes;

namespace ColbyDoan
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Custom Assets/Loot/Create Item")]
    /// Holds item info (such as id, type and if it is targeted)
    public class Item : ScriptableObject, IHasID
    {
        [IconSprite] public Sprite image;
        public ItemType type;
        [ReadOnly] public bool usesTarget;
        public ItemRank rank;
        [HideInInspector] public string idName;
        public string ID => idName;
        [TextArea] public string description;
        // [HideInInspector] public EffectModifier effectModifier;

        [SerializeField]
        [ReadOnly]
        bool isOriginal = true;

        public void OnValidate()
        {
            switch (type)
            {
                case ItemType.Effect:
                    usesTarget = ArtifactFactory.effectFactory.GetItem(idName)?.RequiresTarget ?? false;
                    break;
                case ItemType.Trigger:
                    usesTarget = ArtifactFactory.triggerFactory.GetItem(idName)?.HasTarget ?? false;
                    break;
                default:
                    usesTarget = false;
                    break;
            }

            //Debug.Log("Validate idName: " + idName + " rank: " + rank + " type: " + type + " targeted: " + usesTarget);
        }

        public Item Copy()
        {
            Item copy = Instantiate(this);
            copy.name = name;
            copy.isOriginal = false;
            return copy;
        }
    }

    public interface IHasID : IComparable<IHasID>
    {
        string ID { get; }

        int IComparable<IHasID>.CompareTo(IHasID obj)
        {
            return ID.CompareTo(obj.ID);
        }
    }

    public enum ItemType { Trigger, Effect, Curse }
    public enum ItemRank { D, C, B, A, S }

    public struct ItemRestriction
    {
        // list of requirements a given item must match
        // a targeted trigger is valid for both targeted and not
        public ItemType? type;
        public bool? usesTarget;
        public ItemRank? rank;

        /// <summary> Returns true if input does not violate restrictions </summary>
        public bool IsCompatible(Item item)
        {
            // checks type
            if (type != null && item.type != type) return false;
            // checks target
            if (usesTarget != null)
                if (item.type != ItemType.Trigger || !item.usesTarget)
                    if (item.usesTarget != (bool)usesTarget)
                        return false;
            // checks rank
            if (rank == null) return true;
            else if (item.rank != rank) return false;
            else return true;
        }

        /// <summary> Returns restrictions for what is compatible with this item </summary>
        public ItemRestriction(Item item, int rankModifier = 0)
        {
            // require opposing type
            // Targeted triggers have null target requirements
            usesTarget = null;
            if (item.type == ItemType.Trigger)
            {
                type = ItemType.Effect;
                if (!item.usesTarget) usesTarget = false;
            }
            else
            {
                type = ItemType.Trigger;
                usesTarget = item.usesTarget;
            }
            // must be same rank, barring rank mods
            rank = item.rank + rankModifier;
        }
        public ItemRestriction(ItemType? _type = null, bool? _usesTarget = null, ItemRank? _rank = null)
        {
            type = _type;
            usesTarget = _usesTarget;
            rank = _rank;
        }
    }
}