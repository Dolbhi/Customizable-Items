using System;

using UnityEngine;

namespace ColbyDoan
{
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
        ForgeData _data;

        readonly Vector2[] CASE_OFFSETS = new Vector2[] { new Vector2(1.6f, 2), new Vector2(1.6f, -2), new Vector2(3, 0) };

        // [Header("Info")]
        bool _fullyCustom = true;

        // public event Action onUsedUp;
        // public event Action onForge;
        // public event Action onGraphicChange;

        void Start()
        {
            _data = settings.GetData();

            // fully custom if all cases are custom
            _fullyCustom = triggersManager.CreateCases(_data.triggerCases);
            _fullyCustom = effectsManager.CreateCases(_data.effectCases) && _fullyCustom;

            // link selection events
            triggersManager.OnSelectionChange += _OnTriggerSelectionChange;
            effectsManager.OnSelectionChange += _OnEffectSelectionChange;

            // set restrictions
            if (_fullyCustom)
            {
                // set restrictions to none
                triggersManager.UpdateRestrictions();
                effectsManager.UpdateRestrictions();
                UpdatePlatformSprite();
            }
            else
            {
                // set restrictions to default
                triggersManager.UpdateRestrictions(_data.usesTarget ? true : null, _data.rank);
                effectsManager.UpdateRestrictions(_data.usesTarget ? null : false, _data.rank);
                UpdatePlatformSprite(_data.rank);
            }
        }

        /// <summary>
        /// Handle change in item selection
        /// </summary>
        /// <param name="change"> if an item is currently selected </param>
        void _OnTriggerSelectionChange(bool change)
        {
            // Update stuff
            UpdateGlow();

            // if custom update restrictions and sprite colour
            if (_fullyCustom)
            {
                if (change)
                {
                    // set restriction and colour to new trigger
                    effectsManager.UpdateRestrictions(triggersManager.SelectedItem);
                    UpdatePlatformSprite(triggersManager.SelectedItem.rank);
                }
                else
                {
                    // clear restrictions
                    effectsManager.UpdateRestrictions();
                    UpdatePlatformSprite();
                }
            }
        }
        void _OnEffectSelectionChange(bool change)
        {
            // Update stuff

            UpdateGlow();

            // if custom update restrictions and sprite colour
            if (_fullyCustom)
            {
                if (change)
                {
                    // set restriction and colour to new effect
                    triggersManager.UpdateRestrictions(effectsManager.SelectedItem, effectsManager.selectedCase.Modifier);
                    UpdatePlatformSprite(triggersManager.SelectedItem.rank - (int)effectsManager.selectedCase.Modifier);
                }
                else
                {
                    // clear restrictions
                    effectsManager.UpdateRestrictions();
                    UpdatePlatformSprite();
                }
            }
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
        void UpdatePlatformSprite(ItemRank? rank = null)
        {
            // set to "Base" of no rank given
            string catagory = rank?.ToString() ?? "Base";
            platformSprite.SetCategoryAndLabel(catagory, "platform");
            // onGraphicChange?.Invoke();
        }

        // set glow based on readiness to forge
        public void UpdateGlow()
        {
            if (_ReadyToForge)
            {
                interactable.enabled = true;
                glow.GlowActive = true;
            }
            else
            {
                interactable.enabled = false;
                glow.GlowActive = false;
            }
        }

        public void GivePlayerArtifact(PlayerBehaviour interacter)
        {
            if (!_ReadyToForge)
            {
                Debug.Log("trigger or effect not selected");
                return;
            }

            interacter.character.artifacts.Add(triggersManager.SelectedItem, effectsManager.SelectedItem, effectsManager.selectedCase.Modifier);

            // onForge?.Invoke();
            if (!_data.reusable)
            {
                // onUsedUp?.Invoke();
                // make everything uninteractable
                triggersManager.DisableAllCases();
                effectsManager.DisableAllCases();
                interactable.enabled = false;
            }
            UpdateGlow();
        }
    }
}