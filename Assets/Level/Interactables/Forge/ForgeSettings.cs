using UnityEngine;

namespace ColbyDoan
{
    [CreateAssetMenu(fileName = "New Forge Settings", menuName = "Custom Assets/Loot/Create Forge Settings")]
    public class ForgeSettings : ScriptableObject
    {
        // Forge randomization
        const float TARGETED_CHANCE = .3f;
        /// <summary>
        /// {D, C, B, A, S}
        /// </summary>
        static readonly int[] RANK_WEIGHTS = new int[] { 8, 10, 10, 10, 5 };

        // Case randomization
        const float CUSTOM_CASE_CHANCE = .2f;
        /// <summary>
        /// {Broken weight, None weight, Bundle weight}
        /// </summary>
        static readonly int[] MOD_WEIGHTS = new int[] { 3, 1, 1 };
        /// <summary>
        /// {1, 2, 3}
        /// </summary>
        static readonly int[] CASE_COUNT_WEIGHTS = new int[] { 1, 1, 1 };

        WeightedRandomizer _rankRandomizer = new WeightedRandomizer(RANK_WEIGHTS);

        WeightedRandomizer _modRandomizer = new WeightedRandomizer(MOD_WEIGHTS);
        WeightedRandomizer _caseCountRandomizer = new WeightedRandomizer(CASE_COUNT_WEIGHTS);


        [SerializeField] ForgeData defaultData;

        [Header("Randomization options")]
        [SerializeField] bool randomizeTargeted;
        [SerializeField] bool randomizeRank;
        [SerializeField] bool randomizeTriggerCount;
        [SerializeField] bool randomizeEffectCount;
        // [SerializeField] bool allowCustom; idk test it first
        [SerializeField] ArtifactPools[] itemPools;
        ArtifactPools _combinedPool;

        void OnValidate()
        {
            _combinedPool = CreateInstance<ArtifactPools>();
            for (int i = 0; i < itemPools.Length; i++)
            {
                _combinedPool.CombinePool(itemPools[i]);
            }
        }

        public ForgeData GetData()
        {
            ForgeData output = defaultData;

            if (randomizeTargeted)
            {
                output.usesTarget = Random.value <= TARGETED_CHANCE;
            }
            if (randomizeRank)
            {
                output.rank = (ItemRank)_rankRandomizer.Choose();
            }

            // Case generation
            // enabling randomizeXCount will completely override default case data with an array of a random length
            if (randomizeTriggerCount)
            {
                // generate case count
                int caseCount = _caseCountRandomizer.Choose() + 1;
                output.triggerCases = new CaseData[caseCount];
            }
            if (randomizeEffectCount)
            {
                // generate case count
                int caseCount = _caseCountRandomizer.Choose() + 1;
                output.effectCases = new CaseData[caseCount];
            }

            /*Case Data Array Interpretation
            Array length: number of cases to generate
            Case data with doNotRandomize = false: completely random case
            Case data with custom = true and null item: generate random fitting item from itemPool
            */
            for (int i = 0; i < output.triggerCases.Length; i++)
            {
                // generate a case if null, else populate if not custom and preset with item
                CaseData data = output.triggerCases[i];
                if (data == null || !data.doNotRandomize)
                {
                    if (Random.value < CUSTOM_CASE_CHANCE)
                        output.triggerCases[i] = CaseData.CreateTriggerCase();
                    else
                        output.triggerCases[i] = CaseData.CreateTriggerCase(_combinedPool.GetForgeItem(output.usesTarget, true, output.rank));
                }
                else if (!data.custom && data.item == null)
                {
                    data.item = _combinedPool.GetForgeItem(output.usesTarget, true, output.rank);
                }
            }
            for (int i = 0; i < output.effectCases.Length; i++)
            {
                // generate a case if null, else populate if not custom and preset with item
                CaseData data = output.effectCases[i];
                if (data == null || !data.doNotRandomize)
                {
                    // modifier is restricted for S and D rank effects
                    EffectModifier mod = (EffectModifier)(_modRandomizer.Choose() - 1);
                    if (mod == EffectModifier.Bundle && output.rank == ItemRank.D) mod = EffectModifier.None;
                    if (mod == EffectModifier.Broken && output.rank == ItemRank.S) mod = EffectModifier.None;

                    if (Random.value < CUSTOM_CASE_CHANCE)
                        output.effectCases[i] = CaseData.CreateEffectCase(mod);
                    else
                        output.effectCases[i] = CaseData.CreateEffectCase(mod, _combinedPool.GetForgeItem(output.usesTarget, false, output.rank - (int)mod));
                }
                else if (!data.custom && data.item == null)
                {
                    data.item = _combinedPool.GetForgeItem(output.usesTarget, false, output.rank - (int)data.mod);
                }
            }

            return output;
        }
    }

    [System.Serializable]
    public struct ForgeData
    {
        // settings
        public bool reusable;
        public int costPerItem;
        public int costPerCustomItem;

        // item generation
        public bool usesTarget;
        public ItemRank rank;

        // case generation
        public CaseData[] triggerCases;
        public CaseData[] effectCases;
    }

    [System.Serializable]
    public class CaseData
    {
        public bool doNotRandomize = false;

        public bool custom;
        public EffectModifier mod;
        public Item item;

        /// <summary>
        /// Creates a CaseData for a trigger case with the given item, null item results in a custom case
        /// </summary>
        /// <param name="chosenItem"> Trigger item in case, null item results in custom case </param>
        /// <returns> Trigger case data </returns>
        public static CaseData CreateTriggerCase(Item chosenItem = null)
        {
            if (chosenItem == null) return new CaseData() { doNotRandomize = true, custom = true };
            if (chosenItem.type != ItemType.Trigger) Debug.LogError("Trying to make trigger case with non trigger item");
            return new CaseData() { doNotRandomize = true, item = chosenItem, custom = false };
        }

        /// <summary>
        /// Creates a CaseData for a effect case with the given item, null item results in a custom case
        /// </summary>
        /// <param name="chosenMod"> modifier of effect case </param>
        /// <param name="chosenItem"> effect item in case, null item results in custom case </param>
        /// <returns> Effect case data </returns>
        public static CaseData CreateEffectCase(EffectModifier chosenMod = EffectModifier.None, Item chosenItem = null)
        {
            if (chosenItem == null) return new CaseData() { doNotRandomize = true, mod = chosenMod, custom = true };
            if (chosenItem.type == ItemType.Trigger) Debug.LogError("Trying to make effect case with non effect item");
            return new CaseData() { doNotRandomize = true, mod = chosenMod, item = chosenItem, custom = false };
        }
    }
}