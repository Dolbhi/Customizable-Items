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

        public IArtifactCase selectedTrigger;
        public IArtifactCase selectedEffect;

        [Header("Info")]
        [SerializeField] bool unrestricted;
        [SerializeField] bool usesTarget;
        [SerializeField] ItemRank rank;
        public ItemRestriction itemRestriction = new ItemRestriction();
        public bool reusable;

        [Header("Loot data")]
        public bool autoFillCases;
        public ArtifactPools pools;
        public event Action<ItemRank, bool> onFillCases;
        public event Action onUsedUp;
        public event Action onForge;
        public event Action onGraphicChange;

        bool _ReadyToForge => selectedTrigger != null && selectedEffect != null;

        void Start()
        {
            UpdateRestrictions();
            // set item restrictions
            if (!unrestricted)
            {
                // fill empty cases
                if (autoFillCases)
                {
                    onFillCases.Invoke(rank, usesTarget);
                }
            }
            UpdatePlatformSprite();
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
        [ContextMenu("Update Platform Sprite")]
        void UpdatePlatformSprite()
        {
            string catagory = itemRestriction.rank?.ToString() ?? "Base";
            // Debug.Log(itemRestriction.rank + catagory, this);
            platformSprite.SetCategoryAndLabel(catagory, "platform");
            onGraphicChange?.Invoke();
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

        // update restictions on custom artifact selected if unrestricted, else use default vars
        [ContextMenu("Update restriction")]
        public void UpdateRestrictions()
        {
            if (!unrestricted)
            {
                itemRestriction.usesTarget = usesTarget;
                itemRestriction.rank = rank;
                return;
            }
            if (selectedEffect != null)
            {
                itemRestriction = new ItemRestriction(selectedEffect.CaseItem, ((int)selectedEffect.Modifier));
                itemRestriction.type = null;
            }
            else if (selectedTrigger != null)
            {
                itemRestriction = new ItemRestriction(selectedTrigger.CaseItem);
                itemRestriction.type = null;
            }
            else
            {
                itemRestriction = new ItemRestriction();
            }
            UpdatePlatformSprite();
        }

        public void GivePlayerArtifact(PlayerBehaviour interacter)
        {
            if (!_ReadyToForge)
            {
                Debug.Log("trigger or effect not selected");
                return;
            }

            interacter.character.artifacts.Add(selectedTrigger.CaseItem, selectedEffect.CaseItem, selectedEffect.Modifier);

            onForge?.Invoke();
            if (!reusable)
            {
                onUsedUp?.Invoke();
                interactable.enabled = false;
            }
            UpdateGlow();
        }
    }

    public interface IArtifactCase
    {
        Item CaseItem { get; set; }
        EffectModifier Modifier { get; }
        void Deselect();
    }
}