using UnityEngine;

namespace ColbyDoan
{
    public class WeightedRandomizer
    {
        int[] _weights;
        int _sum;

        public WeightedRandomizer(params int[] weights)
        {
            SetWeights(weights);
        }

        /// <summary>
        /// Make a weighted selection of a random index
        /// </summary>
        /// <returns>Chosen index</returns>
        public int Choose()
        {
            // int randInt = Random.Range(0, sum);
            // for (int i = 0; randInt > _weights[i]; i++)
            // {

            // }
            int i = 0;
            int currentWeight = _weights[i];
            for (int randInt = Random.Range(0, _sum); randInt < currentWeight; randInt -= currentWeight)
            {
                i++;
                currentWeight = _weights[i];
            }
            return i;
        }

        /// <summary>
        /// Set weights and recalculates the sum, don't use negative weights
        /// </summary>
        /// <param name="weights">Array of new weights in order</param>
        public void SetWeights(int[] weights)
        {
            _weights = weights;
            _sum = 0;
            foreach (int i in weights)
            {
                _sum += i;
            }
        }
    }
}