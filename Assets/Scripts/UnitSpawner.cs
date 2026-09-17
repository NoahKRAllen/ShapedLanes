using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [Tooltip("Where spawned units appear - e.g. the start of a lane.")]
    [SerializeField] private Transform spawnPoint;
 
    [Tooltip("Optional parent to keep the hierarchy tidy while units are alive. Leave empty to spawn at scene root.")]
    [SerializeField] private Transform unitsContainer;
 
    [Tooltip("The lane this spawner feeds units into. Each lane should have its own spawner + buttons.")]
    [SerializeField] private LaneWaypoints lane;
 
    public void SpawnUnit(GameObject unitPrefab)
    {
        if (unitPrefab == null)
        {
            print($"{gameObject.name}.SpawnUnit was called with no prefab assigned.");
            return;
        }
 
        if (spawnPoint == null)
        {
            print($"{gameObject.name} has no spawn point assigned.");
            return;
        }
 
        GameObject unit = Instantiate(unitPrefab, spawnPoint.position, spawnPoint.rotation, unitsContainer);
 
        UnitMovementController mover = unit.GetComponent<UnitMovementController>();
        if (mover != null && lane != null)
        {
            mover.Initialize(lane);
        }
    }
}
