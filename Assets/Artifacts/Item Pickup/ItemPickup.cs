using UnityEngine;
namespace ColbyDoan
{
    public class ItemPickup : MonoBehaviour
    {
        public Item CurrentItem
        {
            get { return item; }
            set
            {
                item = value;
                if (!item)
                {
                    itemRenderer.SetEmpty();
                    interactable.hoverText = "Bruh";
                }
                else
                {
                    itemRenderer.SetItem(item);
                    interactable.hoverText = "Pick up " + item.name;
                }
            }
        }
        [SerializeField] Item item;
        [SerializeField] ItemRenderer itemRenderer;
        [SerializeField] Interactable interactable;

#if UNITY_EDITOR
        Item _last;
        private void OnValidate()
        {
            if (item != _last)
            {
                if (!item)
                {
                    Debug.LogWarning("No item set");
                    CurrentItem = null;
                    _last = item;
                    return;
                }
                CurrentItem = item.Copy();
                _last = item;
            }
        }
#endif

        public void OnPickup(PlayerBehaviour playerBehaviour)
        {
            playerBehaviour.inventory.AddItem(item);
            Destroy(transform.root.gameObject);
        }
    }
}
