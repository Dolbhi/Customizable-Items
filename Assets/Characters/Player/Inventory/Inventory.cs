using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace ColbyDoan
{
    public class Inventory : MonoBehaviour, IEnumerable<InventoryItem>
    {
        public RankedSubInventory untargetedTriggers;
        public RankedSubInventory targetedTriggers;
        public RankedSubInventory untargetedEffects;
        public RankedSubInventory targetedEffects;

        public event Action<Item> OnItemPickup;
        // public event Action OnInventoryChanged;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
        public IEnumerator<InventoryItem> GetEnumerator()
        {
            return new InventoryEnum(this);
        }

        public void AddItem(Item item, int count = 1)
        {
            switch (item.type)
            {
                case ItemType.Effect:
                    if (item.usesTarget)
                    {
                        targetedEffects.AddItem(item, count);
                    }
                    else
                    {
                        untargetedEffects.AddItem(item, count);
                    }
                    break;
                case ItemType.Trigger:
                    if (item.usesTarget)
                    {
                        targetedTriggers.AddItem(item, count);
                    }
                    else
                    {
                        untargetedTriggers.AddItem(item, count);
                    }
                    break;
            }
            OnItemPickup?.Invoke(item);
        }

        public bool TryRemoveItem(Item item)
        {
            switch (item.type)
            {
                case ItemType.Effect:
                    if (item.usesTarget)
                    {
                        return targetedEffects.TryRemoveItem(item) != 0;
                    }
                    else
                    {
                        return untargetedEffects.TryRemoveItem(item) != 0;
                    }
                case ItemType.Trigger:
                    if (item.usesTarget)
                    {
                        return targetedTriggers.TryRemoveItem(item) != 0;
                    }
                    else
                    {
                        return untargetedTriggers.TryRemoveItem(item) != 0;
                    }
                default:
                    return false;
            }
        }
    }

    public class InventoryEnum : IEnumerator<InventoryItem>
    {
        Inventory inventory;
        public InventoryItem Current { get; set; }
        object IEnumerator.Current => Current;

        int inventoryIndex = 0;
        ItemRank currentRank = ItemRank.D;
        int rankIndex = -1;

        public InventoryEnum(Inventory invent)
        {
            inventory = invent;
        }

        public bool MoveNext()
        {
            // Avoids going beyond the end of the collection.
            while (++rankIndex >= GetInventory(inventoryIndex).GetItemsOfRank(currentRank).Count)
            {
                rankIndex = -1;
                // Avoid going beyond the end of ranks
                if ((int)++currentRank >= 5)
                {
                    currentRank = ItemRank.D;
                    // Avoid going beyond the end of subinventories
                    if (++inventoryIndex >= 4)
                    {
                        return false;
                    }
                }
            }
            // Set current item to next item in collection.
            Current = GetInventory(inventoryIndex).GetItemsOfRank(currentRank)[rankIndex];
            return true;

        }
        RankedSubInventory GetInventory(int i)
        {
            return i switch
            {
                0 => inventory.untargetedTriggers,
                1 => inventory.targetedTriggers,
                2 => inventory.untargetedEffects,
                3 => inventory.targetedEffects,
                _ => null
            };
        }

        public void Reset()
        {
            inventoryIndex = 0;
            currentRank = ItemRank.D;
            rankIndex = -1;
        }

        void IDisposable.Dispose() { }
    }

    [System.Serializable]
    public class RankedSubInventory
    {
        public List<InventoryItem> D = new List<InventoryItem>();
        public List<InventoryItem> C = new List<InventoryItem>();
        public List<InventoryItem> B = new List<InventoryItem>();
        public List<InventoryItem> A = new List<InventoryItem>();
        public List<InventoryItem> S = new List<InventoryItem>();

        public int Count => D.Count + C.Count + B.Count + A.Count + S.Count;

        public List<InventoryItem> GetItemsOfRank(ItemRank rank)
        {
            return rank switch
            {
                ItemRank.D => D,
                ItemRank.C => C,
                ItemRank.B => B,
                ItemRank.A => A,
                ItemRank.S => S,
                _ => null
            };
        }

        public void AddItem(Item item, int count = 1, bool isInfinite = false)
        {
            if (!item) return;

            List<InventoryItem> subInventory = GetItemsOfRank(item.rank);
            InventoryItem newItem = new InventoryItem(item, count, isInfinite);

            int index = subInventory.BinarySearch(newItem);
            if (index >= 0)
            {
                // item already present
                InventoryItem inventorySlot = subInventory[index];

                if (inventorySlot.infinite) return;

                inventorySlot.count += count;
                inventorySlot.infinite = isInfinite;
                return;
            }

            // make new slot
            subInventory.Insert(-index - 1, newItem);
        }

        /// <returns> No. of items removed </returns>
        public int TryRemoveItem(Item item, int count = 1)
        {
            if (!item) return 0;

            List<InventoryItem> subInventory = GetItemsOfRank(item.rank);
            InventoryItem newItem = new InventoryItem(item, count);

            int index = subInventory.BinarySearch(newItem);
            if (index >= 0)
            {
                // item present
                var slot = subInventory[index];
                if (slot.infinite || slot.count > count)
                {
                    // more than enough items to remove
                    slot.count -= count;
                    return count;
                }
                // insufficient or just enough items to remove
                int output = slot.count;
                subInventory.RemoveAt(index);
                return output;
            }

            // unable to find
            return 0;
        }
    }

    [System.Serializable]
    public class InventoryItem : IComparable<InventoryItem>
    {
        // [HideInInspector] public RankedSubInventory currentSubInventory;
        public Item item;
        public int count;
        public bool infinite;

        public InventoryItem(Item item, int count = 1, bool infinite = false)
        {
            // this.currentSubInventory = currentSubInventory;
            this.item = item;
            this.count = infinite ? 1024 : count;
            this.infinite = infinite;
        }

        // public void ReduceCount(int count = 1)
        // {
        //     if (count >= this.count) currentSubInventory.GetInventory(item.rank).Remove(this);
        //     this.count -= count;
        // }

        public int CompareTo(InventoryItem obj)
        {
            return (item as IHasID).CompareTo(obj.item);
        }
    }
}
