using UnityEngine;

/// <summary>
/// Moves a unit along a LaneWaypoints path. While moving, periodically scans for the
/// closest enemy within aggroRadius; if one is found, switches to chasing/holding at that
/// enemy instead of the next waypoint. When the target dies or leaves aggro range, resumes
/// the lane from the same waypoint index it was on - it doesn't restart from the spawn.
///
/// Attack execution isn't implemented here - once within attackRange the unit just holds
/// position, ready for a combat script to hook into later (e.g. an OnAttackRange event).
///
/// Supports walking a lane forward (player units, spawn -> base) or in reverse
/// (enemy units, base -> spawn) via the "reversed" flag passed to Initialize.
///
/// Movement is Rigidbody-driven (velocity toward the destination each FixedUpdate) rather
/// than direct transform manipulation. This is purely so overlapping units physically push
/// each other apart via Unity's collision solver instead of stacking on the same spot -
/// there's no gravity, tipping, or bouncing involved, all of that is frozen off below.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class UnitMover : MonoBehaviour
{
    private enum MoveState { FollowingWaypoints, Attacking }
    //[SerializeField] private float moveSpeed = 3.5f;


    
    [Header("Movement")]
    [SerializeField] private float waypointArriveDistance = 0.5f;

    [Header("Targeting")]
    //[Tooltip("Radius within which the unit will notice and engage enemies.")]
    //[SerializeField] private float aggroRadius = 5f;
    //[Tooltip("Distance at which the unit stops closing in and holds position to attack. Must be <= aggroRadius.")]
    //[SerializeField] private float attackRange = 1.5f;
    [Tooltip("Only colliders on these layers count as valid targets.")]
    [SerializeField] private LayerMask enemyLayerMask;
    [Tooltip("How often (seconds) to re-scan for targets. Lower = more responsive, more expensive.")]
    [SerializeField] private float targetScanInterval = 0.25f;

    private static readonly Collider[] ScanBuffer = new Collider[16];

    private Rigidbody _rigidbody;
    private LaneWaypoints _lane;
    private int _waypointIndex;
    private int _direction = 1;
    [SerializeField] private Transform _target;
    private float _scanTimer;
    private MoveState _state = MoveState.FollowingWaypoints;
    private Unit _unit;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _unit = GetComponent<Unit>();
        if(!_rigidbody)
        {
            print("Missing rigidbody on" + gameObject.name);
            gameObject.SetActive(false);
        }

        if (_unit == null)
        {
            print("Missing unit on "  + gameObject.name);
            gameObject.SetActive(false);
        }
        
        // Physics here exists purely to resolve overlap between units - not for gravity,
        // tipping, or bouncing. Force the correct setup in code so every unit prefab works
        // right regardless of how its Rigidbody was configured in the inspector.
        _rigidbody.useGravity = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
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

    private void Update()
    {
        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0f)
        {
            _scanTimer = targetScanInterval;
            ScanForTarget();
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
            _unit.combatController.StartAttack(_target.GetComponent<Unit>());
            // TODO:Attack combat hook here
            
            // In attack range - hold position. Hook a combat/attack script in here later.
            // Still subject to physics separation, so units piling up on the same target
            // will push apart sideways rather than overlap.
            Stop();
        }
    }

    public void TargetDead()
    {
        _target = null; 
    }
    private void TickFollowingWaypoints()
    {
        if (_lane == null || _lane.WaypointCount == 0)
        {
            Stop();
            return;
        }

        Transform waypoint = _lane.GetWaypoint(_waypointIndex);
        if (waypoint == null)
        {
            Stop(); // Reached the end of the lane (arrived at the base / spawn, depending on direction).
            return;
        }

        if (Vector3.Distance(transform.position, waypoint.position) <= waypointArriveDistance)
        {
            _waypointIndex += _direction;
            return;
        }

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