// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;


namespace ColbyDoan
{
    [RequireComponent(typeof(Interactable))]
    public class ArtifactCase : MonoBehaviour, IArtifactCase
    {
        [SerializeField] Item item;
        public virtual Item CaseItem
        {
            get { return item; }
            set
            {
                item = value;
                if (item != null)
                {
                    _itemRenderer.SetItem(item);
                    interactable.hoverText = "Select " + item?.name;
                }
                else
                {
                    _itemRenderer.SetEmpty();
                    interactable.hoverText = "fuck";
                }
            }
        }
        [Header("References")]
        [SerializeField] protected ArtifactForge forge;
        [SerializeField] protected Interactable interactable;
        [SerializeField] ItemRenderer _itemRenderer;
        [SerializeField] UnityEngine.U2D.Animation.SpriteResolver _baseSprite, _pillarSprite, _bottomSprite;

        [Header("Info")]
        public ItemType type;
        protected bool selected;
        [SerializeField] EffectModifier _effectModifier;
        public EffectModifier Modifier => _effectModifier;

        [Header("Case reference")]
        [SerializeField] Transform itemCase;
        [SerializeField] Vector2 selectedPos = Vector2.zero;
        [SerializeField] Vector2 unselectedPos = new Vector2(0, -.43f);

        //Color unselectedColor = new Color(.8f, .8f, .8f);

        protected virtual void Awake()
        {
            forge.onUsedUp += ClearOut;
            forge.onFillCases += FillSelf;
            forge.onForge += OnForge;
            forge.onGraphicChange += UpdateSprites;
        }

        void Start()
        {
            UpdateSprites();
        }

#if UNITY_EDITOR
        Item last;
        void OnValidate()
        {
            if (item != last)
            {
                if (item)
                {
                    if (item.type != type) // type mismatch
                    {
                        Debug.LogWarning("Mismatched Item Type at " + gameObject.name);
                        CaseItem = null;
                    }
                    else
                    {
                        var newItem = Instantiate(item);
                        newItem.name = item.name;
                        CaseItem = newItem;
                    }
                }
                else CaseItem = null;
                last = item;
            }
            // UpdateSprites();
        }
#endif

        [ContextMenu("Update sprites")]
        void UpdateSprites()
        {
            if (!forge)
            {
                // Debug.LogError("No associated forge");
                return;
            }

            string catagory = forge.itemRestriction.rank == null ? "Base" : forge.itemRestriction.rank.ToString();
            string modifierPrefix, typePrefix;
            if (type == ItemType.Trigger)
            {
                modifierPrefix = "";
                typePrefix = "trigger ";
            }
            else
            {
                typePrefix = "effect ";
                modifierPrefix = Modifier switch
                {
                    EffectModifier.None => "",
                    EffectModifier.Broken => "broken ",
                    EffectModifier.Bundle => "bundle ",
                    _ => ""
                };
            }

            _baseSprite.SetCategoryAndLabel(catagory, modifierPrefix + "base");
            _pillarSprite.SetCategoryAndLabel(catagory, modifierPrefix + typePrefix + "pillar");
            _bottomSprite.SetCategoryAndLabel(catagory, "bottom");
        }

        public virtual void OnInteract(PlayerBehaviour playerBehaviour)
        {
            if (!item || item.idName == "")
            {
                Debug.Log("Valid item missing from case");

                Deselect();

                return;
            }

            // deselect current/self if neccessary
            if (!selected)
                Select();
            else
                Deselect();
        }

        // [ContextMenu("Select Case")]
        public void Select()
        {
            selected = true;
            // replace with self
            if (type == ItemType.Trigger)
            {
                forge.selectedTrigger?.Deselect();
                forge.selectedTrigger = this;
            }
            else
            {
                forge.selectedEffect?.Deselect();
                forge.selectedEffect = this;
            }
            interactable.hoverText = "Deselect " + item?.name;
            // update forge
            forge.UpdateGlow();
            // set graphic
            itemCase.localPosition = selectedPos;
            //pillar.color = selected ? Color.white : unselectedColor;
            //bg.color = selected ? Color.white : unselectedColor;
        }
        // [ContextMenu("Deselect Case")]
        public virtual void Deselect()
        {
            selected = false;
            // remove self
            if (forge.selectedTrigger == this as IArtifactCase)
            {
                forge.selectedTrigger = null;
            }
            else if (forge.selectedEffect == this as IArtifactCase)
            {
                forge.selectedEffect = null;
            }
            interactable.hoverText = "Select " + item?.name;
            // update forge
            forge.UpdateGlow();
            // set graphic
            itemCase.localPosition = unselectedPos;
            //pillar.color = selected ? Color.white : unselectedColor;
            //bg.color = selected ? Color.white : unselectedColor;
        }

        void ClearOut()
        {
            interactable.enabled = false;
            Deselect();
            //pillar.color = unselectedColor;
            //bg.color = unselectedColor;
            _itemRenderer.SetEmpty();
        }
        [ContextMenu("Fill self")]
        protected virtual void FillSelf(ItemRank rank, bool requiresTarget)
        {
            if (CaseItem) return;
            if (!forge)
            {
                Debug.LogError("No associated forge");
                return;
            }
            Item result = null;
            if (type == ItemType.Trigger)
            {
                var targetedTriggers = forge.pools.GetRankList(true, true, rank);
                var untargetedTriggers = forge.pools.GetRankList(false, true, rank);

                int targeteds = targetedTriggers.Count;
                int untargeteds = untargetedTriggers.Count;

                if (requiresTarget)
                {
                    int index = Random.Range(0, targeteds);
                    result = targetedTriggers[index];
                }
                else
                {
                    int index = Random.Range(0, targeteds + untargeteds);
                    if (index >= targeteds)
                    {
                        result = untargetedTriggers[index - targeteds];
                    }
                    else
                    {
                        result = targetedTriggers[index];
                    }
                }
            }
            else if (type == ItemType.Effect)
            {
                // Debug.Log($"{rank} minus {(int)Modifier} equals {rank - (int)Modifier}", this);
                result = forge.pools.GetRandomItem(requiresTarget, false, rank - (int)Modifier);
            }
            Item newItem = null;
            if (result)
            {
                newItem = Instantiate(result);
                newItem.name = result.name;
            }
            CaseItem = newItem;
        }
        protected virtual void OnForge()
        {
            // nothing (for now)
        }
        // public static implicit operator bool(ArtifactCase a) => a != null && a.item;
    }
}
