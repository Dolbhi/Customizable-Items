using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

using ColbyDoan.Attributes;

namespace ColbyDoan
{
    public class PlatformGlow : MonoBehaviour
    {
        public Color inactiveColor;
        public Color brightHigh;
        public Color brightLow;

        public bool GlowActive
        {
            get => glowActive;
            set
            {
                if (glowActive == value) return;
                glowActive = value;
                if (value)
                    StartGlow();
                else
                    StopGlow();
            }
        }
        [ReadOnly][SerializeField] bool glowActive = false;

        [SerializeField] SpriteRenderer spriteRenderer;

        [ContextMenu("Toggle Glow")]
        void ToggleGlow()
        {
            GlowActive = !GlowActive;
        }

        void Awake()
        {
            StopGlow();
        }

        void StopGlow()
        {
            glowLoop.Complete();
            spriteRenderer.color = inactiveColor;
        }

        Sequence glowLoop;
        void StartGlow()
        {
            glowLoop = DOTween.Sequence();
            glowLoop.Append(spriteRenderer.DOColor(brightHigh, .5f));
            glowLoop.Append(spriteRenderer.DOColor(brightLow, .5f));
            glowLoop.AppendCallback(StartGlow);
            glowLoop.Play();
        }
    }
}
