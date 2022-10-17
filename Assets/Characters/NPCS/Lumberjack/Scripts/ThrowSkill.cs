// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class ThrowSkill : CooldownSkill
    {
        public override bool Ready => base.Ready && !Active && axeTracker.HasAxe;
        public bool Active { get; set; }

        public float throwSpeed;
        public float cooldown;
        public float damage = 2;
        public float range = 10;

        public AxeTracker axeTracker;
        public RecallSkill recallSkill;// probably remove
        public Animator anime;

        public override void Activate()
        {
            // Throwing
            // set facing and freeze movement
            character.FacingDirection = TargetPos - character.transform.position;
            Stats.speed.AddMultiplier(0);

            // cooldown
            Active = true;
            cooldownHandler.StartCooldown(cooldown);
            // recallSkill.cooldown.StartCooldown(); start recall cooldown to prevent instant recalls

            // animation
            anime.SetTrigger("Throw");
        }
        readonly float normalizationConst = Mathf.Sqrt(1 - 1 / 25);
        /// <summary>
        /// Animation event method to actually throw the axe
        /// </summary>
        public void ThrowAxe()
        {
            // goodbye axe
            axeTracker.SetAxe(false);

            // set axe damage (perhaps does not need to be done every throw)
            // Axe.projectile.damagables = manager.character.damageMask;
            // Axe.projectile.SetDamage(new DamageInfo(manager.character, damage));

            // velocity finding
            Vector3 direction = (TargetPos - axeTracker.Pivot).normalized * normalizationConst + Vector3.forward * .2f;

            // setting and launching
            axeTracker.axe.transform.position = axeTracker.Pivot + direction * 1.5f;
            axeTracker.axe.kinematicObject.AccelerateTo(direction * throwSpeed);
            axeTracker.axe.Launch();

            // recoil
            character.kinematicObject.ApplyImpulse(-direction * throwSpeed * axeTracker.axe.kinematicObject.mass);

            // unfreeze movement
            Stats.speed.RemoveMultiplier(0);

            // finish
            Active = false;
        }
        public override void Cancel()
        {
            Active = false;
        }
    }
}
