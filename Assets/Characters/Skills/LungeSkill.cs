using UnityEngine;

namespace ColbyDoan
{
    public class LungeSkill : Skill
    {
        public override bool Ready => enabled && lungeCooldown.Ready;

        public float speed = 10;
        public float lungeDist = 6;
        public float LungeDistSqr => lungeDist * lungeDist;
        public float cooldown = 3;
        Vector3 verticalSpeed;

        Cooldown lungeCooldown;

        KinematicObject kinematicObject;

        void Awake()
        {
            lungeCooldown = new Cooldown(cooldown);
            verticalSpeed = -Vector3.forward * (lungeDist / speed) * PhysicsSettings.gravity;
        }

        public override void SetUp(Character setTo)
        {
            base.SetUp(setTo);
            kinematicObject = setTo.kinematicObject;
        }

        // Lunge
        public override void Activate()
        {
            if (Ready)
            {
                Vector3 lungeVelocity = TargetPos - transform.position;
                lungeVelocity.Normalize();
                lungeVelocity *= speed;
                lungeVelocity += verticalSpeed;
                kinematicObject.AccelerateTo(lungeVelocity);

                lungeCooldown.StartCooldown();
            }
        }
    }
}