// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    [RequireComponent(typeof(RectTransform))]
    public class CopyOwnRect : MonoBehaviour
    {
        public RectTransform pasteTarget;
        RectTransform _copyTarget;

        [Header("Padding")]
        public float top;
        public float bottom;
        public float left;
        public float right;

        void OnValidate()
        {
            _copyTarget = GetComponent<RectTransform>();
            // _copyTarget.anchorMin = _copyTarget.anchorMax = Vector2.one / 2;
            // _copyTarget.position = Vector2.zero;
        }

        [ContextMenu("Copy rect to target")]
        public void CopyRect()
        {
            // center self
            _copyTarget.anchorMin = _copyTarget.anchorMax = Vector2.one / 2;
            _copyTarget.localPosition = Vector2.zero;

            // center target
            Vector2 middle = (pasteTarget.anchorMin + pasteTarget.anchorMax) / 2;
            pasteTarget.anchorMin = pasteTarget.anchorMax = middle;
            pasteTarget.position = Vector2.zero;

            // calc. target rect.
            Rect targetRect = _copyTarget.rect;
            targetRect.max += new Vector2(right, top);
            targetRect.min -= new Vector2(left, bottom);

            pasteTarget.offsetMin = targetRect.min;
            pasteTarget.offsetMax = targetRect.max;
        }
    }
}
