using System;

namespace ColbyDoan
{
    /// <summary>
    /// Static class for storing persistent infomation
    /// </summary>
    public static class GameStats
    {
        static int xp = 0;
        public static event Action OnXPChanged = delegate { };

        public static int GetXP()
        {
            return xp;
        }
        public static void ChangeXP(int change)
        {
            xp += change;
            OnXPChanged.Invoke();
        }

        /// <summary>
        /// Reset everything to starting values
        /// </summary>
        public static void Reset()
        {
            xp = 0;
        }
    }
}
