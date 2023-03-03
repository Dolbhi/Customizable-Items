using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;

namespace ColbyDoan
{
    using CharacterBase;
    using Physics;

    // framework of an effect that can subscribe to triggers and be updated
    public abstract class Effect : IFactoryProduct
    {
        public abstract string Name { get; }
        public abstract bool RequiresTarget { get; }
        public virtual bool Hidden => false;

        protected ArtifactManager user;
        public Item source;

        public abstract void Trigger(TriggerContext context);

        /// <summary> set up effect </summary>
        public void SetUp(ArtifactManager manager, Item effectInfo = null)
        {
            user = manager;
            source = effectInfo;
            // Debug.Log("Effect source: " + effectInfo?.name);
            InternalSetUp(manager);
        }
        protected virtual void InternalSetUp(ArtifactManager manager) { }
        public virtual void OnDestroy() { }

        public virtual void Upgrade(int increase = 1) { level += increase; }
        public virtual void SetLevel(int toSet) { level += toSet; }

        public int level = 1;

        //public Item GetInfo()
        //{
        //    Item item = new Item();
        //    item.idName = Name;
        //    item.usesTarget = RequiresTarget;
        //    item.type = ItemType.Effect;
        //    return item;
        //}
    }
    public enum EffectModifier { None = 0, Broken = -1, Bundle = 1 }

    // untargeted
    public class TestEffect : Effect
    {
        public override string Name => "log_effect";
        public override bool RequiresTarget => false;

        public override void Trigger(TriggerContext context)
        {
            Debug.Log("FUCK IT WORKS");
        }
    }


    public abstract class FlatStatUpEffect : Effect
    {
        public override bool RequiresTarget => false;
        protected abstract float StatIncrease { get; }
        protected abstract CharacterStat GetStat(CharacterStats stats);
        public override void Trigger(TriggerContext context)
        {
            GetStat(user.character.stats).BaseValue += StatIncrease * level;
        }
    }
    public class SpeedUpEffect : FlatStatUpEffect
    {
        public override string Name => "speed_up_effect";
        protected override float StatIncrease => 1;
        protected override CharacterStat GetStat(CharacterStats stats)
        {
            return stats.speed;
        }
    }
    public class AttackSpeedUpEffect : FlatStatUpEffect
    {
        public override string Name => "attack_speed_up_effect";
        protected override float StatIncrease => 1;
        protected override CharacterStat GetStat(CharacterStats stats)
        {
            return stats.attackSpeed;
        }
    }
    public class ProjectileBurstEffect : Effect
    {
        public override string Name => "projectile_burst_effect";
        public override bool RequiresTarget => false;

        const string projectilePath = "bullet";
        const float shotSpeed = 30;
        AsyncOperationHandle<GameObject> opHandle;

        Projectile projectileProp;
        //float burstReadyTime;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(manager);
            LoadAsset();
        }

        async void LoadAsset()
        {
            opHandle = Addressables.LoadAssetAsync<GameObject>(projectilePath);
            await opHandle.Task;

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
                projectileProp = opHandle.Result.GetComponent<Projectile>();
            else
                Debug.LogError("Prop cannot be loaded");
        }

        public override void Trigger(TriggerContext context)
        {
            if (!projectileProp) return;

            int bulletCount = 6 + level * 4;
            float angleSeperation = 2 * Mathf.PI / bulletCount;
            float angle = 0;
            for (int i = 0; i < bulletCount; i++)
            {
                angle += angleSeperation;
                Vector3 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var damage = new DamageInfo(user, _damageMultiplier: .8f, _invokeOnHit: true);
                projectileProp.FireCopy(context.position + direction, direction * shotSpeed, user.character.damageMask, damage);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            // Addressables.Release
        }
    }
    public class AimedBulletEffect : Effect
    {
        public override string Name => "aimed_bullet_effect";
        public override bool RequiresTarget => false;

        const string projectilePath = "bullet";
        const float shotSpeed = 15;
        const float summonOffset = 4;
        const float rangeSqr = 100;
        AsyncOperationHandle<GameObject> opHandle;

        Projectile projectileProp;
        //float burstReadyTime;
        HashSet<Transform> trackedGroup;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(manager);
            LoadAsset();

            // setup enemy tracker
            trackedGroup = RootTracker.GetSet(TrackerTag.Enemy);
            // if (manager.character.trackingGroup.TrackingKey == "ally")
            // else
            //     trackedGroup = RootTracker.GetSet("ally");
        }

