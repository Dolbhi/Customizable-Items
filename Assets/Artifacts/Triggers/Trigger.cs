using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ColbyDoan
{
    /// <summary> framework to set up an event to trigger effects </summary>
    public abstract class Trigger : IFactoryProduct
    {
        public abstract string Name { get; }
        public abstract bool HasTarget { get; }

        public ArtifactManager user;
        public Item source;

        // public event OnEffectsChange

        // attached effects
        public Dictionary<string, Effect> effects = new Dictionary<string, Effect>(1);

        /// <summary> base function, trigger all effects using context </summary>
        protected void TriggerEffects(TriggerContext context)
        {
            // Debug.Log("Trigger " + Name + " has been triggered");

            // Trigger
            var effectsList = effects.Values;
            foreach (Effect effect in effectsList)
            {
                effect.Trigger(context);
            }
        }
        /// <summary> Trigger effect on self </summary>
        protected void TriggerEffects()
        {
            if (HasTarget)
            {
                Debug.LogError("NO TARGET BRO, YOU LIED");
            }
            TriggerContext context = new TriggerContext(user.transform.position);
            TriggerEffects(context);
        }
        /// <summary> Trigger effect on target </summary>
        protected void TriggerEffects(Transform target = null)
        {
            if (!target)
            {
                Debug.LogError("NO TARGET BRO, YOU LIED");
            }
            TriggerContext context = new TriggerContext(target);
            TriggerEffects(context);
        }
        /// <summary> Trigger effect on position </summary>
        protected void TriggerEffects(Vector3 targetPos)
        {
            if (HasTarget)
            {
                Debug.LogError("NO TARGET BRO, YOU LIED");
            }
            TriggerContext context = new TriggerContext(targetPos);
            TriggerEffects(context);
        }

        /// <summary> adds effect to list and subscribes it's activation if new, upgrades it if not </summary>
        public void AddEffect(Item effectInfo, EffectModifier modifier)
        {
            // check if info passed is a valid effect
            if (effectInfo.type != ItemType.Effect)
            {
                Debug.LogWarning("item " + effectInfo.idName + " is not an effect");
                return;
            }
            if (effectInfo.usesTarget && !HasTarget)
            {
                Debug.LogWarning("effect " + effectInfo.idName + " is invalid for trigger " + Name);
                return;
            }

            // add "broken_" of "bundle_" prefix to id
            string id = modifier.GetPrefix() + effectInfo.idName;

            // upgrades effect if already present
            if (effects.ContainsKey(id))
            {
                // upgrades by 5 if is bundle
                // int upgradeAmount = (modifier == EffectModifier.Bundle) ? 5 : 1;
                effects[id].Upgrade();
                // info.effect = effects[id];
                return;
            }
            else
            {
                // get base effect
                Effect effect = ArtifactFactory.effectFactory.GetItem(effectInfo.idName);
                // makes broken effect if needed
                if (modifier == EffectModifier.Broken)
                {
                    effect = new BrokenEffect() { baseEffect = effect };
                }
                else if (modifier == EffectModifier.Bundle)
                {
                    effect = new BundleEffect() { baseEffect = effect };
                }
                // adds
                effects.Add(effect.Name, effect);
                // set up
                effect.SetUp(user, effectInfo);

                // info.effect = effect;
                // info.effectIsNew = true;
                return;
            }
        }

        public void SetUp(ArtifactManager manager, Item triggerInfo) { user = manager; source = triggerInfo; InternalSetUp(manager); }
        public virtual void OnDestroy() { foreach (Effect effect in effects.Values) { effect.OnDestroy(); } }
        protected abstract void InternalSetUp(ArtifactManager manager);
    }

    /// <summary> context struct for triggering effects </summary>
    public struct TriggerContext
    {
        public Vector3 position;
        public Transform targetRoot;
        public Character GetCharacter => Character.FindFromRoot(targetRoot);

        public TriggerContext(Vector3 pos)
        {
            this.position = pos;
            this.targetRoot = null;
        }
        public TriggerContext(Transform target)
        {
            this.position = target.position;
            this.targetRoot = target;
        }
    }

    // untargeted
    public class HurtTrigger : Trigger
    {
        public override string Name => "hurt_trigger";
        public override bool HasTarget => false;

        const float hurtAmount = 5;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            manager.character.healthManager.OnHurt += HealthManager_OnHurt;
        }

        void HealthManager_OnHurt(HurtInfo damage)
        {
            int count = (int)(damage.damage / hurtAmount);
            for (int i = 0; i < count; i++)
                TriggerEffects();
        }
    }
    public class DeathTrigger : Trigger
    {
        public override string Name => "death_trigger";
        public override bool HasTarget => false;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            manager.character.healthManager.OnDeath += TriggerEffects;
        }
    }
    public class KillTrigger : Trigger
    {
        public override string Name => "kill_trigger";
        public override bool HasTarget => false;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            user.OnHit += HandleHit;
        }

        void HandleHit(float damage, bool isCrit, Health health)
        {
            if (health.CurrentHealth <= 0)
            {
                TriggerEffects(health.transform.position);
            }
        }
    }
    public class MultiKillTrigger : Trigger
    {
        public override string Name => "multi_kill_trigger";
        public override bool HasTarget => false;

        const float timeLimit = 3;
        Queue<float> pastKillExpireTimes = new Queue<float>(3);

        protected override void InternalSetUp(ArtifactManager manager)
        {
            user.OnHit += HandleHit;
        }

        void HandleHit(float damage, bool isCrit, Health health)
        {
            // check for kill
            if (health.CurrentHealth <= 0)
            {
                // check for expires
                while (pastKillExpireTimes.Count != 0 && pastKillExpireTimes.Peek() < Time.time)
                {
                    pastKillExpireTimes.Dequeue();
                }
                // check if there is still 2 or more unexpired kills
                if (pastKillExpireTimes.Count >= 2)
                {
                    pastKillExpireTimes.Clear();
                    TriggerEffects();
                }
                else
                {
                    // add new kill
                    pastKillExpireTimes.Enqueue(Time.time + timeLimit);
                }
            }
        }
    }
    public abstract class TimerTrigger : Trigger
    {
        public override bool HasTarget => false;
        protected virtual int Period { get; }

        WaitForSeconds wait;
        Coroutine timer;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            timer = manager.StartCoroutine(TimerCoroutine());
        }

        IEnumerator TimerCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Period);
                TriggerEffects();
            }
        }
    }
    public class TinyTimer : TimerTrigger
    {
        public override string Name => "two_second_trigger";
        protected override int Period => 2;
    }
    public class ShortTimer : TimerTrigger
    {
        public override string Name => "short_timer_trigger";
        protected override int Period => 10;
    }
    public class MediumTimer : TimerTrigger
    {
        public override string Name => "medium_timer_trigger";
        protected override int Period => 60;
    }
    public class LongTimer : TimerTrigger
    {
        public override string Name => "long_timer_trigger";
        protected override int Period => 300;
    }
    public class TileBreakTrigger : Trigger
    {
        public override string Name => "tile_break_trigger";
        public override bool HasTarget => false;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            TileManager.OnTileChange += HandleTileChange;
        }

        void HandleTileChange(Vector3Int cellPos, TileBase tile)
        {
            if (tile == null)
            {
                TriggerEffects(TileManager.Instance.mainTilemap.CellToWorld(cellPos));
            }
        }
    }
    public class KillCountTrigger : Trigger
    {
        public override string Name => "kill_count_trigger";
        public override bool HasTarget => false;

        const int killsNeeded = 100;
        int _currentKills;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            user.OnHit += HandleHit;
        }

        void HandleHit(float damage, bool isCrit, Health health)
        {
            if (health.CurrentHealth <= 0)
            {
                _currentKills++;
                if (_currentKills >= killsNeeded)
                {
                    _currentKills -= killsNeeded;
                    TriggerEffects();
                }
            }
        }
    }
    public class HeavyHurtTrigger : Trigger
    {
        public override string Name => "heavy_hurt_trigger";
        public override bool HasTarget => false;

        const float hurtTreshold = .5f;
        Health health;
        float pastHp = 1;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            health = manager.character.healthManager;
            manager.character.healthManager.OnHurt += HealthManager_OnHurt;
        }

        void HealthManager_OnHurt(HurtInfo damage)
        {
            user.StartCoroutine(UpdateLastHp(health.FractionFull));
            if (pastHp - health.FractionFull > hurtTreshold) TriggerEffects();
        }

        readonly WaitForSeconds oneSec = new WaitForSeconds(1);
        IEnumerator UpdateLastHp(float newHp)
        {
            yield return oneSec;
            pastHp = newHp;
        }
    }
    public class NoHurtTrigger : Trigger
    {
        public override string Name => "no_hurt_trigger";
        public override bool HasTarget => false;

        const float hurtCooldown = 5;
        const float period = 1;

        float cooldownOverTime;

        WaitForSeconds waitRoutine = new WaitForSeconds(period);

        Coroutine triggerRoutine;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            manager.character.healthManager.OnHurt += ResetCooldown;

            cooldownOverTime = Time.time + hurtCooldown;
            triggerRoutine = user.StartCoroutine(UpdateCoroutine());
        }

        void ResetCooldown(HurtInfo damage)
        {
            cooldownOverTime = Time.time + hurtCooldown;
        }

        IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                float timeLeft = cooldownOverTime - Time.time;
                if (timeLeft < 0)
                {
                    // do triggering loop
                    TriggerEffects();
                    yield return waitRoutine;
                }
                else
                {
                    // do waiting loop
                    yield return new WaitForSeconds(timeLeft);
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            user.StopCoroutine(triggerRoutine);
            user.character.healthManager.OnHurt -= ResetCooldown;
        }
    }
    // targeted
    public class HitTrigger : Trigger
    {
        public override string Name => "hit_trigger";
        public override bool HasTarget => true;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            user.OnHit += User_OnHit;
        }

        private void User_OnHit(float damage, bool isCrit, Health health)
        {
            if (damage > .5f * user.character.stats.attack.BaseValue && health.CurrentHealth > 0)
            {
                TriggerEffects(health.transform.root);
            }
        }
    }
    public class CritTrigger : Trigger
    {
        public override string Name => "crit_trigger";
        public override bool HasTarget => true;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            user.OnHit += User_OnHit;
        }

        private void User_OnHit(float arg1, bool crit, Health health)
        {
            if (crit && health.CurrentHealth > 0)
                TriggerEffects(health.transform.root);
        }
    }
    public class HeavyHitTrigger : Trigger
    {
        public override string Name => "heavy_hit_trigger";
        public override bool HasTarget => true;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            user.OnHit += HandleHit;
        }

        void HandleHit(float damage, bool isCrit, Health health)
        {
            if (damage >= 3 * user.character.stats.attack.BaseValue && health.CurrentHealth > 0)
            {
                TriggerEffects(health.transform.root);
            }
        }
    }
    public class HitLowHPTrigger : Trigger
    {
        public override string Name => "hit_low_hp_trigger";
        public override bool HasTarget => true;

        const float lowHPTreshold = .33f;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            user.OnHit += HandleHit;
        }

        void HandleHit(float damage, bool isCrit, Health health)
        {
            // Debug.Log((health.CurrentHealth + damage) / health.MaxHealth);
            if ((health.CurrentHealth + damage) / health.MaxHealth <= lowHPTreshold && health.CurrentHealth > 0)
            {
                TriggerEffects(health.transform.root);
            }
        }
    }
    // public class SelfAuraTrigger : Trigger
    // {
    //     public override string Name => "self_aura_trigger";
    //     public override bool HasTarget => true;
    // }

}