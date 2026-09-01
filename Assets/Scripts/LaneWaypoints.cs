using UnityEngine;

public class LaneWaypoints : MonoBehaviour
{
    [Tooltip("Ordered points from the spawn end of the lane to the enemy base. " +
             "Place these around fixed towers once those exist.")]
    [SerializeField] private Transform[] waypoints;
 
    public int WaypointCount => waypoints != null ? waypoints.Length : 0;
 
    public Transform GetWaypoint(int index)
    {
        if (waypoints == null || index < 0 || index >= waypoints.Length)
            return null;
 
        return waypoints[index];
    }
 
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
 
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null) continue;
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            Gizmos.DrawWireSphere(waypoints[i].position, 0.25f);
        }
        Gizmos.DrawWireSphere(waypoints[^1].position, 0.25f);
    }
#endif
}
