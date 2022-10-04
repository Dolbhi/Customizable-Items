using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class OverloadRevolverSkill : CooldownSkill
    {
        public float overloadCooldown = 3;
        public float overloadInaccuracy = 40;

        public int bulletCount = 6;
        // public float baseFirePeriod = .2f;
        // bool firing = false;
        // public float unloadDuration = 1;

        public RevolverShootSkill shootSkill;

        // public override bool Ready => base.Ready && !firing;

        public override void Activate()
        {
            if (!Ready) return;

            // if (shootSkill.loadedBullets > 0)
            // {
            //     // unload once then again after a delay
            //     while (shootSkill.loadedBullets > 0)
            //         shootSkill.FireBullet(overloadInaccuracy);
            //     StartCoroutine(DelayedFireCoroutine());
            // }
            // else

            // just unload at once
            // shootSkill.Reload();
            // while (shootSkill.loadedBullets > 0)

            for (int i = bulletCount; i > 0; i--)
            {
                shootSkill.FireBullet(overloadInaccuracy);
            }
            // shootSkill.zoinked++;

            // StartCoroutine("RapidFireCoroutine");
            cooldownHandler.StartCooldown(overloadCooldown);
        }

        // IEnumerator RapidFireCoroutine()
        // {
        //     StartCoroutine("StopSkillCoroutine");

        //     firing = true;
        //     WaitForSeconds period = new WaitForSeconds(baseFirePeriod / Stats.attackSpeed.FinalValue);

        //     while (firing)
        //     {
        //         shootSkill.FireBullet(overloadInaccuracy);
        //         yield return period;
        //     }
        // }

        // // stop the firing loop
        // WaitForSeconds unloadDurationWait = new WaitForSeconds(1);
        // IEnumerator StopSkillCoroutine()
        // {
        //     yield return unloadDurationWait;

        //     base.Cancel();
        //     firing = false;
        //     StopCoroutine("RapidFireCoroutine");
        // }

        // IEnumerator DelayedFireCoroutine()
        // {
        //     yield return new WaitForSeconds(.1f);
        //     // load and fire
        //     shootSkill.Reload();
        //     while (shootSkill.loadedBullets > 0)
        //         shootSkill.FireBullet(overloadInaccuracy);

        //     cooldownHandler.StartCooldown(overloadCooldown);

        //     if (shootSkill.CooldownLeft <= 0)
        //         shootSkill.StartCooldown();
        // }
    }
}
