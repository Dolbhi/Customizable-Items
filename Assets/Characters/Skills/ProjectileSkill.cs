using UnityEngine;
using UnityEngine.Events;

namespace ColbyDoan
{
    public class ProjectileSkill : Skill
    {
        public override bool Ready => enabled && fireCooldown.Ready;

        public UnityEvent<UnityAction> attackAnimation;
        public bool skipAnimation = false;

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
            Vector3 direction = ((Vector2)displacement.GetDepthApparentPosition()).normalized;
            shot.FireCopy(fireOrigin.position + direction, direction * shotSpeed, character.damageMask, new DamageInfo(character, damageMultiplier), displacement.z < -1);

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