using UnityEngine;

using ColbyDoan.Attributes;

namespace ColbyDoan
{
    using Physics;

    public class RiflemanSkills : MonoBehaviour
    {
        public OldSkillCrap ShootingSkill;
        public OldSkillCrap MeleeSkill;
        public OldSkillCrap DodgeRollSkill;
        public OldSkillCrap SpecialSkill;

        public Gun rifle;

        Character character;

        public float Inaccuracy { get; private set; }

        [Header("Projectile Fields")]
        public Projectile bullet;
        public float projectileMomentum = 1;
        public float projectileDamage;
        public bool automatic;

        [Header("Accuracy Fields")]
        public float minInaccuracy = 5;
        public float accuracyMovePenaltyCoefficient = 5;
        public float accuracyRecoilPenalty = 3;
        public float accuracyRecoveryRate = 1000;

        [Header("Melee Fields")]
        public Transform meleeCenter;
        public float meleeDamage;
        public float meleeRange;
        public float meleeKnockback;
        public float meleeCooldown;
        public float meleeStunDuration = 2;

        [Header("Dodge Roll Fields")]
        public float verticalDodgeSpeed = 2;
        public float dodgeSpeedMultiplier = 5;
        public float dodgeCooldown = 3;

        [Header("Special Fields")]
        public Projectile specialRound;
        public float specialDamage;
        public float specialMomentum;
        public float specialCooldown;

        protected void Start()
        {
            // Stats.attackSpeed.OnStatChanged += UpdateRifleAttackSpeed;
        }
        void Update()
        {
            // point rifle
            // rifle.PointAt(TargetPos - rifle.transform.position);// + rifle.transform.position.z * Vector3.up);
            // print("target" + targetedPos);
            // print("rifle: " + (rifle.transform.position - rifle.transform.position.z * Vector3.down));
        }

        void UpdateRifleAttackSpeed(float value)
        {
            rifle.animator.SetFloat("Attack Speed", value);
        }

        #region Shooting
        // public void SetProjectile(Projectile set)
        // {
        //     Destroy(projectile.gameObject);
        //     projectile = Instantiate(set, transform);
        //     projectile.gameObject.SetActive(false);
        // }
        void StartShooting()
        {
            if (!ShootingSkill.Ready) return;

            rifle.animator.Play("Rifle Fire");
            if (automatic)
                rifle.animator.SetBool("Firing", true);
        }
        void StopShooting()
        {
            rifle.animator.SetBool("Firing", false);
        }
        public void Shoot()
        {
            if (specialsLoaded > 0)
            {
                FireProjectile(specialRound, specialMomentum, new DamageInfo(character, specialDamage, _invokeOnHit: true, _metaTriggerID: "on_rifle_special_hit"));
                specialsLoaded--;
                SpecialSkill.enabled = true;
            }
            else
            {
                FireProjectile(bullet, projectileMomentum, new DamageInfo(character, projectileDamage, _invokeOnHit: true));
            }
        }
        Projectile FireProjectile(Projectile projectile, float momentum, DamageInfo damage)
        {
            return null;
        }
        #endregion

        #region Melee
        void StartMeleeAttack()
        {
            if (MeleeSkill.Ready)
            {
                ShootingSkill.enabled = false;
                MeleeSkill.enabled = false;
                SpecialSkill.enabled = false;
                rifle.animator.SetTrigger("Melee Attack");
                MeleeSkill.cooldown.StartCooldown();
            }
        }
        /// <summary> Animation action to deal damage </summary>
        public void MeleeDamage()
        {
            Collider2D[] results = Physics2D.OverlapCircleAll(meleeCenter.transform.position, meleeRange, character.damageMask, transform.position.z - 1.5f, transform.position.z + 1);
            foreach (Collider2D collider in results)
            {
                if (collider.transform.root == transform.root) continue;
                Vector3 direction = collider.transform.root.position - transform.position;
                ForceInfo knockback = new ForceInfo(10 * direction.normalized + character.kinematicObject.velocity + Vector3.forward * 10, meleeKnockback);
                Character.FindFromRoot(collider.transform.root).statusEffects.GetStatus<StunSE>("stun").ApplyStatus(meleeStunDuration);
                new DamageInfo(character, meleeDamage, _knockback: knockback, _invokeOnHit: true).ApplyTo(collider.transform);
            }
        }
        /// <summary> Animation action to reset </summary>
        public void ResetMelee()
        {
            ShootingSkill.enabled = true;
            MeleeSkill.enabled = true;
            SpecialSkill.enabled = true;
        }
        #endregion

        #region Special
        [ReadOnly] int specialsLoaded;
        void LoadSpecial()
        {
            if (!SpecialSkill.Ready) return;
            specialsLoaded++;
            // cool down
            SpecialSkill.cooldown.StartCooldown();
            SpecialSkill.enabled = false;
        }

        // public void FireSpecial()
        // {
        //     if (SpecialSkill.Ready)
        //     {
        //         // projectile
        //         Projectile fired = specialRound.FireCopy(rifle.nozzel.position - Vector3.up * rifle.nozzel.position.z, rifle.transform.right * specialMomentum / specialRound.mass, character.damageMask);
        //         fired.damage = new DamageInfo(character, specialDamage, _invokeOnHit: true, _customCallbackID: "on_rifle_special_hit");
        //         // fired.OnHit.AddListener(delegate { SummonExplosion(fired); });

        //         // sfx
        //         rifle.audioSource.Play();
        //         // recoil
        //         character.kinematicObject.ApplyImpulse(-rifle.transform.right * projectileMomentum);
        //         // cool down
        //         SpecialSkill.cooldown.StartCooldown();
        //     }
        // }

        // public void SummonExplosion(Projectile projectile)
        // {
        //     Explosion explosion = Instantiate(grenadeExplosion, projectile.transform.position, Quaternion.identity);
        //     //print("projectile pos " + projectile.transform.position);
        //     ForceInfo knockback = new ForceInfo(Vector2.right * grenadeKnockback);
        //     explosion.damage = new DamageInfo(character, _damageMultiplier: grenadeExplosiveDamage, _knockback: knockback, _customCallbackID: "on_rifle_grenade_hit");
        //     //print(grenadeExplosiveDamage * stats.damage.FinalValue);
        //     explosion.gameObject.SetActive(true);
        // }
        #endregion
    }

    public class RifleSpecialTrigger : Trigger
    {
        public override string Name => "rifle_special_trigger";
        public override bool HasTarget => true;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            user.metaTriggers.Add("on_rifle_special_hit", TriggerEffects);
        }
    }
}