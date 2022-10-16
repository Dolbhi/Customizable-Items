using UnityEngine;

namespace ColbyDoan
{
    public class XPDropper : MonoBehaviour
    {
        [SerializeField] Health health;

        public int xp = 1;

        void Awake()
        {
            health.OnDeath += RewardXP;
        }
        void RewardXP()
        {
            GameStats.ChangeXP(xp);
        }
    }
}