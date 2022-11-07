// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.Events;

namespace ColbyDoan
{
    using Physics;

    public class RevolverShootSkill : Skill
    {
        [Header("Shooting Fields")]
        // public int cylinderCount = 6;
        public ProjectileInfo bulletInfo;
        public float moveAccuracyPenalty = .01f;
        // public float reloadTime = 1.5f;
        public float ShootingInaccuracy => character.kinematicObject.Velocity.sqrMagnitude * moveAccuracyPenalty;
        public Gun revolver;
        public ProjectileInfo blueBulletInfo;

        public int zoinked;

        // public override bool Ready => enabled && Stacks > 0;
        // public override bool ShowTimer => false;

        // IDisplayableSkill Implementation
        // public override int Stacks => loadedBullets;
        // public override bool HasStacks => true;

        // [ReadOnly] public int loadedBullets = 6;

        // public UnityEvent<int> OnBulletCountChange;

        public override void SetUp(Character toSet)
        {
            // Reload();

            base.SetUp(toSet);

            // cooldownHandler.onCooldownFinish += Reload;

            Stats.attackSpeed.OnStatChanged += (value) => { revolver.animator.SetFloat("Attack Speed", value); };
        }
        void Update()
        {
            revolver.PointAt((TargetPos.GetDepthApparentPosition() - revolver.transform.position));
        }

        public override void Activate()
        {
            if (!Ready) return;

            revolver.animator.SetBool("Auto Firing", true);
        }
        public override void Cancel()
        {
            base.Cancel();
            revolver.animator.SetBool("Auto Firing", false);
        }

        public void OnShootAnimationStart()
        {
            FireBullet(ShootingInaccuracy);
        }
        public void FireBullet(float inaccuracy)
        {
            // StartCooldown();

            ProjectileInfo info;
            // fire blue if zoinked
            if (zoinked > 0)
            {
                info = blueBulletInfo;
                zoinked--;
            }
            else
            {
                info = bulletInfo;
            }

            // loadedBullets--;
            // OnBulletCountChange.Invoke(loadedBullets);
            // instantiate projectile
            Transform nozzel = revolver.nozzel;
            var angle = Random.Range(-1, 1f);
            angle = Mathf.Sign(angle) * inaccuracy * .5f * angle * angle;
            Vector3 velocity = Quaternion.AngleAxis(angle, Vector3.back) * nozzel.right * info.momentum / info.prop.mass;
            bool shootingDown = TargetPos.z - revolver.transform.position.z < -1;
            Projectile fired = info.prop.FireCopy(nozzel.position.GetUndisplacedPosition(), velocity, character.damageMask, info.GetDamageInfo(character), shootingDown);
            fired.gameObject.SetActive(true);// might not be neccessary
            // sfx
            revolver.audioSource.Play();
            // recoil
            character.kinematicObject.ApplyImpulse(-nozzel.right * info.momentum);

            if (!Ready) revolver.animator.SetBool("Auto Firing", false);
        }

        // public void Reload()
        // {
        //     // while (loadedBullets < cylinderCount) loadedBullets.Enqueue(special);
        //     loadedBullets = cylinderCount;
        //     OnBulletCountChange.Invoke(loadedBullets);
        // }

        // public void StartCooldown()
        // {
        //     cooldownHandler.StartCooldown(reloadTime / Stats.attackSpeed.FinalValue);
        // }
    }

    public class RevolverMeleeTrigger : Trigger
    {
        public override string Name => "revolver_melee_trigger";
        public override bool HasTarget => true;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            user.metaTriggers.Add("on_revolver_melee", TriggerEffects);
        }
    }
    public class BlueRevolverTrigger : Trigger
    {
        public override string Name => "blue_revolver_trigger";
        public override bool HasTarget => true;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            user.metaTriggers.Add("on_blue_revolver_hit", TriggerEffects);
        }
    }
    public class GreenRevolverTrigger : Trigger
    {
        public override string Name => "green_revolver_trigger";
        public override bool HasTarget => true;

        protected override void InternalSetUp(ArtifactManager manager)
        {
            user.metaTriggers.Add("on_green_revolver_hit", TriggerEffects);
        }
    }

    public class RevolverZoinkEffect : Effect
    {
        public override string Name => "revolver_zoink_effect";
        public override bool RequiresTarget => false;

        RevolverShootSkill revolverSkill;

        // get revolver shoot skill from user
        protected override void InternalSetUp(ArtifactManager manager)
        {
            base.InternalSetUp(user);
            var skill = manager.character.skills.skills[0];
            if (skill is RevolverShootSkill)
            {
                revolverSkill = skill as RevolverShootSkill;
            }
            else
            {
                Debug.LogWarning("THIS FUCKER DOESNT HAVE A REVOLVER SHOOT SKILL", manager);
            }
        }

        // increment zoink
        public override void Trigger(TriggerContext context)
        {
            revolverSkill.zoinked++;
        }
    }
    // public class RevolverZoinkedStateSE : RefreshingStatusEffect
    // {
    //     public override string Name => "revolver_zoinked";
    //     public override bool IsDebuff => false;

    //     public RevolverSkills target;

    //     protected override void StartEffect()
    //     {
    //         base.StartEffect();
    //         target.zoinked = true;
    //     }
    //     protected override void StopEffect()
    //     {
    //         base.StopEffect();
    //         target.zoinked = false;
    //     }
    // }

    [System.Serializable]
    public class ProjectileInfo
    {
        public Projectile prop;
        public float momentum = 20;
        public float damageMultiplier = 1;
        public bool invokeOnHit;
        public string metaTriggerID;

        public DamageInfo GetDamageInfo(Character source)
        {
            return new DamageInfo(source, damageMultiplier, _invokeOnHit: invokeOnHit, _metaTriggerID: metaTriggerID);
        }
    }
}