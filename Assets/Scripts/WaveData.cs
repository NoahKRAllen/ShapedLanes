using UnityEngine;

/// <summary>
/// Defines the sequence of enemy waves for a single lane. One WaveData asset per lane.
/// Assign it to that lane's EnemySpawner.
/// </summary>
[CreateAssetMenu(fileName = "New Wave Data", menuName = "ShapedLanes/Wave Data")]
public class WaveData : ScriptableObject
{
    [Tooltip("Seconds to wait after the previous wave finishes before this wave starts. " +
                 "Applies to the first wave too (delay before combat begins).")]
    public float delayBeforeWave = 5f;

    [Tooltip("Seconds between each enemy spawn within this wave.")]
    public float spawnInterval = 1f;
    
    [Tooltip("The enemy prefab spawned by every wave in this asset. Must have a UnitMover component.")]
    public GameObject enemyPrefab;

}