        async void LoadAsset()
        {
            opHandle = Addressables.LoadAssetAsync<GameObject>(projectilePath);
            await opHandle.Task;

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
                projectileProp = opHandle.Result.GetComponent<Projectile>();
            else
                Debug.LogError("Prop cannot be loaded");
        }

        struct SummonData
        {
            public Vector3 origin;
            public Vector3 bestDisplacement;
            public float bestDistSqr;

            public SummonData(Vector3 origin, float maxDistSqr)
            {
                this.origin = origin;
                bestDisplacement = Vector3.zero;
                bestDistSqr = maxDistSqr;
            }
        }
        // List<SummonData> summonData = new List<SummonData>();
        public override void Trigger(TriggerContext context)
        {
            if (!projectileProp) return;

            // do bullet for each level
            for (int i = 0; i < level; i++)
            {
                // add random offset to context pos
                Vector3 randomOffset;
                int count = 0;
                do
                {
                    randomOffset = summonOffset * (Vector3)Random.insideUnitCircle + .1f * Vector3.forward;
                    count++;
                    if (count > 100)
                    {
                        Debug.Log("RANDOM OFFSET FAILURE", user);
                        return;
                    }
                } while (PhysicsSettings.CheckForSolids(context.position + randomOffset, .4f));

                Vector3 summonOrigin = context.position + randomOffset;

                // find nearest enemy in range and shoot them
                Vector3 bestDisplacement = Vector3.zero;
                float bestDistSqr = rangeSqr;
                foreach (Transform target in trackedGroup)
                {
                    var displacement = target.position - summonOrigin;

                    // ignore if target on higher level or out of range
                    if (displacement.z > .9f) continue;
                    if (displacement.sqrMagnitude > bestDistSqr) continue;

                    // Debug.Log("Disp: " + displacement);

                    // check los
                    if (PhysicsSettings.SolidsLinecast(summonOrigin, target.position)) continue;

                    // Debug.Log("LOS passed");

                    bestDisplacement = displacement;
                    bestDistSqr = displacement.sqrMagnitude;
                }

                if (bestDisplacement == Vector3.zero) continue;

                Vector3 direction = bestDisplacement.normalized;
                var damage = new DamageInfo(user, _damageMultiplier: 1, _invokeOnHit: false);
                projectileProp.FireCopy(summonOrigin, direction * shotSpeed, user.character.damageMask, damage);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            // Addressables.Release
        }
    }
    public class RegenEffect : Effect
    {
        public override string Name => "regen_effect";
        public override bool RequiresTarget => false;

        public override void Trigger(TriggerContext context)
        {
            user.character.statusEffects.GetStatus<StackingRegenSE>("regeneration").ApplyStatus(5 * level);
        }
    }
    public class MicroHealEffect : Effect
    {
        public override string Name => "tiny_heal_effect";
        public override bool RequiresTarget => false;

        Health health;
        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(manager);
            health = manager.character.healthManager;
        }

