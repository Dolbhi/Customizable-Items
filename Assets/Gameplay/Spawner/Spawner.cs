using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

// using ColbyDoan.Attributes;

namespace ColbyDoan
{
    public class Spawner : MonoBehaviour
    {
        const int ATTEMPT_LIMIT = 1000;

        public SpawningSettings settings;

        public Bounds spawnArea;

        int _waveNumber = 0;

        public bool logSpawning;

        float _levelStartTime;
        float LevelDuration => Time.time - _levelStartTime;

        WaitForSeconds _waveWait;
        WaitForSeconds _spawnWait;

        void Start()
        {
            _waveWait = new WaitForSeconds(settings.wavePeriod);
            _spawnWait = new WaitForSeconds(settings.squadPeriod);

            StartCoroutine("WaveLoop");
            _levelStartTime = Time.time;
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
                int attempts = 0;
                while (pointsLeft > 0)
                {
                    // Spawn one card
                    SpawnCard objectToSpawn = settings.PickCardToSpawn();

                    // pick a spot within range and try to spawn swarm, repeat until true is returned(insufficient points)
                    Vector3 spawnPos = Vector3.zero;
                    do
                    {
                        spawnPos.x = Random.Range(spawnArea.min.x, spawnArea.max.x);
                        spawnPos.y = Random.Range(spawnArea.min.y, spawnArea.max.y);
                        spawnPos.z = TileManager.Instance.GetTerrainHeight(spawnPos) + .05f;
                        attempts++;
                        if (logSpawning)
                            Debug.Log("Trying to spawn " + objectToSpawn.spawnObject.name + "...", this);
                    }
                    while (!objectToSpawn.Spawn(spawnPos, ref pointsLeft) && attempts < ATTEMPT_LIMIT);

                    if (attempts >= ATTEMPT_LIMIT)
                    {
                        Debug.LogWarning("Spawn attempt limit reached", this);
                    }

                    if (logSpawning)
                        Debug.Log("Spawned " + objectToSpawn.spawnObject.name + ", " + pointsLeft + " points left", this);

                    yield return _spawnWait;
                }

                // no loop if only single wave
                if (settings.singleWave) yield break;
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