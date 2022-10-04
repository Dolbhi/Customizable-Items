using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System;
// using System.Diagnostics;

namespace ColbyDoan
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] InputAction showInventoryAction;

        [SerializeField] Transform triggerParent;
        [SerializeField] Transform effectParent;
        [SerializeField] CanvasGroup canvasGroup;

        [SerializeField] SlotUI slotProp;

        public Inventory playerInventory;

        public static Action<ItemRestriction> OnApproachCustomCase;
        public static event Action<Item> OnItemChosen;
        public static event Action OnInventoryClose = delegate { };

        List<SlotUI> triggerSlots = new List<SlotUI>(0);
        List<SlotUI> effectSlots = new List<SlotUI>(0);

        private void Awake()
        {
            playerInventory.OnItemPickup += (_) => { UpdateUI(); };

            showInventoryAction.started += ShowInventory;
            showInventoryAction.canceled += HideInventory;

            OnApproachCustomCase = OpenForItemSelection;
        }

        void ShowInventory(InputAction.CallbackContext ctx = new InputAction.CallbackContext())
        {
            // cancel if already open
            if (canvasGroup.alpha == 1) return;

            UpdateUI();

            canvasGroup.DOFade(1, .1f).Play();
            canvasGroup.blocksRaycasts = true;
        }
        void HideInventory(InputAction.CallbackContext ctx = new InputAction.CallbackContext())
        {
            var fade = canvasGroup.DOFade(0, .1f);
            fade.onComplete += OnInventoryClose.Invoke;
            fade.Play();
            canvasGroup.blocksRaycasts = false;
        }

        void OpenForItemSelection(ItemRestriction restriction)
        {
            ShowInventory();
            // enable relavant slots
            foreach (SlotUI slot in triggerSlots)
            {
                slot.button.enabled = true;
                if (restriction.IsCompatible(slot.CurrentItem))
                {
                    slot.button.interactable = true;
                }
                else
                {
                    slot.button.interactable = false;
                }
            }
            foreach (SlotUI slot in effectSlots)
            {
                slot.button.enabled = true;
                if (restriction.IsCompatible(slot.CurrentItem))
                {
                    slot.button.interactable = true;
                }
                else
                {
                    slot.button.interactable = false;
                }
            }
        }
        void OnSlotClicked(SlotUI slot)
        {
            HideInventory();
            OnItemChosen?.Invoke(slot.CurrentItem);
            OnItemChosen = null;
        }

        ItemRank[] ranks = (ItemRank[])System.Enum.GetValues(typeof(ItemRank));
        void UpdateUI()
        {
            // Stopwatch timer = new Stopwatch();
            // timer.Start();

            // TRIGGERS
            // match amount of trigger slots to trigger items
            int triggerCount = playerInventory.targetedTriggers.Count + playerInventory.untargetedTriggers.Count;
            // int i = 0;
            while (triggerCount > triggerSlots.Count)
            {
                // i++;
                // if (i > 100) break;
                // print("Loop 1");
                SlotUI newSlot = Instantiate<SlotUI>(slotProp, triggerParent);
                // link all slots to on slot clicked
                newSlot.button.onClick.AddListener(delegate { OnSlotClicked(newSlot); });
                triggerSlots.Add(newSlot);
            }
            // fill slots
            int slotIndex = 0;
            foreach (ItemRank rank in ranks)
            {
                // i++;
                // if (i > 100) break;
                // print("Loop 2");
                SetSlotsFromList(ref slotIndex, triggerSlots, playerInventory.untargetedTriggers.GetItemsOfRank(rank));
                SetSlotsFromList(ref slotIndex, triggerSlots, playerInventory.targetedTriggers.GetItemsOfRank(rank));
            }
            // disable remaining
            while (slotIndex < triggerSlots.Count)
            {
                // i++;
                // if (i > 100) break;
                // print("Loop 3");
                triggerSlots[slotIndex].gameObject.SetActive(false);
                slotIndex++;
            }

            // EFFECTS
            // match amount of effect slots to effect items
            int effectCount = playerInventory.targetedEffects.Count + playerInventory.untargetedEffects.Count;
            while (effectCount > effectSlots.Count)
            {
                // i++;
                // if (i > 100) break;
                SlotUI newSlot = Instantiate<SlotUI>(slotProp, effectParent);
                // link all slots to on slot clicked
                newSlot.button.onClick.AddListener(delegate { OnSlotClicked(newSlot); });
                effectSlots.Add(newSlot);
            }
            // fill slots
            slotIndex = 0;
            foreach (ItemRank rank in ranks)
            {
                // i++;
                // if (i > 100) break;
                SetSlotsFromList(ref slotIndex, effectSlots, playerInventory.untargetedEffects.GetItemsOfRank(rank));
                SetSlotsFromList(ref slotIndex, effectSlots, playerInventory.targetedEffects.GetItemsOfRank(rank));
            }
            // disable remaining
            while (slotIndex < effectSlots.Count)
            {
                // i++;
                // if (i > 100) break;
                effectSlots[slotIndex].gameObject.SetActive(false);
                slotIndex++;
            }
            // print(i);

            // timer.Stop();
            // print("Inventory took: " + timer.ElapsedMilliseconds + "ms to update");
        }

        // fill slots starting from slotIndex with inventoryItems from toAdd, tracks index of slots used
        // void SetSlotsFromSubInventory(ref int slotIndex, List<SlotUI> slots, RankedSubInventory toAdd, RankedSubInventory extraToAdd = null)
        // {
        //     foreach (ItemRank rank in ranks)
        //     {
        //         var currentList = toAdd.GetItemsOfRank(rank);
        //         var extraList = extraToAdd?.GetItemsOfRank(rank);
        //         foreach (InventoryItem inventoryItem in currentList)
        //         {
        //             var slot = slots[slotIndex];
        //             SetSlot(slot, inventoryItem);

        //             slotIndex++;
        //         }
        //     }
        // }

        void SetSlotsFromList(ref int slotIndex, List<SlotUI> slots, List<InventoryItem> items)
        {
            foreach (InventoryItem inventoryItem in items)
            {
                var slot = slots[slotIndex];

                slot.button.enabled = false;
                slot.gameObject.SetActive(true);
                slot.CurrentItem = inventoryItem.item;
                slot.SetCount(inventoryItem.count, inventoryItem.infinite);

                slotIndex++;
            }
        }

        private void OnEnable()
        {
            showInventoryAction.Enable();
        }
        private void OnDisable()
        {
            showInventoryAction.Disable();
            canvasGroup.DOKill();
        }
    }
}
