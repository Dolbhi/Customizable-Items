using System;
using UnityEngine;

namespace ColbyDoan
{
    public class CaseManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] ItemStand casePrefab;
        // [SerializeField] ArtifactForge forge;
        Transform _caseParent;

        [SerializeField] ItemType caseType;
        public Vector2[] caseOffsets = new Vector2[] { new Vector2(1.6f, 2), new Vector2(1.6f, -2), new Vector2(3, 0) };

        public IItemStand selectedCase;
        public IItemStand[] cases;

        public Item SelectedItem => selectedCase?.CaseItem;

        public event Action OnSelectionChange;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataArray"></param>
        /// <returns>if all cases are custom</returns>
        public bool CreateCases(CaseData[] dataArray)
        {
            bool fullyCustom = true;

            // MAKE CASES
            int caseCount = dataArray.Length;
            cases = new IItemStand[caseCount];
            for (int i = 0; i < caseCount; i++)
            {
                // instantiate case at specific pos
                var newCase = Instantiate(casePrefab, _caseParent);
                Vector2 offset = caseCount == 1 ? caseOffsets[2] : caseOffsets[i];
                newCase.transform.SetLocalPositionAndRotation(offset, Quaternion.identity);

                newCase.SetUp(dataArray[i], caseType);
                newCase.OnCompleteInteraction += _OnCompleteInteraction;

                cases[i] = newCase;

                // disable fullyCustom if non custom case exists
                if (!dataArray[i].custom) fullyCustom = false;
            }

            // force selection if only 1 non-custom case
            if (!fullyCustom && caseCount == 1)
            {
                selectedCase = cases[0];
                selectedCase.DisableDeselect();
                OnSelectionChange.Invoke();
            }

            return fullyCustom;
        }

        void Awake()
        {
            _caseParent = transform;
        }

        /// <summary>
        /// Deselect and blank out all cases
        /// </summary>
        public void DisableAllCases()
        {
            selectedCase = null;
            for (int i = 0; i < cases.Length; i++)
            {
                cases[i].DisableCase();
            }
        }
        public void ClearCustomCases()
        {
            for (int i = 0; i < cases.Length; i++)
            {
                cases[i].UseUpCustomItem();
            }
            if (selectedCase?.CaseItem == null)
            {
                selectedCase = null;
            }
        }

        /// <summary>
        /// Update restriction to default data
        /// </summary>
        /// <param name="data"></param>
        public void UpdateRestrictions(bool? usesTarget = null, ItemRank? rank = null)
        {
            _UpdateCaseRestrictions(new ItemRestriction(caseType, usesTarget, rank));
        }
        /// <summary>
        /// Update restriction match opposing item
        /// </summary>
        /// <param name="opposingItem"></param>
        /// <param name="mod"></param>
        public void UpdateRestrictions(Item opposingItem, EffectModifier mod)
        {
            _UpdateCaseRestrictions(new ItemRestriction(opposingItem, ((int)mod)));
        }

        void _UpdateCaseRestrictions(ItemRestriction restriction)
        {
            // do nothing if not set up yet
            if (cases == null) return;
            for (int i = 0; i < cases.Length; i++)
            {
                cases[i].SetRestriction(restriction);
            }
        }

        void _OnCompleteInteraction(IItemStand newCase)
        {
            if (newCase.Selected)
            {
                // switch selection
                selectedCase?.Deselect();
                selectedCase = newCase;
                OnSelectionChange.Invoke();
            }
            else
            {
                // deselect only
                // selectedCase.Deselect();
                selectedCase = null;
                OnSelectionChange.Invoke();
            }
        }
    }
    public interface IItemStand
    {
        bool Selected { get; }
        bool IsCustom { get; }
        Item CaseItem { get; }
        EffectModifier Modifier { get; }
        // ItemType CaseType { get; }
        event Action<IItemStand> OnCompleteInteraction;
        void Deselect();
        void DisableDeselect();
        void DisableCase();
        void UseUpCustomItem();

        void SetUp(CaseData data, ItemType type);
        void SetRestriction(ItemRestriction restriction);
    }
}