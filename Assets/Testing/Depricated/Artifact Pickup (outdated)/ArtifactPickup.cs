using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Experimental.U2D.Animation;

namespace ColbyDoan
{
    public class ArtifactPickup : MonoBehaviour
    {
        public Item trigger;
        public Item effect;

        //[SerializeField] SpriteResolver spriteResolver;

        //[SerializeField] Interactable interactable;

        public void SetArtifacts(Item trigger, Item effect)
        {
            this.trigger = trigger;
            this.effect = effect;

            OnValidate();
        }

        private void OnValidate()
        {
            if (trigger && trigger.type != ItemType.Trigger)
            {
                trigger = null;
                Debug.LogWarning("invalid trigger type");
            }
            if (effect && effect.type != ItemType.Effect)
            {
                effect = null;
                Debug.LogWarning("invalid effect type");
            }

            //spriteResolver?.SetCategoryAndLabel(spriteResolver.GetCategory(), effect?.rank.ToString() ?? "D");
            // get interactable
            //if (!interactable)
            //{
            //    interactable = GetComponent<Interactable>();
            //}

        }

        //public void SetSprite(AsyncOperationHandle<Sprite> handle)
        //{
        //    if (handle.Result)
        //        sprite.sprite = handle.Result;
        //    else
        //        Debug.LogWarning("Unable to get sprite");
        //}

        public void OnPickup(PlayerBehaviour playerBehaviour)
        {
            if (!trigger || !effect)
            {
                Debug.LogError("Trigger or artifact missing!");
            }
            playerBehaviour.GetComponentInChildren<ArtifactManager>().Add(trigger, effect);
            Destroy(transform.root.gameObject);
        }

        // on player approach behaviour
        // void OnTriggerEnter2D(Collider2D collider)
        // {
        //     if (collider.tag == "Player")
        //     {
        //         panel.SetActive(true);
        //     }
        // }

        // void OnTriggerExit2D(Collider2D collider)
        // {
        //     if (collider.tag == "Player")
        //     {
        //         panel.SetActive(false);
        //     }
        // }
    }
}
