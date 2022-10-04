using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ColbyDoan
{
    public class SlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] MaterialLibrary outlines;
        [SerializeField] TMP_Text countText;
        [SerializeField] Image image;
        public Button button;

        Item item;
        public Item CurrentItem { get => item; set { item = value; UpdateItem(); } }

        void UpdateItem()
        {
            // image
            image.sprite = item.image;
            image.material = outlines[(int)item.rank];
        }

        public void SetCount(int count, bool isInfinite = false)
        {
            countText.text = isInfinite ? "!" : count.ToString();
        }

        public void OnPointerEnter(PointerEventData ctx)
        {
            MouseoverTextBox.OnHoverStart?.Invoke(item.description);
        }

        public void OnPointerExit(PointerEventData ctx)
        {
            MouseoverTextBox.OnHoverEnd?.Invoke();
        }
    }
}
