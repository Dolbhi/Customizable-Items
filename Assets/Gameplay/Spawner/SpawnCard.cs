// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using Physics;

    [CreateAssetMenu(fileName = "NewSpawnCard", menuName = "Spawning/Create Spawn Card")]
    public class SpawnCard : ScriptableObject
    {
        public GameObject spawnObject;
        public float objectRadius = .5f;
        public int weight = 10;
        public int cost = 5;

        /// <return> true if objects could spawn </return>
        public virtual bool Spawn(Vector3 center, ref int points)
        {
            if (center.z > 2 || center.z < 0 || PhysicsSettings.CheckForSolids(center, objectRadius))
                return false;

            // spawn
            points -= cost;
            // center.z = Mathf.Floor(center.z);
            Object.Instantiate(spawnObject, center, Quaternion.identity);

            return true;
        }
    }
}
