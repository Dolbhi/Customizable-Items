// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    [CreateAssetMenu(fileName = "NewSpawningSettings", menuName = "Custom Assets/Create Spawning settings")]
    public class SpawningSettings : ScriptableObject
    {
        public EnemyCard[] toSpawn;
        public int pointRate;
        public int rateUpPeriod;

        int[] compoundedWeights;

        void OnValidate()
        {
            compoundedWeights = new int[toSpawn.Length];

            compoundedWeights[0] = toSpawn[0].weight;// first element
            for (int i = 1; i < toSpawn.Length; i++)// other elements henceforth
            {
                compoundedWeights[i] = toSpawn[i].weight + compoundedWeights[i - 1];
            }
        }

        public EnemyCard PickEnemyToSpawn()
        {
            int pick = Random.Range(1, compoundedWeights[compoundedWeights.Length - 1]);
            //Debug.Log("Pick: " + pick);
            int index = FindIndexByPick(pick);
            //Debug.Log("index: " + index);
            return toSpawn[index];
        }

        // binary search time
        int FindIndexByPick(int pick)
        {
            int start = IndexDivide(compoundedWeights.Length);
            return CheckIndex(start, pick, start);
        }
        int CheckIndex(int index, int pick, int div)
        {
            // min or max pick
            if (index <= 0) return 0;
            if (index >= compoundedWeights.Length - 1 && compoundedWeights.Length > 3) return compoundedWeights.Length - 1;

            if (pick <= compoundedWeights[index])
            {
                if (pick > compoundedWeights[index - 1]) return index;
                else return CheckIndex(index - IndexDivide(div), pick, IndexDivide(div));
            }
            else return CheckIndex(index + IndexDivide(div), pick, IndexDivide(div));
        }
        int IndexDivide(int div)
        {
            return Mathf.CeilToInt(div / 2f);
        }
    }
}