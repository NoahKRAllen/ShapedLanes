using UnityEngine;

public class UnitController : MonoBehaviour
{
    private enum MoveState { FollowingWaypoints, Attacking }
    
    [Tooltip("Only colliders on these layers count as valid targets.")]
    [SerializeField] private LayerMask enemyLayerMask;
    [Tooltip("How often (seconds) to re-scan for targets. Lower = more responsive, more expensive.")]
    [SerializeField] private float targetScanInterval = 0.25f;

    private static readonly Collider[] ScanBuffer = new Collider[16];

    private Rigidbody _rigidbody;
    private LaneWaypoints _lane;
    private int _waypointIndex;
    private int _direction = 1;
    [Tooltip("This is for readonly purposes to see what is actively being targeted")]
    [SerializeField] private Transform _target;
    private float _scanTimer;
    private MoveState _state = MoveState.FollowingWaypoints;
    private Unit _unit;
    
    private bool _withinRange;
    private float _nextAttackTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _unit = GetComponent<Unit>();
        if (_unit == null)
        {
            print("Missing unit on "  + gameObject.name);
            gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Call right after Instantiate to assign which lane this unit walks.
    /// Player units use the defaults (start at index 0, walk forward toward the base).
    /// Enemy units pass startWaypointIndex = lane.WaypointCount - 1 and reversed: true
    /// to walk from the base end back toward the player spawn.
    /// </summary>
    public void Initialize(LaneWaypoints lane, int startWaypointIndex = 0, bool reversed = false)
    {
        _lane = lane;
        _waypointIndex = startWaypointIndex;
        _direction = reversed ? -1 : 1;
    }
    // Update is called once per frame
    void Update()
    {
        if (_unit.health <= 0)
        {
            Destroy(gameObject);
        }
        
        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0f)
        {
            _scanTimer = targetScanInterval;
            //Scan for targets
        }

        if (_nextAttackTime > 0f)
        {
            _nextAttackTime -= Time.deltaTime;
        }
        if (_nextAttackTime <= 0f && _target)
        {
            _nextAttackTime = 1f / _unit.attackSpeed;
            //Do Attack
        }
        
    }
    private void FixedUpdate()
    {
        if (_state == MoveState.Attacking)
            TickAttacking();
        else
            TickFollowingWaypoints();
    }

    private void ScanForTarget()
    {
        // Keep the current target as long as it's alive and still in range, rather than
        // re-picking "closest" every scan - avoids flickering between two nearby enemies.
        if (_target && Vector3.Distance(transform.position, _target.position) <= _unit.agroRadius)
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

        _target = closest;
        _state = _target != null ? MoveState.Attacking : MoveState.FollowingWaypoints;
    }

    private void TickAttacking()
    {
        if (_target == null)
        {
            _state = MoveState.FollowingWaypoints;
            return;
        }

        float distance = Vector3.Distance(transform.position, _target.position) - _target.localScale[0] / 2;
        
        if (distance > _unit.agroRadius)
        {
            // Wandered out of range - give up and resume the lane from where we left off.
            _target = null;
            _state = MoveState.FollowingWaypoints;
            _unit.combatController.StopAttack();
            Stop();
            return;
        }
        
        if (distance > _unit.attackRange)
        {
            MoveTowards(_target.position);
        }
        else
        {
            //_unit.combatController.StartAttack(_target.GetComponent<Unit>());
            // TODO:Attack combat hook here
            
            // In attack range - hold position. Hook a combat/attack script in here later.
            // Still subject to physics separation, so units piling up on the same target
            // will push apart sideways rather than overlap.
            Stop();
        }
    }
    private void TickFollowingWaypoints()
    {
        if (_lane == null || _lane.WaypointCount == 0)
        {
            Stop();
            print("Either lane is null or we have no waypoints setup");
            return;
        }

        Transform waypoint = _lane.GetWaypoint(_waypointIndex);
        if (waypoint == null)
        {
            Stop(); // Reached the end of the lane (arrived at the base / spawn, depending on direction).
            return;
        }

        //TODO: Trim this and replace it with callable function upon entering trigger boxes that will be setup across the lanes before each waypoint
        //if (Vector3.Distance(transform.position, waypoint.position) <= waypointArriveDistance)
        //{
        //    _waypointIndex += _direction;
        //    return;
        //}

        MoveTowards(waypoint.position);
    }

    private void MoveTowards(Vector3 destination)
    {
        Vector3 flatDestination = new Vector3(destination.x, transform.position.y, destination.z);
        Vector3 direction = flatDestination - transform.position;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            Stop();
            return;
        }

        _rigidbody.linearVelocity = direction.normalized * _unit.movementSpeed;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Stop()
    {
        _rigidbody.linearVelocity = Vector3.zero;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!_unit) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _unit.agroRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _unit.attackRange);
    }
}
