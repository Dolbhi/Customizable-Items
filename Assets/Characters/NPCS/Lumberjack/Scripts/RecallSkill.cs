// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using Physics;

    public class RecallSkill : CooldownSkill
    {
        public override bool Ready => base.Ready && !axeTracker.HasAxe;
        LumberjackAxe Axe => axeTracker.axe;

        public float cooldown;
        public float recallAcceleration;
        public float recallMaxSpeed;
        // public float damage = 2;
        // public float range = 10;

        // public ThrowSkill throwSkill;
        public AxeTracker axeTracker;
        public Animator anime;
        Transform _transform;

        void Awake()
        {
            _transform = transform;
        }

        public override bool TargetInRange(SightingInfo info)
        {
            if (!Axe) return false;
            var axePos = Axe.transform.position;
            return !Physics.PhysicsSettings.SolidsLinecast(axePos, _transform.position, Mathf.Max(axePos.z, _transform.position.z));
        }

        void FixedUpdate()
        {
            // catch axe
            if (Active)
            {
                // accelerate axe
                Vector3 direction = Axe.transform.position - axeTracker.Pivot;
                Axe.kinematicObject.AccelerateTo(-direction.normalized * recallMaxSpeed, recallAcceleration * Time.fixedDeltaTime);

                // try catch axe
                if (((Vector2)(Axe.transform.position - axeTracker.Pivot)).sqrMagnitude < 1)
                {
                    _CatchAxe();
                }
            }
        }
        void _CatchAxe()
        {
            // update axe
            axeTracker.SetAxe(true);
            Axe.gameObject.SetActive(false);
            Axe.SetGravity(true);

            // unset active
            Active = false;

            // animation
            anime.SetTrigger("Catch");
            anime.SetBool("Catching", false);

            // recoil
            character.kinematicObject.ApplyImpulse(Axe.kinematicObject.velocity * Axe.kinematicObject.mass);

            // unfreeze movement
            Stats.speed.RemoveMultiplier(0);

            // start cooldown (again)
            cooldownHandler.StartCooldown(cooldown);
        }

        public override void Activate()
        {
            // Recalling
            // freeze movement
            Stats.speed.AddMultiplier(0);

            // set active
            Active = true;

            // start cooldown
            cooldownHandler.StartCooldown(cooldown);

            // recall axe
            Vector2 direction = axeTracker.Pivot - Axe.transform.position;
            direction.Normalize();
            Axe.kinematicObject.AccelerateTo((Vector3)direction * recallMaxSpeed * .2f + Vector3.forward);
            // print(Axe.kinematicObject.Velocity.magnitude);
            Axe.SetGravity(false);

            // animation
            anime.SetBool("Catching", true);

        }

        public override void Cancel()
        {
            Active = false;

            // animation
            anime.SetBool("Catching", false);

            // unfreeze movement
            Stats.speed.RemoveMultiplier(0);

            if (!axeTracker.HasAxe)
                Axe.SetGravity(true);
        }
    }
}
