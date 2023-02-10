using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    // a way to sort and store item infos
    [CreateAssetMenu(fileName = "New Pool", menuName = "Custom Assets/Loot/Create Artifact Pool")]
    public class ArtifactPools : ScriptableObject
    {
        public RankedPools untargetedT = new RankedPools();
        public RankedPools untargetedE = new RankedPools();
        public RankedPools targetedT = new RankedPools();
        public RankedPools targetedE = new RankedPools();

        public List<Item> toSort = new List<Item>();

        public List<Item> GetRankList(bool isTargeted, bool isTrigger, ItemRank rank)
        {
            RankedPools typeClass;
            if (isTargeted)
            {
                if (isTrigger)
                    typeClass = targetedT;
                else
                    typeClass = targetedE;
            }
            else
            {
                if (isTrigger)
                    typeClass = untargetedT;
                else
                    typeClass = untargetedE;
            }

            // TEPools targetClass = isTargeted ? targeted : untargeted;
            // RankedPools typeClass = isTrigger ? targetClass.triggers : targetClass.effects;
            return rank switch
            {
                ItemRank.D => typeClass.D,
                ItemRank.C => typeClass.C,
                ItemRank.B => typeClass.B,
                ItemRank.A => typeClass.A,
                ItemRank.S => typeClass.S,
                _ => null
            };
        }

        /// <summary>
        /// Gets random item of specified rank and type for forge
        /// </summary>
        /// <param name="targetProvided">item compatible with target being provided</param>
        /// <param name="isTrigger">item is a trigger</param>
        /// <param name="rank">item's rank</param>
        public Item GetForgeItem(bool targetProvided, bool isTrigger, ItemRank rank)
        {
            // create options list
            var items = new List<Item>();
            items.AddRange(GetRankList(targetProvided, isTrigger, rank));
            // target provided effects and no target provided triggers can include items of the other target type
            if (targetProvided != isTrigger)
                items.AddRange(GetRankList(!targetProvided, isTrigger, rank));
            // pick item from options
            int index = Random.Range(0, items.Count);
            return items[index];
        }
        /// <summary>
        /// Get random item out of specified ranked list
        /// </summary>
        /// <param name="isTargeted">item must use targets</param>
        /// <param name="isTrigger">item is a trigger</param>
        /// <param name="rank">item's rank</param>
        public Item GetRandomItem(bool isTargeted, bool isTrigger, ItemRank rank)
        {
            List<Item> list = GetRankList(isTargeted, isTrigger, rank);

            if (list.Count == 0)
            {
                Debug.LogWarning("No item of stated type in pool", this);
                return null;
            }

            int randomIndex = Random.Range(0, list.Count);
            return list[randomIndex];
        }
        /// <summary>
        /// Get any random item with equal chance
        /// </summary>
        public Item GetRandomItem()
        {
            // trigger or effect
            List<Item> allItems = new List<Item>();
            if (Random.Range(0, 2) == 0)
            {
                allItems.AddRange(targetedT.all);
                allItems.AddRange(untargetedT.all);
            }
            else
            {
                allItems.AddRange(targetedE.all);
                allItems.AddRange(untargetedE.all);
            }

            int dropIndex = Random.Range(0, allItems.Count);

            return allItems[dropIndex];
        }

        public void AddItem(Item item)
        {
            if (item.usesTarget)
            {
                // targeted.AddItem(item);
                if (item.type == ItemType.Trigger)
                    targetedT.AddItem(item);
                else
                    targetedE.AddItem(item);
            }
            else
            {
                // untargeted.AddItem(item);
                if (item.type == ItemType.Trigger)
                    untargetedT.AddItem(item);
                else
                    untargetedE.AddItem(item);
            }
        }

        public void CombinePool(ArtifactPools pool)
        {
            untargetedT.all.AddRange(pool.untargetedT.all);
            untargetedE.all.AddRange(pool.untargetedE.all);
            targetedT.all.AddRange(pool.targetedT.all);
            targetedE.all.AddRange(pool.targetedE.all);

            Sort();
        }

        public void Sort()
        {
            // sort
            untargetedT.Sort();
            targetedT.Sort();
            untargetedE.Sort();
            targetedE.Sort();

            // insert new
            foreach (Item item in toSort)
            {
                AddItem(item);
            }
            toSort.Clear();

            // Debug.Log("sort complete");
        }

        [ContextMenu("Clear Pool")]
        public void Clear()
        {
            untargetedT.Clear();
            targetedT.Clear();
            untargetedE.Clear();
            targetedE.Clear();
        }
    }

    /// <summary>
    /// Sorted collection of all types of artifact items of the same rank
    /// </summary>
    // [System.Serializable]
    // public class ArtifactPool
    // {
    //     public List<Item> TargetedT = new List<Item>();
    //     public List<Item> UntargetedT = new List<Item>();
    //     public List<Item> AllTriggers = new List<Item>();

    //     public List<Item> TargetedE = new List<Item>();
    //     public List<Item> UntargetedE = new List<Item>();
    //     public List<Item> AllEffects = new List<Item>();

    // public void AddItem(Item item)
    // {
    //     // find correct rank pool
    //     List<Item> toInsert = item.rank switch
    //     {
    //         ItemRank.D => D,
    //         ItemRank.C => C,
    //         ItemRank.B => B,
    //         ItemRank.A => A,
    //         ItemRank.S => S,
    //         _ => null
    //     };

    //     if (toInsert == null) return;

    //     // insert into all list
    //     int index = all.BinarySearch(item);
    //     if (index >= 0)
    //     {
    //         Debug.Log("item already present");
    //         return;
    //     }
    //     all.Insert(-index - 1, item);

    //     // insert into ranked list
    //     index = toInsert.BinarySearch(item);
    //     toInsert.Insert(-index - 1, item);
    // }

    // public void Sort()
    // {
    //     // sorts all lists
    //     var oldAll = all;

    //     TargetedT = new List<Item>(all.Count);
    //     UntargetedT.Clear();
    //     AllTriggers.Clear();
    //     TargetedE.Clear();
    //     UntargetedE.Clear();
    //     AllEffects.Clear();

    //     foreach (Item item in oldAll)
    //         AddItem(item);
    // }

    //     public void Clear()
    //     {
    //         TargetedT.Clear();
    //         UntargetedT.Clear();
    //         AllTriggers.Clear();
    //         TargetedE.Clear();
    //         UntargetedE.Clear();
    //         AllEffects.Clear();
    //     }

    //     public ArtifactPool DeepClone()
    //     {
    //         ArtifactPool output = new ArtifactPool();
    //         output.TargetedT.AddRange(TargetedT);
    //         output.UntargetedT.AddRange(UntargetedT);
    //         output.AllTriggers.AddRange(AllTriggers);
    //         output.TargetedE.AddRange(TargetedE);
    //         output.UntargetedE.AddRange(UntargetedE);
    //         output.AllEffects.AddRange(AllEffects);

    //         return output;
    //     }
    // }

    public struct ItemMask
    {
        int mask;

        public ItemMask(int _mask)
        {
            mask = _mask;
        }

        public bool TestItem(Item item)
        {
            return (~mask & ItemToMask(item)) == 0;
        }

        public static ItemMask ItemToMask(Item item)
        {
            int output = item.usesTarget ? 0b10000000 : 0;
            output |= 2 << (5 + (int)item.type);
            output |= 2 << (int)item.rank;

            return output;
        }

        public static readonly ItemMask all = new ItemMask(0b11111111);
        public static readonly ItemMask allRanks = new ItemMask(0b00011111);
        public static readonly ItemMask targeted = new ItemMask(0b10000000);

        public static implicit operator int(ItemMask mask) => mask.mask;
        public static implicit operator ItemMask(int mask) => new ItemMask(mask);
    }

    // [System.Serializable]
    // public class TEPools
    // {
    //     public RankedPools triggers;
    //     public RankedPools effects;

    //     public void AddItem(Item item)
    //     {
    //         switch (item.type)
    //         {
    //             case ItemType.Effect:
    //                 effects.AddItem(item);
    //                 break;
    //             case ItemType.Trigger:
    //                 triggers.AddItem(item);
    //                 break;
    //         }
    //     }

    //     public void Sort()
    //     {
    //         triggers.Sort();
    //         effects.Sort();
    //     }

    //     public void Clear()
    //     {
    //         triggers.Clear();
    //         effects.Clear();
    //     }
    // }

    [System.Serializable]
    public class RankedPools
    {
        public List<Item> D = new List<Item>();
        public List<Item> C = new List<Item>();
        public List<Item> B = new List<Item>();
        public List<Item> A = new List<Item>();
        public List<Item> S = new List<Item>();

        public List<Item> all = new List<Item>();

        public void AddItem(Item item)
        {
            // find correct rank pool
            List<Item> toInsert = item.rank switch
            {
                ItemRank.D => D,
                ItemRank.C => C,
                ItemRank.B => B,
                ItemRank.A => A,
                ItemRank.S => S,
                _ => null
            };

            if (toInsert == null) return;

            // insert into all list
            int index = all.BinarySearch(item);
            if (index >= 0)
            {
                Debug.Log("item already present");
                return;
            }
            all.Insert(-index - 1, item);

            // insert into ranked list
            index = toInsert.BinarySearch(item);
            toInsert.Insert(-index - 1, item);
        }

        public void Sort()
        {
            // sorts all lists
            var oldAll = all;

            all = new List<Item>(all.Count);
            D.Clear();
            C.Clear();
            B.Clear();
            A.Clear();
            S.Clear();

            foreach (Item item in oldAll)
                AddItem(item);
        }

        public void Clear()
        {
            all.Clear();
            D.Clear();
            C.Clear();
            B.Clear();
            A.Clear();
            S.Clear();
        }

        // class IDToCompare : IHasID
        // {
        //     public IDToCompare(string id)
        //     {
        //         idToCompare = id;
        //     }

        //     string idToCompare;
        //     public string ID => idToCompare;

        // }
        // public Item SearchByID(string id)
        // {
        //     var idToFind = new IDToCompare(id);

        //     int index = (all as List<IHasID>).BinarySearch(idToFind);
        //     if (index < 0)
        //     {
        //         Debug.Log("item not present");
        //         return null;
        //     }
        //     else return all[index];
        // }

        public RankedPools DeepClone()
        {
            RankedPools output = new RankedPools();
            output.all.AddRange(all);
            output.D.AddRange(D);
            output.C.AddRange(C);
            output.B.AddRange(B);
            output.A.AddRange(A);
            output.S.AddRange(S);

            return output;
        }
    }
}