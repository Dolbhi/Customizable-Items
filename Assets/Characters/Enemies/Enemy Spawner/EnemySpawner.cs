using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ColbyDoan.Attributes;

namespace ColbyDoan
{
    public class EnemySpawner : MonoBehaviour
    {
        public SpawningSettings spawningSettings;

        public Transform spawnAround;
        public float maxSpawningRadius = 70;
        public float minSpawningRadius = 10;
        public float periodMin;
        public float periodMax;

        public bool pauseSpawning;

        [ReadOnly][SerializeField] int points;
        [ReadOnly][SerializeField] EnemyCard nextEnemy;

        float levelStartTime;
        float LevelDuration => Time.time - levelStartTime;

        // public Transform testPos;
        // [ContextMenu("Test spawn")]
        // void Test()
        // {
        //     int pnt = 50;
        //     spawningSettings.toSpawn[0].SpawnSwarm(testPos.position, ref pnt);
        // }

        void Start()
        {
            StartCoroutine("PointsLoop");
            StartCoroutine("SpawnLoop");
            levelStartTime = Time.time;
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator SpawnLoop()
        {
            while (true)
            {
                nextEnemy = spawningSettings.PickEnemyToSpawn();
                yield return new WaitForSeconds(Random.Range(periodMin, periodMax));

                // skip spawning
                if (pauseSpawning || nextEnemy.cost > points) continue;

                // pick a spot within range and try to spawn swarm, repeat until true is returned(insufficient points)
                Vector3 swarmPos;
                do
                {
                    swarmPos = (Random.rotationUniform * Vector2.right) * Random.Range(minSpawningRadius, maxSpawningRadius) + spawnAround.position;
                    swarmPos.z = TileManager.Instance.GetTerrainHeight(swarmPos) + .1f;
                }
                while (!nextEnemy.SpawnSwarm(swarmPos, ref points));
            }
        }

        IEnumerator PointsLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                points += spawningSettings.pointRate + ((spawningSettings.rateUpPeriod != 0) ? (int)(LevelDuration / spawningSettings.rateUpPeriod) : 0);
            }
        }

        // void RandomSpawnInArea(EnemyBehaviour enemy, Vector2 center, float radius)
        // {
        //     Instantiate(enemy, center + Random.insideUnitCircle * radius, Quaternion.identity);
        // }
    }
}