// using System;

using UnityEngine;

namespace ColbyDoan
{
    using Attributes;

    [RequireComponent(typeof(Interactable))]
    public class ArtifactForge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Interactable interactable;
        [SerializeField] PlatformGlow glow;
        [SerializeField] UnityEngine.U2D.Animation.SpriteResolver platformSprite;
        [SerializeField] CaseManager triggersManager;
        [SerializeField] CaseManager effectsManager;

        bool _ReadyToForge => triggersManager.selectedCase != null && effectsManager.selectedCase != null;

        public ForgeSettings settings;
        [SerializeField][ReadOnly] ForgeData _data;

        readonly Vector2[] CASE_OFFSETS = new Vector2[] { new Vector2(1.6f, 2), new Vector2(1.6f, -2), new Vector2(3, 0) };

        // [Header("Info")]
        bool _fullyCustom = true;

        // public event Action onUsedUp;
        // public event Action onForge;
        // public event Action onGraphicChange;

        void Start()
        {
            _data = settings.GetData();

            // link selection events
            triggersManager.OnSelectionChange += _OnItemSelectionChange;
            effectsManager.OnSelectionChange += _OnItemSelectionChange;

            // fully custom if all cases are custom
            bool a = triggersManager.CreateCases(_data.triggerCases);
            bool b = effectsManager.CreateCases(_data.effectCases);
            _fullyCustom = a && b;

            // set restrictions
            if (_fullyCustom)
            {
                // set restrictions to none
                triggersManager.UpdateRestrictions();
                effectsManager.UpdateRestrictions();
                _UpdatePlatformSprite();
            }
            else
            {
                // set restrictions to default
                triggersManager.UpdateRestrictions(_data.usesTarget ? true : null, _data.rank);
                effectsManager.UpdateRestrictions(_data.usesTarget ? null : false, _data.rank);
                _UpdatePlatformSprite(_data.rank);
            }
        }

        /// <summary>
        /// Handle change in item selection
        /// </summary>
        /// <param name="change"> if an item is currently selected </param>
        void _OnItemSelectionChange()
        {
            // Update stuff
            _UpdateInteractability();

            // if custom update restrictions and sprite colour
            if (_fullyCustom)
            {
                _UpdateRestrictions();
            }
        }
        void _UpdateRestrictions()
        {
            // find which case to follow restrictions
            IItemStand selectedCase;
            if (triggersManager.SelectedItem != null)
            {
                selectedCase = triggersManager.selectedCase;
            }
            else if (effectsManager.SelectedItem != null)
            {
                selectedCase = effectsManager.selectedCase;
            }
            else
            {
                // clear restriction
                triggersManager.UpdateRestrictions();
                effectsManager.UpdateRestrictions();

                _UpdatePlatformSprite();
                return;
            }

            // set restriction and colour to new trigger or effect
            triggersManager.UpdateRestrictions(selectedCase.CaseItem, selectedCase.Modifier);
            effectsManager.UpdateRestrictions(selectedCase.CaseItem, selectedCase.Modifier);

            _UpdatePlatformSprite(selectedCase.CaseItem.rank - (int)selectedCase.Modifier);
        }

        // void OnValidate()
        // {
        //     if (!Application.isPlaying)
        //     {
        //         if (!unrestricted)
        //         {
        //             itemRestriction.usesTarget = usesTarget;
        //             itemRestriction.rank = rank;
        //         }
        //         else
        //         {
        //             itemRestriction = new ItemRestriction();
        //         }
        //         Debug.Log("onvalidate restriction rank: " + itemRestriction.rank, this);
        //     }
        //     // update sprite if rank or restrictedness is changed
        //     UpdatePlatformSprite();
        // }
        // [ContextMenu("Update Platform Sprite")]
        void _UpdatePlatformSprite(ItemRank? rank = null)
        {
            // set to "Base" of no rank given
            string catagory = rank?.ToString() ?? "Base";
            platformSprite.SetCategoryAndLabel(catagory, "platform");
            // onGraphicChange?.Invoke();
        }

        int _cost;
        // set glow based on readiness to forge
        void _UpdateInteractability()
        {
            if (_ReadyToForge)
            {
                // set price
                _cost = triggersManager.selectedCase.IsCustom ? _data.costPerCustomItem : _data.costPerItem;
                _cost += effectsManager.selectedCase.IsCustom ? _data.costPerCustomItem : _data.costPerItem;
                interactable.hoverText = "Forge Artifact (" + _cost + "TB)";

                interactable.enabled = true;
                glow.GlowActive = true;
            }
            else
            {
                interactable.enabled = false;
                glow.GlowActive = false;
            }
        }

        /// <summary>
        /// Interaction trigger
        /// </summary>
        /// <param name="interacter"> Player who interacted </param>
        public void TryForge(PlayerBehaviour interacter)
        {
            if (!_ReadyToForge)
            {
                Debug.Log("trigger or effect not selected");
                return;
            }

            // Check balance
            if (!GameStats.TryDeductDataPoints(_cost))
            {
                // not enough xp
                return;
            }

            interacter.artifacts.Add(triggersManager.SelectedItem, effectsManager.SelectedItem, effectsManager.selectedCase.Modifier);

            // onForge?.Invoke();
            if (!_data.reusable)
            {
                // onUsedUp?.Invoke();
                // make everything uninteractable
                triggersManager.DisableAllCases();
                effectsManager.DisableAllCases();
                // interactable.enabled = false;
                // glow.GlowActive = false;
            }

            // reset for new forge
            triggersManager.ClearCustomCases();
            effectsManager.ClearCustomCases();
            _UpdateInteractability();
            _UpdateRestrictions();
        }
    }
}