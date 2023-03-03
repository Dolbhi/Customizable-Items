using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

// using ColbyDoan.Attributes;

namespace ColbyDoan
{
    public class EnemySpawner : MonoBehaviour
    {
        public SpawningSettings settings;

        public Bounds spawnArea;

        int _waveNumber = 0;

        // public Transform spawnAround;
        // public float maxSpawningRadius = 70;
        // public float minSpawningRadius = 10;
        // public float periodMin;
        // public float periodMax;

        // public bool pauseSpawning;

        // [ReadOnly][SerializeField] int points;
        // [ReadOnly][SerializeField] EnemyCard nextEnemy;

        float levelStartTime;
        float LevelDuration => Time.time - levelStartTime;

        // public Transform testPos;
        // [ContextMenu("Test spawn")]
        // void Test()
        // {
        //     int pnt = 50;
        //     spawningSettings.toSpawn[0].SpawnSwarm(testPos.position, ref pnt);
        // }

        WaitForSeconds _waveWait;
        WaitForSeconds _squadWait;

        void Start()
        {
            _waveWait = new WaitForSeconds(settings.wavePeriod);
            _squadWait = new WaitForSeconds(settings.squadPeriod);

            StartCoroutine("WaveLoop");
            levelStartTime = Time.time;
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator WaveLoop()
        {
            while (true)
            {
                // Spawn wave
                Debug.Log("Spawning wave " + _waveNumber + "...", this);
                int pointsLeft = settings.startingPoints + _waveNumber * settings.pointScaling;
                while (pointsLeft > 0)
                {
                    // Spawn squard
                    EnemyCard enemyToSpawn = settings.PickEnemyToSpawn();

                    // pick a spot within range and try to spawn swarm, repeat until true is returned(insufficient points)
                    Vector3 swarmPos = Vector3.zero;
                    do
                    {
                        swarmPos.x = Random.Range(spawnArea.min.x, spawnArea.max.x);
                        swarmPos.y = Random.Range(spawnArea.min.y, spawnArea.max.y);
                        swarmPos.z = TileManager.Instance.GetTerrainHeight(swarmPos) + .1f;
                        if (swarmPos.z < 0) continue;
                        Debug.Log("Trying to spawn " + enemyToSpawn.enemy.name + "...", this);
                    }
                    while (!enemyToSpawn.SpawnSwarm(swarmPos, ref pointsLeft));

                    Debug.Log("Spawned squad of " + enemyToSpawn.enemy.name + ", " + pointsLeft + " points left", this);

                    yield return _squadWait;
                }

                yield return _waveWait;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
        }

        // IEnumerator SpawnLoop()
        // {
        //     while (true)
        //     {
        //         nextEnemy = spawningSettings.PickEnemyToSpawn();
        //         yield return new WaitForSeconds(Random.Range(periodMin, periodMax));

        //         // skip spawning
        //         if (pauseSpawning || nextEnemy.cost > points) continue;

        //         // pick a spot within range and try to spawn swarm, repeat until true is returned(insufficient points)
        //         Vector3 swarmPos;
        //         do
        //         {
        //             swarmPos = (Random.rotationUniform * Vector2.right) * Random.Range(minSpawningRadius, maxSpawningRadius) + spawnAround.position;
        //             swarmPos.z = TileManager.Instance.GetTerrainHeight(swarmPos) + .1f;
        //         }
        //         while (!nextEnemy.SpawnSwarm(swarmPos, ref points));
        //     }
        // }

        // IEnumerator PointsLoop()
        // {
        //     while (true)
        //     {
        //         yield return new WaitForSeconds(1);
        //         points += spawningSettings.pointRate + ((spawningSettings.rateUpPeriod != 0) ? (int)(LevelDuration / spawningSettings.rateUpPeriod) : 0);
        //     }
        // }

        // void RandomSpawnInArea(EnemyBehaviour enemy, Vector2 center, float radius)
        // {
        //     Instantiate(enemy, center + Random.insideUnitCircle * radius, Quaternion.identity);
        // }
    }
}