        public override void Trigger(TriggerContext context)
        {
            health.Heal(health.MaxHealth * .02f * level);
        }
    }
    public class SmallExplosionEffect : Effect
    {
        public override string Name => "small_explosion_effect";
        public override bool RequiresTarget => false;

        AsyncOperationHandle<GameObject> opHandle;
        const string explosionKey = "explosion";
        Explosion explosionProp;

        const float baseDamage = .8f;
        const float radius = 2;
        const float baseKnockback = 2;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(manager);
            LoadAssets();
        }
        async void LoadAssets()
        {
            opHandle = Addressables.LoadAssetAsync<GameObject>(explosionKey);
            await opHandle.Task;

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
                explosionProp = opHandle.Result.GetComponent<Explosion>();
            else
                Debug.LogError("Prop cannot be loaded");
        }

        public override void Trigger(TriggerContext context)
        {
            if (!explosionProp) return;

            Explosion explosion = GameObject.Instantiate<Explosion>(explosionProp, context.position, Quaternion.identity);
            explosion.damage = new DamageInfo(user, baseDamage + .2f * level, _knockback: new ForceInfo(baseKnockback * Vector3.right));
            explosion.radius = radius + level;
            explosion.gameObject.SetActive(true);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Addressables.Release(opHandle);
        }
    }
    public class SpeedBuffEffect : Effect
    {
        public override string Name => "speed_buff_effect";
        public override bool RequiresTarget => false;

        public override void Trigger(TriggerContext context)
        {
            user.character.statusEffects.GetStatus<SpeedSE>("speed").ApplyStatus(3 * level);
        }
    }
    public class ArmorBuffEffect : Effect
    {
        public override string Name => "armor_buff_effect";
        public override bool RequiresTarget => false;

        public override void Trigger(TriggerContext context)
        {
            user.character.statusEffects.GetStatus<ArmoredSE>("armored").ApplyStatus(3 * level);
        }
    }
    public class DropBombEffect : Effect
    {
        public override string Name => "drop_bomb_effect";
        public override bool RequiresTarget => false;
        AsyncOperationHandle<GameObject> opHandle;
        const string grenadeKey = "artifact_grenade";
        MonoReferencer grenadeProp;

        // stats
        const float baseDamage = 3;
        const float radius = 5;
        const float baseKnockback = 4;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(manager);
            LoadGrenade();
        }

        async void LoadGrenade()
        {
            opHandle = Addressables.LoadAssetAsync<GameObject>(grenadeKey);
            await opHandle.Task;

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
                grenadeProp = opHandle.Result.GetComponent<MonoReferencer>();
            else
                Debug.LogError("Prop cannot be loaded");
        }


        public override void Trigger(TriggerContext context)
        {
            if (!grenadeProp) return;
            // GameObject.Instantiate(grenadeProp, context.position, Quaternion.identity);
            MonoReferencer grenade = GameObject.Instantiate<MonoReferencer>(grenadeProp, context.position, Quaternion.identity);

            KinematicObject ko = grenade.components.DictionaryData["KinematicObject"] as KinematicObject;
            ko.Velocity += 3 * (Vector3)Random.insideUnitCircle + 5 * Vector3.forward;

            Explosion explosion = grenade.components.DictionaryData["Explosion"] as Explosion;
            explosion.damage = new DamageInfo(user, baseDamage + .2f * level, _knockback: new ForceInfo(baseKnockback * Vector3.right));
            explosion.radius = radius;
            // explosion.gameObject.SetActive(true);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Addressables.Release(opHandle);
        }
    }
    public class AngerEffect : Effect
    {
        public override string Name => "anger_effect";
        public override bool RequiresTarget => false;

        StatusEffectsManager statusEffectsManager;
        const float _baseDuration = 2;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(manager);
            statusEffectsManager = manager.character.statusEffects;
        }

        public override void Trigger(TriggerContext context)
        {
            var angerSE = statusEffectsManager.GetStatus<AngerSE>("anger");
            if (angerSE.StackCount < level)
                angerSE.ApplyStatusWithLimit(_baseDuration + level, level);
        }
    }
    public class UpgradeEffect : Effect
    {
        public override string Name => "upgrade_effect";
        public override bool RequiresTarget => false;

        // protected override void SetUp(ArtifactManager manager)
        // {
        //     base.SetUp(manager);
        //     statusEffectsManager = manager.character.statusEffects;
        // }

        public override void Trigger(TriggerContext context)
        {
            for (int i = level; i > 0; i--)
            {
                UpgradeRandom();
            }
        }
        void UpgradeRandom()
        {
            var keyIndex = Random.Range(0, user.triggers.Count);
            Trigger trigger = user.triggers.ElementAt(keyIndex).Value;
            Effect effect;
            do
            {
                var effectIndex = Random.Range(0, trigger.effects.Count);
                effect = trigger.effects.ElementAt(effectIndex).Value;
            } while (effect.Hidden);

            EffectModifier modifier = EffectModifier.None;
            if (effect is BrokenEffect) modifier = EffectModifier.Broken;
            else if (effect is BundleEffect) modifier = EffectModifier.Bundle;

            // user.OnArtifactAdded.Invoke(new NewArtifactInfo(trigger.source, effect.source, modifier, trigger));
            user.Add(trigger.source, effect.source, modifier);
        }
    }
    public class DownRankEffect : Effect
    {
        public override string Name => "down_rank_effect";
        public override bool RequiresTarget => false;

        Inventory inventory;

        protected override void InternalSetUp(ArtifactManager user)
        {
            base.InternalSetUp(user);
            inventory = user.character.GetComponentInChildren<Inventory>();
            if (!inventory) Debug.LogWarning("USER HAS NO INVENTORY");
        }

        public override void Trigger(TriggerContext context)
        {
            int times = level;
            while (times > 0)
            {
                DownRank();
                times--;
            }
        }
        void DownRank()
        {
            InventoryItem itemSlot;

            int untargetedEffects = inventory.untargetedEffects.Count - inventory.untargetedEffects.D.Count;
            int targetedEffects = inventory.targetedEffects.Count - inventory.targetedEffects.D.Count;

            if (untargetedEffects + targetedEffects <= 0) return;

            int index = Random.Range(0, untargetedEffects + targetedEffects);
            if (index < untargetedEffects)
            {
                var currentRank = ItemRank.C;
                var currentList = inventory.untargetedEffects.GetItemsOfRank(currentRank);
                while (index >= currentList.Count)
                {
                    index -= currentList.Count;
                    currentRank++;
                    currentList = inventory.untargetedEffects.GetItemsOfRank(currentRank);
                }
                itemSlot = currentList[index];
            }
            else
            {
                index -= untargetedEffects;

                var currentRank = ItemRank.C;
                var currentList = inventory.targetedEffects.GetItemsOfRank(currentRank);
                while (index >= currentList.Count)
                {
                    index -= currentList.Count;
                    currentRank++;
                    currentList = inventory.targetedEffects.GetItemsOfRank(currentRank);
                }
                itemSlot = currentList[index];
            }

            inventory.TryRemoveItem(itemSlot.item);
            Item newItem = itemSlot.item.Copy();
            newItem.rank--;
            inventory.AddItem(newItem);
        }
    }
    public class LimitedHPUpEffect : Effect
    {
        public override string Name => "limited_hp_up_effect";
        public override bool RequiresTarget => false;

        int _currentBonus = 0;
        const int maxBonusPerLevel = 100;

        public override void Trigger(TriggerContext context)
        {
            if (_currentBonus >= maxBonusPerLevel * level) return;

            user.character.stats.maxHealth.BaseValue -= _currentBonus;
            _currentBonus = Mathf.Min(maxBonusPerLevel * level, _currentBonus + level);
            user.character.stats.maxHealth.BaseValue += _currentBonus;
        }
    }
    public class ReduceCooldownEffect : Effect
    {
        public override string Name => "reduce_cooldown_effect";
        public override bool RequiresTarget => false;

        public override void Trigger(TriggerContext context)
        {
            foreach (Skill skill in user.character.skills.skills)
            {
                skill.ReduceCooldown(level);
            }
        }
    }
    public class TempSpeedUpEffect : Effect
    {
        public override string Name => "temp_speed_up_effect";
        public override bool RequiresTarget => false;

        float currentSpeedBonus = 0;
        const float speedIncrease = .1f;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(manager);
            GameManager.Instance.OnLevelLoaded += ResetBonus;// might not work as intended
        }

        public override void Trigger(TriggerContext context)
        {
            user.character.stats.speed.BaseValue -= currentSpeedBonus;
            currentSpeedBonus += speedIncrease * level;
            user.character.stats.speed.BaseValue += currentSpeedBonus;
        }

        void ResetBonus()
        {
            user.character.stats.speed.BaseValue -= currentSpeedBonus;
            currentSpeedBonus = 0;
        }
    }
    public class DropItemEffect : Effect
    {
        public override string Name => "drop_item_effect";
        public override bool RequiresTarget => false;

        AsyncOperationHandle<GameObject> opHandle;
        const string pickupKey = "item pickup";
        ItemPickup pickupProp;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(manager);
            LoadAssets();
        }
        async void LoadAssets()
        {
            opHandle = Addressables.LoadAssetAsync<GameObject>(pickupKey);
            await opHandle.Task;

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
                pickupProp = opHandle.Result.GetComponent<ItemPickup>();
            else
                Debug.LogError("Prop cannot be loaded");
        }

        public override void Trigger(TriggerContext context)
        {
            if (!pickupProp) return;

            for (int i = level; level > 0; level--)
            {
                ItemPickup pickup = GameObject.Instantiate<ItemPickup>(pickupProp, context.position, Quaternion.identity);
                pickup.CurrentItem = GameManager.Instance.itemPool.GetRandomItem().Copy();
                // find pickup's KO and yeet it
                KinematicObject.FindFromRoot(pickup.transform.root).velocity = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.forward) * Vector3.right * 2 + Vector3.forward * 3;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Addressables.Release(opHandle);
        }
    }
    // public class TargetedProjectileEffect : Effect
    // {
    //     public override string Name => "targeted_projectile_effect";
    //     public override bool RequiresTarget => false;

    //     const string projectilePath = "bullet";
    //     const float shotSpeed = 30;
    //     AsyncOperationHandle<GameObject> opHandle;

    //     Projectile projectileProp;
    //     //float burstReadyTime;

    //     protected override void SetUp(ArtifactManager manager)
    //     {
    //         base.SetUp(manager);
    //         LoadAsset();
    //     }
    //     async void LoadAsset()
    //     {
    //         opHandle = Addressables.LoadAssetAsync<GameObject>(projectilePath);
    //         await opHandle.Task;

    //         if (opHandle.Status == AsyncOperationStatus.Succeeded)
    //             projectileProp = opHandle.Result.GetComponent<Projectile>();
    //         else
    //             Debug.LogError("Prop cannot be loaded");
    //     }

    //     public override void Trigger(TriggerContext context)
    //     {


    //         Vector3 direction;
    //         var damage = new DamageInfo(user.character, _damageMultiplier: .8f, _invokeOnHit: true);
    //         projectileProp.FireCopy(context.position + direction, direction * shotSpeed, user.character.damageMask, damage);
    //     }
    // }
    // targeted
    public class TestTargetEffect : Effect
    {
        public override string Name => "test_target_effect";
        public override bool RequiresTarget => true;

        public override void Trigger(TriggerContext context)
        {
            Debug.Log("i cast fuck on " + context.targetRoot.name);
        }
    }
    public class BurnEffect : Effect
    {
        public override string Name => "burn_effect";
        public override bool RequiresTarget => true;

        CharacterStat damage;

        public override void Trigger(TriggerContext context)
        {
            Character.FindFromRoot(context.targetRoot)?.statusEffects.GetStatus<BurnSE>("burn").ApplyStatus(1 + 3 * level, damage.FinalValue * .1f);
        }

        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(manager);
            damage = manager.character.stats.attack;
        }
    }
    public class SlowEffect : Effect
    {
        public override string Name => "slow_effect";
        public override bool RequiresTarget => true;

        public override void Trigger(TriggerContext context)
        {
            Character.FindFromRoot(context.targetRoot)?.statusEffects.GetStatus<SlowSE>("slow").ApplyStatus(1 + 3 * level);
        }
    }
    public class StunEffect : Effect
    {
        public override string Name => "stun_effect";
        public override bool RequiresTarget => true;

        public override void Trigger(TriggerContext context)
        {
            Character.FindFromRoot(context.targetRoot)?.statusEffects.GetStatus<StunSE>("stun").ApplyStatus(1 + level);
        }
    }
    public class WeakEffect : Effect
    {
        public override string Name => "weak_effect";
        public override bool RequiresTarget => true;

        public override void Trigger(TriggerContext context)
        {
            Character.FindFromRoot(context.targetRoot)?.statusEffects.GetStatus<WeakSE>("weak").ApplyStatus(5 * level);
        }
    }
    public class VulnerableEffect : Effect
    {
        public override string Name => "vulnerable_effect";
        public override bool RequiresTarget => true;

        public override void Trigger(TriggerContext context)
        {
            Character.FindFromRoot(context.targetRoot)?.statusEffects.GetStatus<VulnerableSE>("Vulnerable").ApplyStatus(5 * level);
        }
    }

    public class KillEffect : Effect
    {
        public override string Name => "kill_effect";
        public override bool RequiresTarget => true;

        public override void Trigger(TriggerContext context)
        {
            Debug.Log("i cast kill on " + context.targetRoot.name);
            Health.instanceFromTransform[context.targetRoot.transform].Damage(1000);
        }
    }

    // meta
    public class BrokenEffect : Effect
    {
        public override string Name => "broken_" + baseEffect?.Name;
        public override bool RequiresTarget => baseEffect.RequiresTarget;
        public Effect baseEffect;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(manager);
            baseEffect.SetUp(manager, null);
        }

        public override void Trigger(TriggerContext context)
        {
            float random = Random.value;
            float prob = (1f + level) / (10 + level);
            // Debug.Log($"Random: {random}, Probability: {prob}");
            if (random < prob)
                baseEffect.Trigger(context);
        }
    }
    public class BundleEffect : Effect
    {
        public override string Name => "bundle_" + baseEffect?.Name;
        public override bool RequiresTarget => baseEffect.RequiresTarget;
        public Effect baseEffect;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(manager);
            baseEffect.Upgrade(4);
            baseEffect.SetUp(manager, null);
        }

        public override void Upgrade(int increase = 1)
        {
            base.Upgrade(increase);
            baseEffect.Upgrade(5);
        }
        public override void SetLevel(int toSet)
        {
            base.SetLevel(toSet);
            baseEffect.SetLevel(level * 5);
        }

        public override void Trigger(TriggerContext context)
        {
            baseEffect.Trigger(context);
        }
    }
}