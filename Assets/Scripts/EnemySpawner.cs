using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns enemy units for one lane, driven by a WaveData asset. Attach one per lane,
/// alongside that lane's LaneWaypoints and UnitSpawner. Starts running its waves
/// automatically when the scene begins.
///
/// Enemies spawn at spawnPoint (typically the base end of the lane) and walk the same
/// LaneWaypoints the player's units use, but in reverse - see UnitMover.Initialize.
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
    
    [Tooltip("Shuts down waves on player win")]
    [SerializeField] private bool playerWon = false;

    private void Start()
    {
        if (waveData == null)
        {
            Debug.LogWarning($"{nameof(EnemySpawner)} has no WaveData assigned - no enemies will spawn.", this);
            return;
        }

        if (lane == null)
        {
            Debug.LogError($"{nameof(EnemySpawner)} has no lane assigned.", this);
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError($"{nameof(EnemySpawner)} has no spawn point assigned.", this);
            return;
        }

        foreach (WaveData wave in waveData)
        {
            StartCoroutine(RunWave(wave));
        }
    }
    
    private void SpawnEnemy(GameObject toSpawn)
    {
        if (toSpawn == null)
        {
            Debug.LogWarning($"{nameof(EnemySpawner)}'s WaveData has no enemyPrefab assigned.", this);
            return;
        }

        GameObject enemy = Instantiate(toSpawn, spawnPoint.position, spawnPoint.rotation, enemiesContainer);


        if (!enemy.TryGetComponent(out UnitMover mover)) return;
        int startWaypointIndex = lane.WaypointCount - 1;
        mover.Initialize(lane, startWaypointIndex, reversed: true);
    }

    private IEnumerator RunWave(WaveData wave)
    {
        if (wave.delayBeforeWave > 0f)
        {
            yield return new WaitForSeconds(wave.delayBeforeWave);
        }
        while (!playerWon)
        {
            if (!(wave.spawnInterval > 0f)) continue;
            SpawnEnemy(wave.enemyPrefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    
}