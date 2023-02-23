using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace ColbyDoan
{
    public class PickupHUD : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;

        [SerializeField] GameObject TERect;
        [SerializeField] DescriptionHUD triggerDescription;
        [SerializeField] DescriptionHUD effectDescription;

        [SerializeField] DescriptionHUD triggerItemDescription;
        [SerializeField] DescriptionHUD effectItemDescription;

        [SerializeField] ArtifactManager artifacts;
        [SerializeField] Inventory inventory;

        Queue<(Item, Item, EffectModifier)> displayQueue = new Queue<(Item, Item, EffectModifier)>();
        bool displaying = false;

        private void Awake()
        {
            artifacts.OnArtifactAdded += QueueArtifactToDisplay;
            inventory.OnItemPickup += QueueItemToDisplay;
        }

        public void QueueItemToDisplay(Item item)
        {
            QueueArtifactToDisplay(item, null, EffectModifier.None);
        }
        public void QueueArtifactToDisplay(NewArtifactInfo info)
        {
            QueueArtifactToDisplay(info.triggerItem, info.effectItem, info.modifier);
        }
        public void QueueArtifactToDisplay(Item trigger, Item effect, EffectModifier modifier)
        {
            displayQueue.Enqueue((trigger, effect, modifier));

            if (!displaying) DisplayNextArtifact();
        }

        public void DisplayNextArtifact()
        {
            // check if anything left to display
            if (displayQueue.Count == 0)
            {
                displaying = false;
                return;
            }

            displaying = true;

            var toDisplay = displayQueue.Dequeue();

            // set up fade out
            var fade = canvasGroup.DOFade(0, .2f);
            fade.onComplete += DisplayNextArtifact;

            if (toDisplay.Item2 == null)
            {
                // do single item display
                Item item = toDisplay.Item1;
                if (item.type == ItemType.Trigger)
                {
                    triggerItemDescription.gameObject.SetActive(true);
                    effectItemDescription.gameObject.SetActive(false);

                    triggerItemDescription.SetDisplay(item);
                }
                else
                {
                    triggerItemDescription.gameObject.SetActive(false);
                    effectItemDescription.gameObject.SetActive(true);

                    effectItemDescription.SetDisplay(item);
                }
                TERect.SetActive(false);

                fade.SetDelay(2);
            }
            else
            {
                // do full trigger effect display
                triggerItemDescription.gameObject.SetActive(false);
                effectItemDescription.gameObject.SetActive(false);
                TERect.SetActive(true);

                triggerDescription.SetDisplay(toDisplay.Item1);
                effectDescription.SetDisplay(toDisplay.Item2, toDisplay.Item3);

                fade.SetDelay(4);
            }

            // do fade
            canvasGroup.DOFade(1, .1f).Play();
            fade.Play();
        }
    }
}
