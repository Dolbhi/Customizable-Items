// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ColbyDoan
{
    // manages present effects and triggers and their creation/upgrading/removal
    public class ArtifactManager : MonoBehaviour, IAutoDependancy<Character>
    {
        public Character Dependancy { set => character = value; }
        [HideInInspector] public Character character;

        public Action<NewArtifactInfo> OnArtifactAdded = delegate { };
        // public Action OnArtifactChanged = delegate { };

        [SerializeField] ArtifactToAdd[] startingArtifacts = new ArtifactToAdd[0];

        // [SerializeField] bool displayTriggersOnHUD = false;

        public Dictionary<string, Trigger> triggers = new Dictionary<string, Trigger>();
        /// <summary> special events that trigger triggers </summary>
        public Dictionary<string, Action<TriggerContext>> metaTriggers = new Dictionary<string, Action<TriggerContext>>();

        public void InvokeMeta(TriggerContext context, string id)
        {
            // Debug.Log("Trying to invoke " + id, this);
            Action<TriggerContext> evt;
            if (metaTriggers.TryGetValue(id, out evt))
            {
                evt.Invoke(context);
                // Debug.Log("Invoked " + id);
            }
        }

        void Start()
        {
            foreach (ArtifactToAdd pair in startingArtifacts)
            {
                if (pair.Validate())
                {
                    Add(pair.trigger.Copy(), pair.effect.Copy(), pair.modifier);
                }
                else
                {
                    Debug.Log("THIS STARTING ITEM IS SHITTTT", this);
                }
            }
        }

        // built in metaTrigger
        public event Action<float, bool, Health> OnHit;
        public void InvokeOnHit(float damage, bool isCrit, Health targetHealth)
        {
            OnHit?.Invoke(damage, isCrit, targetHealth);
        }

        // add artifact
        public void Add(Item triggerItem, Item effectItem, EffectModifier modifier = EffectModifier.None)
        {
            NewArtifactInfo info = new NewArtifactInfo(triggerItem, effectItem, modifier, null);

            // check if trigger is already present
            Trigger trigger;
            if (triggers.TryGetValue(triggerItem.idName, out trigger))
            {
                // add effect only
                trigger.AddEffect(effectItem, modifier);
            }
            else
            {
                // creates trigger
                trigger = ArtifactFactory.triggerFactory.GetItem(triggerItem.idName);
                //print(trigger);

                // adds to list
                triggers.Add(trigger.Name, trigger);

                // set up
                trigger.SetUp(this, triggerItem);
                trigger.AddEffect(effectItem, modifier);
            }
            info.trigger = trigger;

            OnArtifactAdded.Invoke(info);
            Debug.Log($"Added artifact with trigger: {triggerItem.name}, effect: {effectItem.name}, modifier: {modifier.ToString()}");
        }
        //public void Remove()        

        [ContextMenu("Trigger all effects")]
        void TriggerAllEffects()
        {
            TriggerContext context = new TriggerContext(transform.root);
            foreach (Trigger trigger in triggers.Values)
            {
                foreach (Effect effect in trigger.effects.Values)
                {
                    effect.Trigger(context);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (Trigger trigger in triggers.Values)
            {
                trigger.OnDestroy();
            }
        }
    }

    [System.Serializable]
    struct ArtifactToAdd
    {
        public Item trigger;
        public Item effect;
        public EffectModifier modifier;

        // Check if trigger and effect are set properly
        public bool Validate()
        {
            if (!trigger || !effect) return false;
            if (trigger.type != ItemType.Trigger || effect.type != ItemType.Effect) return false;
            return true;
        }
    }

    public class NewArtifactInfo
    {
        public Item triggerItem;
        public Item effectItem;
        public EffectModifier modifier;
        public Trigger trigger;

        public NewArtifactInfo(Item _triggerItem, Item _effectItem, EffectModifier _modifier, Trigger _trigger)
        {
            triggerItem = _triggerItem;
            effectItem = _effectItem;
            modifier = _modifier;
            trigger = _trigger;
        }
    }
}