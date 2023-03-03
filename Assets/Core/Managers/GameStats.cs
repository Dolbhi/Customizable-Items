using System;

namespace ColbyDoan
{
    /// <summary>
    /// Static class for storing persistent infomation
    /// </summary>
    public static class GameStats
    {
        static int dataPoints = 0;
        public static event Action OnDataPointsChanged = delegate { };

        public static int GetXP()
        {
            return dataPoints;
        }
        public static void ChangeDataPoints(int change)
        {
            dataPoints += change;
            OnDataPointsChanged.Invoke();
        }
        public static bool TryDeductDataPoints(int deduction)
        {
            if (deduction <= dataPoints)
            {
                dataPoints -= deduction;
                OnDataPointsChanged.Invoke();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Reset everything to starting values
        /// </summary>
        public static void Reset()
        {
            dataPoints = 0;
        }
    }
}
