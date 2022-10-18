using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ColbyDoan
{
    public class FacingMeleeSkill : CooldownSkill
    {
        public UnityEvent<Action> MeleeAnimation;
        // public UnityEvent CancelMelee;
        public UnityEvent<float> SetAttackSpeed;

        [SerializeField] AudioSource audioSource = null;
        [SerializeField] Transform attackCenter = null;

        public float cooldown;

        public float knockback;
        public float meleeRadius = .5f;
        public float damageMultiplier = 1;
        public bool invokeOnHit = true;
        public string metaTriggerID;

        public override void SetUp(Character setTo)
        {
            base.SetUp(setTo);
            Stats.attackSpeed.OnStatChanged += SetAttackSpeed.Invoke;
        }

        // Vector2 direction;

        public override void Activate()
        {
            //print("lets go?");
            if (!Ready) return;

            //print("module attacking");
            // get direction
            // direction = TargetPos - transform.position;
            // direction.Normalize();
            // set active
            Active = true;
            // start animation
            MeleeAnimation.Invoke(Attack);
        }
        // public override void Cancel()
        // {
        //     CancelMelee.Invoke();
        // }

        List<Collider2D> hits = new List<Collider2D>();
        public void Attack()
        {
            // play sound
            audioSource.Play();

            // Deal damage
            DamageInfo info = new DamageInfo(character, damageMultiplier, _knockback: new ForceInfo(attackCenter.right * knockback), _invokeOnHit: invokeOnHit, _metaTriggerID: metaTriggerID);
            Physics2D.OverlapCircle(attackCenter.position, meleeRadius, PhysicsSettings.GetFilter(character.damageMask, attackCenter.position, 1, .3f), hits);
            foreach (Collider2D hit in hits)
            {
                //print("attacking");
                info.ApplyTo(hit.transform);
            }

            // update active
            Active = false;
            // cooldown
            cooldownHandler.StartCooldown(cooldown);
        }
    }
}
