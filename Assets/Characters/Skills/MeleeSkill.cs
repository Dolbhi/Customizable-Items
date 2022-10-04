using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ColbyDoan
{
    public class MeleeSkill : CooldownSkill
    {
        public override bool Ready => base.Ready && !Active;
        public bool Active { get; private set; }

        public UnityEvent<Vector2, Action> MeleeAnimation;
        public UnityEvent CancelMelee;
        public UnityEvent<float> SetAttackSpeed;

        [SerializeField] AudioSource audioSource = null;
        [SerializeField] AttackBox attackBox = null;
        // [SerializeField] Transform attackCenter = null;

        public float knockbackSpeed;
        public float knockbackForce;
        public float cooldown;
        public float meleeRadius = .5f;
        public float damageMultiplier = 1;
        public bool invokeOnHit = true;
        public string metaTriggerID;

        public override void SetUp(Character setTo)
        {
            base.SetUp(setTo);
            Stats.attackSpeed.OnStatChanged += SetAttackSpeed.Invoke;
            attackBox.attackMask = character.damageMask;
        }

        Vector2 direction;

        public override void Activate()
        {
            //print("lets go?");
            if (!Ready) return;

            //print("module attacking");
            // get direction
            Vector3 displacement = TargetPos - transform.position;
            direction = ((Vector2)displacement.GetDepthApparentPosition()).normalized;
            // Debug.Log($"disp{displacement} dir:{direction}");
            // set active
            Active = true;
            // start animation
            MeleeAnimation.Invoke(direction, Attack);
        }
        public override void Cancel()
        {
            CancelMelee.Invoke();
        }

        List<Collider2D> hits = new List<Collider2D>();
        public void Attack()
        {
            // play sound
            audioSource.Play();

            // Deal damage
            DamageInfo info = new DamageInfo(character, damageMultiplier, _knockback: new ForceInfo(direction * knockbackSpeed, knockbackForce), _invokeOnHit: invokeOnHit, _metaTriggerID: metaTriggerID);
            attackBox.Attack(info);
            // Physics2D.OverlapCircle(attackCenter.position, meleeRadius, PhysicsSettings.GetFilter(character.damageMask, attackCenter.position, 1, .4f), hits);
            // foreach (Collider2D hit in hits)
            // {
            //     //print("attacking");
            //     info.ApplyTo(hit.transform);
            // }

            // update active
            Active = false;
            // cooldown
            cooldownHandler.StartCooldown(cooldown);
        }
    }
}