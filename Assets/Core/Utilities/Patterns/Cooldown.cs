using UnityEngine;

namespace ColbyDoan
{
    [System.Serializable]
    public class Cooldown
    {
        public bool Ready => ReadyTime <= Time.time;
        public float ReadyTime { get; private set; }
        public float TimeLeft => Mathf.Max(ReadyTime - Time.time, 0);
        public float ProgressFraction => Mathf.Clamp01(1 - (TimeLeft / cooldownDuration));

        public float cooldownDuration;
        public float cooldownMultiplier = 1;

        public Cooldown(float cooldown)
        {
            cooldownDuration = cooldown;
        }

        public void StartCooldown()
        {
            ReadyTime = Time.time + cooldownDuration * cooldownMultiplier;
        }
        public void ForceFinish()
        {
            ReadyTime = 0;
        }

        public void IncreaseCurrentCooldown(float increase)
        {
            if (Ready) return;
            ReadyTime += increase;
        }
        public void ReduceCurrentCooldown(float reduction)
        {
            ReadyTime -= reduction;
        }
    }
}