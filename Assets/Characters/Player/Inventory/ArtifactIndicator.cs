using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace ColbyDoan
{
    /// <summary>
    /// Controller for UI gameobject indicating an owned artifact
    /// Multiple indicators are managed by an ArtifactsHUD gameobject
    /// 
    /// Contains a trigger icon and multiple effect icons
    /// Effect icons also have a counter text component
    /// </summary>
    public class ArtifactIndicator : MonoBehaviour
    {
        public UIItemRenderer triggerIcon;
        public Dictionary<string, MonoReferencer> effectIcons = new Dictionary<string, MonoReferencer>();
        public CanvasGroup canvasGroup;

        public float[] pulseTimes = new float[5];

        public MonoReferencer slotProp;
        const string iconKey = "icon";
        const string textKey = "text";

        Trigger trigger;
        int rank;

        Tween indicatorPulse;

        [ContextMenu("Test display")]
        public void PulseAlpha()
        {
            indicatorPulse.Kill();
            canvasGroup.alpha = 1;
            indicatorPulse = canvasGroup.DOFade(.3f, pulseTimes[rank]);
            indicatorPulse.SetEase(Ease.InCubic);
            indicatorPulse.Play();
            // image.sprite = icon;
            // StopCoroutine("DisplayCoroutine");
            // StartCoroutine("DisplayCoroutine");
        }

        public void SetTrigger(NewArtifactInfo info)
        {
            triggerIcon.SetItem(info.triggerItem);
            this.trigger = info.trigger;
            rank = (int)info.triggerItem.rank;

            var HUDEffect = new DisplayIndicatorEffect() { indicator = this };
            this.trigger.effects.Add(HUDEffect.Name, HUDEffect);
            HUDEffect.SetUp(this.trigger.user, null);
        }

        public void AddEffect(NewArtifactInfo info)
        {
            MonoReferencer effectIcon;
            string effectIDPrefix = info.modifier.GetPrefix();
            if (!effectIcons.TryGetValue(effectIDPrefix + info.effectItem.idName, out effectIcon))
            {
                effectIcon = Instantiate<MonoReferencer>(slotProp, transform);
                ((UIItemRenderer)effectIcon.components.DictionaryData[iconKey]).SetItem(info.effectItem);

                effectIcons.Add(effectIDPrefix + info.effectItem.idName, effectIcon);
            }

            // Update counts
            foreach (Effect effect in trigger.effects.Values)
            {
                if (effect.Hidden) continue;
                MonoReferencer icon;
                if (!effectIcons.TryGetValue(effect.Name, out icon))
                {
                    Debug.LogWarning("Effect icon missing", this);
                    return;
                }

                // change count displayed based on mod
                var levelText = effect.level.ToString();
                if (effect is BrokenEffect)
                    levelText = (effect.level * .1f).ToString();
                else if (effect is BundleEffect)
                    levelText = (effect.level * 5).ToString();

                // Debug.Log(effect.level);
                ((TMP_Text)icon.components.DictionaryData[textKey]).text = levelText;
            }
        }

        void OnDisable()
        {
            indicatorPulse.Kill();
        }
    }

    class DisplayIndicatorEffect : Effect
    {
        public override string Name => "trigger_indicator_effect";
        public override bool RequiresTarget => false;
        public override bool Hidden => true;

        public ArtifactIndicator indicator;

        public override void Trigger(TriggerContext context)
        {
            indicator.PulseAlpha();
            // print(rank);
        }
    }
}
