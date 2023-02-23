using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan.CharacterBase
{
    public abstract class StatMultiplyStatusEffect : RefreshingStatusEffect
    {
        CharacterStat[] targetStats;
        protected abstract float[] Multipliers { get; }

        public override void SetUp(StatusEffectsManager target)
        {
            base.SetUp(target);
            targetStats = GetStats(target.character.stats);
            if (targetStats.Length != Multipliers.Length)
            {
                Debug.LogError("number of multipliers does not match number of stats");
            }
        }
        protected abstract CharacterStat[] GetStats(CharacterStats stats);
        protected override void StartEffect()
        {
            base.StartEffect();
            for (int i = 0; i < Multipliers.Length; i++)
            {
                targetStats[i].AddMultiplier(Multipliers[i]);
            }
        }
        protected override void StopEffect()
        {
            base.StopEffect();
            for (int i = 0; i < Multipliers.Length; i++)
            {
                targetStats[i].RemoveMultiplier(Multipliers[i]);
            }
        }
    }
    public abstract class StatChangeStatusEffect : RefreshingStatusEffect
    {
        CharacterStat[] targetStats;
        protected abstract float[] Change { get; }

        public override void SetUp(StatusEffectsManager target)
        {
            base.SetUp(target);
            targetStats = GetStats(target.character.stats);
            if (targetStats.Length != Change.Length)
            {
                Debug.LogError("number of multipliers does not match number of stats");
            }
        }
        protected abstract CharacterStat[] GetStats(CharacterStats stats);
        protected override void StartEffect()
        {
            base.StartEffect();
            for (int i = 0; i < Change.Length; i++)
            {
                targetStats[i].BaseValue += Change[i];
            }
        }
        protected override void StopEffect()
        {
            base.StopEffect();
            for (int i = 0; i < Change.Length; i++)
            {
                targetStats[i].BaseValue -= Change[i];
            }
        }
    }
    public class SlowSE : StatMultiplyStatusEffect
    {
        public override string Name => "slow";
        public override bool IsDebuff => true;

        protected override float[] Multipliers => new float[] { .5f };
        protected override CharacterStat[] GetStats(CharacterStats stats)
        {
            return new CharacterStat[] { stats.speed };
        }
    }
    public class SpeedSE : StatMultiplyStatusEffect
    {
        public override string Name => "speed";
        public override bool IsDebuff => false;

        protected override float[] Multipliers => new float[] { 1.5f };
        protected override CharacterStat[] GetStats(CharacterStats stats)
        {
            return new CharacterStat[] { stats.speed };
        }
    }
    public class InvincibilitySE : RefreshingStatusEffect
    {
        public override string Name => "invincibility";
        public override bool IsDebuff => false;

        protected override void StartEffect()
        {
            base.StartEffect();
            manager.character.healthManager.invincible = true;
        }
        protected override void StopEffect()
        {
            base.StopEffect();
            manager.character.healthManager.invincible = false;
        }
    }
    /// <summary> Prevents movement and skills </summary>
    public class StunSE : RefreshingStatusEffect
    {
        public override string Name => "stun";
        public override bool IsDebuff => true;

        CharacterStat speedStat;
        SkillsManager skillsManager;

        public override void SetUp(StatusEffectsManager target)
        {
            base.SetUp(target);
            speedStat = target.character.stats.speed;
            skillsManager = target.character.skills;
        }
        protected override void StartEffect()
        {
            base.StartEffect();
            speedStat.AddMultiplier(0);
            if (skillsManager)
                skillsManager.enabled = false;
            else
                Debug.Log("Target does not have a assigned skillManager");
        }
        protected override void StopEffect()
        {
            base.StopEffect();
            speedStat.RemoveMultiplier(0);
            if (skillsManager)
                skillsManager.enabled = true;
        }

    }
    public class RegenSE : StackingStatusEffect
    {
        public override string Name => "regeneration";
        public override bool IsDebuff => false;

        Health targetHealth;

        public override void SetUp(StatusEffectsManager target)
        {
            base.SetUp(target);
            targetHealth = target.character.healthManager;
        }
        protected override void StartEffect(Timer timer)
        {
            base.StartEffect(timer);
            targetHealth.regen += 1;
        }
        protected override void StopEffect(Timer timer)
        {
            base.StopEffect(timer);
            targetHealth.regen -= 1;
        }
    }
    public class BurnSE : StackingStatusEffect<float>
    {
        public override string Name => "burn";
        public override bool IsDebuff => true;

        bool active = false;

        protected override void StartEffect(Timer timer)
        {
            base.StartEffect(timer);
            if (!active)
            {
                manager.StartCoroutine(BurnLoop());
            }
        }

        IEnumerator BurnLoop()
        {
            // wait one frame for dictionary to update
            yield return null;
            while (stackTimers.Count > 0)
            {
                float damage = 0;
                foreach (Timer timer in stackTimers)
                {
                    damage += stackData[timer];
                }
                manager.character.healthManager.Damage(damage, showPopup: false);

                yield return new WaitForSeconds(.2f);
            }
            active = false;
        }
    }
    public class WeakSE : StatMultiplyStatusEffect
    {
        public override string Name => "weak";
        public override bool IsDebuff => true;

        protected override float[] Multipliers => new float[] { .5f };
        protected override CharacterStat[] GetStats(CharacterStats stats)
        {
            return new CharacterStat[] { stats.attack };
        }
    }
    public class VulnerableSE : StatChangeStatusEffect
    {
        public override string Name => "vulnerable";
        public override bool IsDebuff => true;

        protected override float[] Change => new float[] { -70 };
        protected override CharacterStat[] GetStats(CharacterStats stats)
        {
            return new CharacterStat[] { stats.armor };
        }
    }
    public class ArmoredSE : StatChangeStatusEffect
    {
        public override string Name => "armored";
        public override bool IsDebuff => false;

        protected override float[] Change => new float[] { 100 };
        protected override CharacterStat[] GetStats(CharacterStats stats)
        {
            return new CharacterStat[] { stats.armor };
        }
    }
    public class AngerSE : StackingStatusEffect
    {
        public override string Name => "anger";
        public override bool IsDebuff => false;

        public override int StackCount => _effectiveStackCount;

        CharacterStat attackSpdStat;

        const float _increment = .2f;
        float _currentIncrease = 0;

        int _effectiveStackCount = 0;

        public override void SetUp(StatusEffectsManager target)
        {
            base.SetUp(target);
            attackSpdStat = target.character.stats.attackSpeed;
        }

        public void ApplyStatusWithLimit(float duration, int limit)
        {
            if (_effectiveStackCount < limit) _effectiveStackCount++;
            Debug.Log(_effectiveStackCount);
            ApplyStatus(duration);
        }

        protected override void StartEffect(Timer timer)
        {
            base.StartEffect(timer);
            UpdateAttackSpd();
        }

        protected override void StopEffect(Timer timer)
        {
            base.StopEffect(timer);
            if (base.StackCount < _effectiveStackCount) _effectiveStackCount = base.StackCount;
            UpdateAttackSpd();
        }

        void UpdateAttackSpd()
        {
            attackSpdStat.BaseValue -= _currentIncrease;
            _currentIncrease = _increment * _effectiveStackCount;
            attackSpdStat.BaseValue += _currentIncrease;
        }
    }
}
