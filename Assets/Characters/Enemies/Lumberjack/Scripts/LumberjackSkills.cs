using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ColbyDoan
{
    // todo move alot of this shit into the skill class
    public class LumberjackSkills : SkillsManagerOld
    {
        public override int SkillCount => 3;
        public MeleeSkill meleeSkill;
        public ThrowSkill throwSkill;
        public RecallSkill recallSkill;

        public Animator animator;
        public UnityEngine.U2D.Animation.SpriteLibrary spriteLibrary;

        public UnityEngine.U2D.Animation.SpriteLibraryAsset axeAsset;
        public UnityEngine.U2D.Animation.SpriteLibraryAsset noAxeAsset;

        public LumberjackAxe axe;
        bool hasAxe = true;
        public bool HasAxe => hasAxe;
        public bool AxeInSight => axe && !PhysicsSettings.SolidsLinecast(axe.transform.position, transform.position, Mathf.Max(axe.transform.position.z, transform.position.z));

        public float axeStunDuration = 2;
        public float meleeRange = 1.2f;

        /// <summary> global pos of where the axe originates and where it returns to </summary>
        Vector3 AxePivot => transform.position + Vector3.forward;

        public override ISkill GetSkill(int skillIndex = 0)
        {
            switch (skillIndex)
            {
                case 2:
                    return recallSkill;
                case 1:
                    return throwSkill;
                case 0:
                default:
                    return null;
                    // return meleeSkill;
            }
        }
        public override void SetUp(Character setTo)
        {
            base.SetUp(setTo);
            meleeSkill.SetUp(setTo);
        }
        public override Vector3 TargetedPos { set { base.TargetedPos = value; /*meleeSkill.TargetedPos = value;*/ } }

        const string axeHitID = "on_axe_hit";
        override protected void Start()
        {
            base.Start();
            character.artifacts.metaTriggers.Add(axeHitID, Stun);

            Stats.attackSpeed.OnStatChanged += (value) => { animator.SetFloat("Attack Speed", value); };
        }

        void Stun(TriggerContext context)
        {
            Debug.Log("axe hit");
            context.GetCharacter?.statusEffects.GetStatus<StunSE>("stun").ApplyStatus(axeStunDuration);
        }

        void Update()
        {
            // catch axe
            if (!hasAxe && axe && !throwSkill.Active)
            {
                if (((Vector2)(axe.transform.position - AxePivot)).sqrMagnitude < 1)
                {
                    recallSkill.CatchAxe();
                }
            }
        }

        /// <summary> set axe presence </summary>
        void SetAxe(bool toSet)
        {
            hasAxe = toSet;
            if (toSet)
            {
                spriteLibrary.spriteLibraryAsset = axeAsset;
                meleeSkill.damageMultiplier = 2.5f;
                meleeSkill.metaTriggerID = "axeHitID";
            }
            else
            {
                spriteLibrary.spriteLibraryAsset = noAxeAsset;
                meleeSkill.damageMultiplier = 1f;
                meleeSkill.metaTriggerID = "";
            }
        }
        public void AnimationThrowAxeEvent()
        {
            throwSkill.ThrowAxe();
        }

        void AnticipateCatch()
        {
            StartCoroutine("AccelerateAxe");
        }
        IEnumerator AccelerateAxe()
        {
            while (!hasAxe && recallSkill.Active)
            {
                Vector3 direction = axe.transform.position - AxePivot;
                axe.kinematicObject.AccelerateTo(-direction.normalized * recallSkill.recallMaxSpeed, recallSkill.recallAcceleration * Time.deltaTime);
                yield return null;
            }
        }


        [System.Serializable]
        public class ThrowSkill : ISkill
        {
            public LumberjackSkills manager;
            public float throwSpeed;
            public Cooldown cooldown;
            public float damage = 2;
            public float range = 10;

            public bool Ready => enabled && cooldown.Ready && !Active && manager.HasAxe;
            public bool enabled { get => enabledOld; set => enabledOld = value; }
            public bool Active { get; set; }
            bool enabledOld = true;

            LumberjackAxe Axe => manager.axe;

            public void Activate()
            {
                // Throwing
                // set facing and freeze movement
                manager.character.FacingDirection = manager.TargetedDisplacement;
                manager.Stats.speed.AddMultiplier(0);

                // cooldown
                Active = true;
                cooldown.StartCooldown();
                manager.recallSkill.cooldown.StartCooldown();

                // animation
                manager.animator.SetTrigger("Throw");
            }
            readonly float normalizationConst = Mathf.Sqrt(1 - 1 / 25);
            public void ThrowAxe()
            {
                // goodbye axe
                manager.SetAxe(false);

                // set axe damage (perhaps does not need to be done every throw)
                // Axe.projectile.damagables = manager.character.damageMask;
                // Axe.projectile.SetDamage(new DamageInfo(manager.character, damage));

                // velocity finding
                Vector3 direction = ((Vector3)manager.TargetPos - manager.AxePivot).normalized * normalizationConst + Vector3.forward * .2f;

                // setting and launching
                Axe.transform.position = manager.AxePivot + direction * 1.5f;
                Axe.kinematicObject.AccelerateTo(direction * throwSpeed);
                Axe.Launch();

                // recoil
                manager.character.kinematicObject.ApplyImpulse(-direction * throwSpeed * Axe.kinematicObject.mass);

                // unfreeze movement
                manager.Stats.speed.RemoveMultiplier(0);

                // finish
                Active = false;
            }
            public void Cancel()
            {
                Active = false;
            }
        }


        [System.Serializable]
        public class RecallSkill : ISkill
        {
            public LumberjackSkills manager;
            public Cooldown cooldown;
            public float recallAcceleration;
            public float recallMaxSpeed;
            // public float damage = 2;
            // public float range = 10;

            public bool Ready => enabled && cooldown.Ready && !Active && !manager.HasAxe && manager.AxeInSight;
            public bool enabled { get => enabledOld; set => enabledOld = value; }
            public bool Active { get; set; }
            bool enabledOld = true;

            LumberjackAxe Axe => manager.axe;

            public void Activate()
            {
                // Recalling
                // freeze movement
                manager.Stats.speed.AddMultiplier(0);

                // set active
                Active = true;

                // start cooldown
                cooldown.StartCooldown();

                // recall axe
                Vector2 direction = manager.AxePivot - Axe.transform.position;
                direction.Normalize();
                Axe.kinematicObject.AccelerateTo((Vector3)direction * recallMaxSpeed * .2f + Vector3.forward);
                // print(Axe.kinematicObject.Velocity.magnitude);
                Axe.SetGravity(false);

                // ready catch
                manager.AnticipateCatch();

                // animation
                manager.animator.SetBool("Catching", true);

            }
            public void CatchAxe()
            {
                // update axe
                manager.SetAxe(true);
                Axe.gameObject.SetActive(false);
                Axe.SetGravity(true);

                // unset active
                Active = false;

                // animation
                manager.animator.SetTrigger("Catch");
                manager.animator.SetBool("Catching", false);

                // recoil
                manager.character.kinematicObject.ApplyImpulse(Axe.kinematicObject.velocity * Axe.kinematicObject.mass);

                // unfreeze movement
                manager.Stats.speed.RemoveMultiplier(0);

                // start cooldown
                cooldown.StartCooldown();
            }
            public void Cancel()
            {
                Active = false;

                // animation
                manager.animator.SetBool("Catching", false);

                // unfreeze movement
                manager.Stats.speed.RemoveMultiplier(0);

                if (!manager.hasAxe)
                    Axe.SetGravity(true);
            }
        }
    }
}
