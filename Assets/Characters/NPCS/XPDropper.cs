using UnityEngine;

namespace ColbyDoan
{
    public class XPDropper : MonoBehaviour
    {
        [SerializeField] Health health;

        public int xp = 10;

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