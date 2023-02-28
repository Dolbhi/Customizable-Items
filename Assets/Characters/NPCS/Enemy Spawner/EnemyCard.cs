// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using Physics;

    [CreateAssetMenu(fileName = "NewEnemySpawnCard", menuName = "Spawning/Create Enemy Card")]
    public class EnemyCard : ScriptableObject
    {
        public GameObject enemy;
        public float enemyRadius = .5f;
        public int weight = 10;
        public int cost = 5;
        public float swarmRadius = 20;
        public bool needsGround = true;
        public int minSwarmSize = 3;
        public int maxSwarmSize = 10;

        const int _maxAttempts = 50;

        /// <summary> spawns a swarm of its enemy centered on center </summary>
        /// <return> true if enemies could spawn </return>
        public bool SpawnSwarm(Vector3 center, ref int points)
        {
            // if (cost > points) return true;

            // abort if: 1. center above height limit 2. no ground when it needs it 3. solid in the way
            if (center.z > 2 || (needsGround && center.z < 0) || PhysicsSettings.CheckForSolids(center, .7f))
                return false;

            // spawn loop
            int count = Random.Range(minSwarmSize, maxSwarmSize + 1);
            while (count > 0 && points > 0)
            {
                // find point
                Vector3 chosenPoint;
                int attempts = 0;

                // try spawn at random points
                do
                {
                    if (attempts >= _maxAttempts) return false;
                    attempts++;

                    chosenPoint = (Vector3)Random.insideUnitCircle * swarmRadius + center;
                }
                while (PhysicsSettings.SolidsLinecast(center, chosenPoint) || !Spawn(chosenPoint)); // ensure line of sight with swarm center

                points -= cost;
                count--;
                // if (count >= maxSwarmSize) break;
            }
            Debug.Log("Spawned a squad of " + enemy.name + "(s)");
            return true;
        }

        /// <summary> spawns its enemy at position, returns true if successful </summary>
        public bool Spawn(Vector3 position)
        {
            if (needsGround && TileManager.Instance.GetTerrainHeight(position) != (int)position.z)
                return false;
            if (PhysicsSettings.CheckForSolids(position, enemyRadius))
                return false;

            if (!needsGround) position.z += .8f;

            // spawn
            Object.Instantiate(enemy, position, Quaternion.identity);
            return true;
        }
    }
}
