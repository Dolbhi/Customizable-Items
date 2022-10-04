// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

namespace ColbyDoan
{
    public class CustomArtifactCase : ArtifactCase
    {
        Inventory currentArtifactOwner; // problems arise with simultanious interactions

        public override Item CaseItem
        {
            get => base.CaseItem;
            set
            {
                base.CaseItem = value;
                interactable.hoverText = value ? "Remove " + value.name : "Choose item";
            }
        }

        protected override void FillSelf(ItemRank rank, bool requiresTarget) { }// dont self fill
        protected override void OnForge()
        {
            CaseItem = null;
            Deselect();
        }

        public override void OnInteract(PlayerBehaviour playerBehaviour)
        {
            if (selected)
            {
                Deselect();
            }
            else
            {
                // selection routine
                // pass callbacks to receive item from inventory and to clear callbacks when inventory is closed (i.e cancellation)
                currentArtifactOwner = playerBehaviour.inventory;

                InventoryUI.OnItemChosen += ItemChosenCallback;
                InventoryUI.OnInventoryClose += ClearCallbacks;

                // prompt inventory for appropriate item using forge restrictions + case type restriction
                var restriction = forge.itemRestriction;
                restriction.type = type;
                if (restriction.rank != null)
                    restriction.rank -= ((int)Modifier);
                InventoryUI.OnApproachCustomCase.Invoke(restriction);
            }
        }
        // return current artifact to owner and do base deselection
        public override void Deselect()
        {
            if (CaseItem != null)
            {
                currentArtifactOwner.AddItem(CaseItem);
                CaseItem = null;
            }
            base.Deselect();

            forge.UpdateRestrictions();
        }

        // receive item from inventory and pass on the new restriction to the forge
        void ItemChosenCallback(Item item)
        {
            // print(gameObject.name + " callback triggered");
            if (currentArtifactOwner.TryRemoveItem(item))
            {
                CaseItem = item;

                Select();

                forge.UpdateRestrictions();
            }
        }
        void ClearCallbacks()
        {
            // print(gameObject.name + " removal callback");
            InventoryUI.OnItemChosen -= ItemChosenCallback;
            InventoryUI.OnInventoryClose -= ClearCallbacks;
        }
    }
}