// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class GunMeleeSkill : CooldownSkill
    {
        public Transform meleeCenter;
        public float damageMultiplier;
        public float knockback;
        public float range;
        public float cooldown;
        public float stunDuration = 2;

        public Gun gun;

        public Skill[] skillsToDisable;

        public override void Activate()
        {
            if (Ready)
            {
                SetActive(false);
                gun.animator.SetTrigger("Melee Attack");
                cooldownHandler.StartCooldown(cooldown);
            }
        }
        /// <summary> Animation action to deal damage </summary>
        public void MeleeDamage()
        {
            Collider2D[] results = Physics2D.OverlapCircleAll(meleeCenter.transform.position, range, character.damageMask, transform.position.z - 1.5f, transform.position.z + 1);
            foreach (Collider2D collider in results)
            {
                if (collider.transform.root == transform.root) continue;
                Vector3 direction = collider.transform.root.position - transform.position;
                ForceInfo knockbackForce = new ForceInfo(10 * direction.normalized + character.kinematicObject.velocity + Vector3.forward * 10, knockback);
                Character.FindFromRoot(collider.transform.root).statusEffects.GetStatus<StunSE>("stun").ApplyStatus(stunDuration);
                new DamageInfo(character, damageMultiplier, _knockback: knockbackForce, _invokeOnHit: true).ApplyTo(collider.transform);
            }
        }
        public void SetActive(bool toSet)
        {
            Active = toSet;
            foreach (Skill skill in skillsToDisable)
                skill.enabled = toSet;
        }
    }
}
