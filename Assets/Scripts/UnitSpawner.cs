using Mono.Cecil;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [Tooltip("Where spawned units appear - e.g. the start of a lane.")]
    [SerializeField] private Transform spawnPoint;
 
    [Tooltip("Optional parent to keep the hierarchy tidy while units are alive. Leave empty to spawn at scene root.")]
    [SerializeField] private Transform unitsContainer;
 
    [Tooltip("The lane this spawner feeds units into. Each lane should have its own spawner + buttons.")]
    [SerializeField] private LaneWaypoints lane;
 
    public void SpawnUnit(Unit unitPrefab)
    {
        if (!unitPrefab)
        {
            print($"{gameObject.name}.SpawnUnit was called with no prefab assigned.");
            return;
        }
 
        if (!spawnPoint)
        {
            print($"{gameObject.name} has no spawn point assigned.");
            return;
        }

        if (ResourceManager.ResourceManagerInstance.CheckResource() < unitPrefab.spawnCost)
        {
            print($"Not enough funds for spawning {unitPrefab.name}");
            return;
        }
        GameObject unit = Instantiate(unitPrefab.gameObject, spawnPoint.position, spawnPoint.rotation, unitsContainer);
        unit.SetActive(true);
        ResourceManager.ResourceManagerInstance.RemoveFromResource(unitPrefab.spawnCost);
        UnitMovementController mover = unit.GetComponent<UnitMovementController>();
        if (mover != null && lane != null)
        {
            mover.Initialize(lane);
        }
    }
}
