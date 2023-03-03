// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    [CreateAssetMenu(fileName = "NewSpawningSettings", menuName = "Spawning/Create Spawning settings")]
    public class SpawningSettings : ScriptableObject
    {
        public SpawnCard[] toSpawn;
        public int startingPoints;
        public int pointScaling;
        public float wavePeriod;
        public float squadPeriod;
        public bool singleWave = false;

        WeightedRandomizer _enemyChooser;

        void OnValidate()
        {
            // set enemyChooser
            int[] weights = new int[toSpawn.Length];
            for (int i = 0; i < toSpawn.Length; i++)
            {
                weights[i] = toSpawn[i].weight;
            }
            _enemyChooser = new WeightedRandomizer(weights);
        }

        public SpawnCard PickCardToSpawn()
        {
            int index = _enemyChooser.Choose();
            //Debug.Log("index: " + index);
            return toSpawn[index];
        }

        // // binary search time
        // int FindIndexByPick(int pick)
        // {
        //     int start = IndexDivide(compoundedWeights.Length);
        //     return CheckIndex(start, pick, start);
        // }
        // int CheckIndex(int index, int pick, int div)
        // {
        //     // min or max pick
        //     if (index <= 0) return 0;
        //     if (index >= compoundedWeights.Length - 1 && compoundedWeights.Length > 3) return compoundedWeights.Length - 1;

        //     if (pick <= compoundedWeights[index])
        //     {
        //         if (pick > compoundedWeights[index - 1]) return index;
        //         else return CheckIndex(index - IndexDivide(div), pick, IndexDivide(div));
        //     }
        //     else return CheckIndex(index + IndexDivide(div), pick, IndexDivide(div));
        // }
        // int IndexDivide(int div)
        // {
        //     return Mathf.CeilToInt(div / 2f);
        // }
    }
}