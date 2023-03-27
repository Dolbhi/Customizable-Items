// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace ColbyDoan
{
    public class Sign : MonoBehaviour
    {
        [SerializeField] TMP_Text textUI;
        [SerializeField] CanvasGroup canvas;
        [SerializeField] CopyOwnRect rectCopier;
        [SerializeField] Interactable interactable;

        [TextArea(3, 10)]
        [ContextMenuItem("Set Text", "SetText")]
        [SerializeField] string signText;

        bool _enabled = false;

        void Awake()
        {
            canvas.alpha = 0;
        }

        void OnValidate()
        {
            textUI.text = signText;
        }

        // [ContextMenu("Set Text")]
        void SetText()
        {
            textUI.text = signText;
            rectCopier.CopyRect();
        }

        public void ToggleSign()
        {
            if (!_enabled)
            {
                EnableSign();
            }
            else
            {
                DisableSign();
            }
        }

        public void EnableSign()
        {
            _enabled = true;
            canvas.DOKill();
            canvas.DOFade(1, .2f);
            interactable.hoverText = "Stop Reading";
        }

        public void DisableSign()
        {
            _enabled = false;
            canvas.DOKill();
            canvas.DOFade(0, .2f);
            interactable.hoverText = "Read Sign";
        }
    }
}
