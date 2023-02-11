using System;
// using System.Collections.Generic;
using UnityEngine;


namespace ColbyDoan
{
    [RequireComponent(typeof(Interactable))]
    public class ArtifactCase : MonoBehaviour, INewArtifactCase
    {
        [Header("References")]
        [SerializeField] Interactable interactable;
        [SerializeField] GameObject glassCase;
        [SerializeField] ItemRenderer itemRenderer;
        [SerializeField] UnityEngine.U2D.Animation.SpriteResolver baseSprite, pillarSprite, bottomSprite;

        [Header("Case reference")]
        [SerializeField] Transform itemCase;
        [SerializeField] Vector2 selectedPos = Vector2.zero;
        [SerializeField] Vector2 unselectedPos = new Vector2(0, -.43f);

        Item _item;
        CaseData _data;
        ItemType _type;
        bool _selected;
        ItemRestriction _restriction;

        /// <summary>
        /// Setting also sets itemRenderer and hovertext of interaction
        /// </summary>
        public Item CaseItem => _item;
        public EffectModifier Modifier => _data.mod;
        public ItemType CaseType => _type;
        public bool Selected => _selected;

        public event Action<INewArtifactCase> OnCompleteInteraction;

        /// <summary>
        /// Owner to return item to
        /// </summary>
        Inventory _currentItemOwner; // problems arise with simultanious interactions

        //Color unselectedColor = new Color(.8f, .8f, .8f);

        // interaction interface
        public void OnInteract(PlayerBehaviour playerBehaviour)
        {
            // deselect current/self if neccessary
            if (_selected)
            {
                Deselect();
                OnCompleteInteraction.Invoke(this);
            }
            else if (_data.custom)
            {
                // custom selection routine
                // pass callbacks to receive item from inventory and to clear callbacks when inventory is closed (i.e cancellation)
                _currentItemOwner = playerBehaviour.inventory;

                InventoryUI.OnItemChosen += _ItemChosenCallback;
                InventoryUI.OnInventoryClose += _ClearCallbacks;

                // prompt inventory for appropriate item using forge restrictions + case type restriction
                InventoryUI.OnApproachCustomCase.Invoke(_restriction.IsCompatible);
            }
            else
            {
                Select();
                OnCompleteInteraction.Invoke(this);
            }

        }

        public void SetUp(CaseData data, ItemType type)
        {
            _data = data;
            _type = type;
            // _forge = forge;

            if (_data.custom)
            {
                interactable.hoverText = "Choose item from inventory";
            }
            else
            {
                _item = _data.item.Copy();
                _UpdateItemDisplayAndHover();
            }
        }
        public void SetRestriction(ItemRestriction newRestriction)
        {
            _restriction = newRestriction;

            _UpdateSprites(_restriction.rank);

            // apply mods
            if (_restriction.rank != null)
            {
                _restriction.rank -= (int)_data.mod;
            }
        }

        // update state
        public void Select()
        {
            _selected = true;
            interactable.hoverText = "Deselect " + _item?.name;
            // set graphic
            itemCase.localPosition = selectedPos;
            //pillar.color = selected ? Color.white : unselectedColor;
            //bg.color = selected ? Color.white : unselectedColor;
        }
        // update state
        public void Deselect()
        {
            // return item if present
            if (_data.custom && CaseItem != null)
            {
                _currentItemOwner.AddItem(CaseItem);
                _item = null;
                _UpdateItemDisplayAndHover();
            }

            _selected = false;
            interactable.hoverText = "Select " + _item?.name;
            // set graphic
            itemCase.localPosition = unselectedPos;
            //pillar.color = selected ? Color.white : unselectedColor;
            //bg.color = selected ? Color.white : unselectedColor;
        }

        public void DisableCase()
        {
            interactable.enabled = false;
            Deselect();
            //pillar.color = unselectedColor;
            //bg.color = unselectedColor;
            itemRenderer.SetEmpty();
        }

        public void UseUpCustomItem()
        {
            if (_data.custom)
            {
                _item = null;
                _UpdateItemDisplayAndHover();
                Deselect();
            }
        }

        void _UpdateSprites(ItemRank? rank = null)
        {
            string catagory = rank == null ? "Base" : rank.ToString();
            string modifierPrefix, typePrefix;
            if (_type == ItemType.Trigger)
            {
                typePrefix = "trigger ";
                modifierPrefix = "";
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

            glassCase.SetActive(!_data.custom);

            baseSprite.SetCategoryAndLabel(catagory, modifierPrefix + "base");
            pillarSprite.SetCategoryAndLabel(catagory, modifierPrefix + typePrefix + "pillar");
            bottomSprite.SetCategoryAndLabel(catagory, "bottom");
        }

        void _UpdateItemDisplayAndHover()
        {
            if (_data.custom)
            {
                // displays for custom item
                if (_item != null)
                {
                    itemRenderer.SetItem(_item);
                    interactable.hoverText = "Remove " + _item.name;
                }
                else
                {
                    itemRenderer.SetEmpty();
                    interactable.hoverText = "Choose item from inventory";
                }
            }
            else
            {
                // displays for custom item
                if (_item != null)
                {
                    itemRenderer.SetItem(_item);
                    interactable.hoverText = "Select " + _item.name;
                }
                else
                {
                    itemRenderer.SetEmpty();
                    interactable.enabled = false;
                }
            }
        }

        void _ItemChosenCallback(Item item)
        {
            // print(gameObject.name + " callback triggered");
            if (_currentItemOwner.TryRemoveItem(item))
            {
                _item = item;
                _UpdateItemDisplayAndHover();

                Select();
                OnCompleteInteraction.Invoke(this);
            }
        }
        void _ClearCallbacks()
        {
            // print(gameObject.name + " removal callback");
            InventoryUI.OnItemChosen -= _ItemChosenCallback;
            InventoryUI.OnInventoryClose -= _ClearCallbacks;
        }
    }
}
