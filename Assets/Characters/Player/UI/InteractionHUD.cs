// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace ColbyDoan
{
    public class InteractionHUD : MonoBehaviour
    {
        public TMP_Text interactionText;
        public static Action<string> SetInteractionText;

        void Awake()
        {
            SetInteractionText += SetText;
        }

        void SetText(string text)
        {
            if (!interactionText) return;
            interactionText.text = text;
        }
    }
}
