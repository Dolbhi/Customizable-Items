using System;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace ColbyDoan
{
    public class MouseoverTextBox : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        [SerializeField] LayoutElement layoutElement;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] InputAction mousePos;

        public static Action<string> OnHoverStart;
        public static Action OnHoverEnd;

        void Awake()
        {
            mousePos.Enable();
            canvasGroup.alpha = 0;
        }
        void OnEnable()
        {
            OnHoverStart += ShowBox;
            OnHoverEnd += HideBox;
        }
        void OnDisable()
        {
            OnHoverStart -= ShowBox;
            OnHoverEnd -= HideBox;
        }


        void ShowBox(string tooltip)
        {
            canvasGroup.alpha = 1;
            text.text = tooltip;
            layoutElement.enabled = text.preferredWidth > 200;
        }
        void HideBox()
        {
            canvasGroup.alpha = 0;
        }
    }
}
