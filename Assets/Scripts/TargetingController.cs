using UnityEngine;

public class TargetingController : MonoBehaviour
{
    private float _scanTimer;
    [Tooltip("How often (seconds) to re-scan for targets. Lower = more responsive, more expensive.")]
    [SerializeField] float targetScanInterval;
    
    [SerializeField] private Transform target;
    
    [Tooltip("Only colliders on these layers count as valid targets.")]
    [SerializeField] private LayerMask enemyLayerMask;
    
    private static readonly Collider[] ScanBuffer = new Collider[16];
    
    private Unit _unit;

    private bool _stationaryUnit = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _unit = GetComponent<Unit>();
        if (_unit.unitMover == null)
        {
            _stationaryUnit = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0f)
        {
            _scanTimer = targetScanInterval;
            ScanForTarget();
        }
    }
    
    private void ScanForTarget()
    {
        // Keep the current target as long as it's alive and still in range, rather than
        // re-picking "closest" every scan - avoids flickering between two nearby enemies.
        if (target && Vector3.Distance(transform.position, target.position) <= _unit.agroRadius)
            return;

        int count = Physics.OverlapSphereNonAlloc(transform.position, _unit.agroRadius, ScanBuffer, enemyLayerMask);

        Transform closest = null;
        float closestSqrDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Transform candidate = ScanBuffer[i].transform;
            float sqrDist = (candidate.position - transform.position).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = candidate;
            }
        }

        target = closest;

        if (_stationaryUnit)
        {
            //TODO: Build new script for towers that handles the attack trigger that is currently done inside of UnitMover
            return;
        }
        
        if (target == null)
        {
            print("No target, swapping to follow waypoints");
            _unit.unitMover.SetToAttackingState(null);
            return;
        }
        print($"Target found, setting attacking state to {target.name}");
        _unit.unitMover.SetToAttackingState(target);
    }
}
