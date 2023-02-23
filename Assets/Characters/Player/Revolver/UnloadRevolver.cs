using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using CharacterBase;

    [RequireComponent(typeof(RevolverShootSkill))]
    public class UnloadRevolver : CooldownSkill
    {
        public float cooldown = 3;

        [SerializeField] float shootInterval = .07f;

        RevolverShootSkill _revolverSkill;

        public override void Activate()
        {
            if (Ready)
            {
                StartCoroutine("RollShootCoroutine");

                cooldownHandler.StartCooldown(cooldown);
            }
        }

        IEnumerator RollShootCoroutine()
        {
            var waitInterval = new WaitForSeconds(shootInterval);
            int count = 0;
            while (_revolverSkill.loadedBullets > 0)
            {
                _revolverSkill.FireBullet(0);
                count++;
                yield return waitInterval;
            }
        }

        void OnDisable()
        {
            StopCoroutine("RollShootCoroutine");
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
