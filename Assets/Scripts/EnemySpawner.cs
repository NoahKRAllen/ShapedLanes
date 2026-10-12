using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Spawns enemy units for one lane, driven by a WaveData asset. Attach one per lane,
/// alongside that lane's LaneWaypoints and UnitSpawner. Starts running its waves
/// automatically when the scene begins.
///
/// Enemies spawn at spawnPoint (typically the base end of the lane) and walk the same
/// LaneWaypoints the player's units use, but in reverse - see UnitMovementController.Initialize.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Tooltip("The lane this spawner sends enemies down (in reverse, base -> player spawn).")]
    [SerializeField] private LaneWaypoints lane;

    [Tooltip("Where enemies appear - typically at or near the base end of the lane.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Optional parent to keep the hierarchy tidy while enemies are alive. Leave empty to spawn at scene root.")]
    [SerializeField] private Transform enemiesContainer;

    [Tooltip("Defines the waves this lane will run.")]
    [SerializeField] private WaveData[] waveData;


    [SerializeField] private GameOver gameOver;
    private void Start()
    {
        if (waveData == null)
        {
           print($"{gameObject.name} has no WaveData assigned - no enemies will spawn.");
            return;
        }

        if (lane == null)
        {
            print($"{gameObject.name} has no lane assigned.");
            return;
        }

        if (spawnPoint == null)
        {
            print($"{gameObject.name} has no spawn point assigned.");
            return;
        }

        foreach (WaveData wave in waveData)
        {
            StartCoroutine(RunWave(wave));
        }
    }
    
    private void SpawnEnemy(GameObject toSpawn)
    {
        if (!toSpawn)
        {
            print($"{gameObject.name}'s WaveData has no enemyPrefab assigned.");
            return;
        }

        GameObject enemy = Instantiate(toSpawn, spawnPoint.position, spawnPoint.rotation, enemiesContainer);


        if (!enemy.TryGetComponent(out UnitMovementController mover)) return;
        int startWaypointIndex = lane.WaypointCount - 1;
        mover.Initialize(lane, startWaypointIndex, reversed: true);
    }

    private IEnumerator RunWave(WaveData wave)
    {
        if (gameOver.gameOver)
        {
            yield break;
        }
        if (wave.delayBeforeWave > 0f)
        {
            yield return new WaitForSeconds(wave.delayBeforeWave);
        }
        while (!gameOver.gameOver)
        {
            if (!(wave.spawnInterval > 0f)) continue;
            SpawnEnemy(wave.enemyPrefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }
}