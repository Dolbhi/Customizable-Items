﻿using UnityEngine;
using UnityEngine.Events;

namespace ColbyDoan
{
    using CharacterBase;

    public class ProjectileSkill : Skill
    {
        public override bool Ready => enabled && fireCooldown.Ready;

        public UnityEvent<UnityAction> attackAnimation;
        public bool skipAnimation = false;

        public ArtifactManager artifacts;
        public Transform fireOrigin;
        public Projectile shot;
        public float shotSpeed = 16;
        public float cooldown;
        public float fireRange;
        public float damageMultiplier = 1;

        Cooldown fireCooldown;

        void Awake()
        {
            fireCooldown = new Cooldown(cooldown);
        }

        public override bool TargetInRange(SightingInfo info)
        {
            return info.HasLineOfSight && info.KnownDisplacement.sqrMagnitude < fireRange * fireRange;
        }

        public override void Activate()
        {
            if (Ready)
            {
                enabled = false;

                if (!skipAnimation)
                {
                    attackAnimation?.Invoke(FireProjectile);
                }
                else
                {
                    FireProjectile();
                }

                //indicator.PlayIndicatorAnimation(Fire, (".", .2f), ("..", .2f), ("...", .2f));
            }
        }
        void FireProjectile()
        {
            Vector3 displacement = TargetPos - fireOrigin.position;
            // account for auto dropping
            displacement.y += Mathf.Ceil(displacement.z);
            Vector3 direction = ((Vector2)displacement).normalized;// ((Vector2)displacement.GetDepthApparentPosition()).normalized;
            shot.FireCopy(fireOrigin.position + direction, direction * shotSpeed, character.damageMask, new DamageInfo(artifacts, damageMultiplier), displacement.z < -1);

            fireCooldown.StartCooldown();
            enabled = true;
        }

        public override void Cancel()
        {
            base.Cancel();
            fireCooldown.StartCooldown();
            enabled = true;
        }
    }
}