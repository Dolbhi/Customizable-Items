using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using CharacterBase;
    using Physics;

    public class DodgeRollSkill : CooldownSkill
    {
        public float verticalDodgeSpeed = 2;
        public float dodgeSpeedMultiplier = 5;
        public float dodgeCooldown = 3;

        public override void Activate()
        {
            if (Ready)
            {
                Vector3 direction = character.Velocity.sqrMagnitude == 0 ? character.FacingDirection.normalized : character.Velocity.normalized;
                Vector3 velocity = direction * Stats.speed.FinalValue * dodgeSpeedMultiplier + verticalDodgeSpeed * Vector3.forward;
                character.kinematicObject.AccelerateTo(velocity);

                cooldownHandler.StartCooldown(dodgeCooldown);
            }
        }
    }
}
