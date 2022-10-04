using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    // a way to sort and store item infos
    [CreateAssetMenu(fileName = "New Pool", menuName = "Custom Assets/Loot/Create Artifact Pool")]
    public class ArtifactPools : ScriptableObject
    {
        public TEPools untargeted = new TEPools();
        public TEPools targeted = new TEPools();

        public List<Item> toSort;

        public List<Item> GetRankList(bool isTargeted, bool isTrigger, ItemRank rank)
        {
            TEPools targetClass = isTargeted ? targeted : untargeted;
            RankedPools typeClass = isTrigger ? targetClass.triggers : targetClass.effects;
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
        public Item GetRandomItem(bool isTargeted, bool isTrigger, ItemRank rank)
        {
            List<Item> list = GetRankList(isTargeted, isTrigger, rank);

            if (list.Count == 0)
            {
                Debug.LogWarning("No item of stated type in pool");
                return null;
            }

            int randomIndex = Random.Range(0, list.Count);
            return list[randomIndex];
        }
        public Item GetRandomItem()
        {
            // trigger or effect
            List<Item> poolT;
            List<Item> poolUT;
            if (Random.Range(0, 2) == 0)
            {
                poolT = targeted.triggers.all;
                poolUT = untargeted.triggers.all;
            }
            else
            {
                poolT = targeted.effects.all;
                poolUT = untargeted.effects.all;
            }

            int count = poolT.Count + poolUT.Count;
            int dropIndex = Random.Range(0, count);

            return (dropIndex >= poolT.Count) ? poolUT[dropIndex - poolT.Count] : poolT[dropIndex];
        }

        public void AddItem(Item item)
        {
            if (item.usesTarget)
            {
                targeted.AddItem(item);
            }
            else
            {
                untargeted.AddItem(item);
            }
        }

        public void Sort()
        {
            // sort
            untargeted.Sort();
            targeted.Sort();

            // insert new
            foreach (Item item in toSort)
            {
                AddItem(item);
            }
            toSort.Clear();

            Debug.Log("sort complete");
        }

        [ContextMenu("Clear Pool")]
        public void Clear()
        {
            untargeted.Clear();
            targeted.Clear();
        }
    }

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

    [System.Serializable]
    public class TEPools
    {
        public RankedPools triggers;
        public RankedPools effects;

        public void AddItem(Item item)
        {
            switch (item.type)
            {
                case ItemType.Effect:
                    effects.AddItem(item);
                    break;
                case ItemType.Trigger:
                    triggers.AddItem(item);
                    break;
            }
        }

        public void Sort()
        {
            triggers.Sort();
            effects.Sort();
        }

        public void Clear()
        {
            triggers.Clear();
            effects.Clear();
        }
    }

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