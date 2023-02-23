using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using CharacterBase;

    [RequireComponent(typeof(RevolverShootSkill))]
    public class AutoloadRevolver : CooldownSkill
    {
        public float cooldown = 3;

        RevolverShootSkill _revolverSkill;

        public override void Activate()
        {
            if (!Ready) return;

            _revolverSkill.cooldownHandler.ForceFinish();
            _revolverSkill.LoadBlueBullet(_revolverSkill.cylinderCount);
            cooldownHandler.StartCooldown(cooldown);
        }

        void OnValidate()
        {
            if (_revolverSkill == null)
            {
                _revolverSkill = GetComponent<RevolverShootSkill>();
            }
        }
    }
}