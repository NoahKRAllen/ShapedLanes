using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

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
/// Movement is NavMeshAgent-driven. The agent's own path following handles per-frame
/// motion; unit-vs-unit separation comes from the agent's built-in obstacle avoidance
/// (tune Radius / Avoidance Priority per prefab in the inspector) instead of physics
/// collision like the old Rigidbody version.
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Unit))]
public class UnitMovementController : MonoBehaviour
{
    private enum MoveState { FollowingWaypoints, Attacking }

    [Header("Movement")]
    [Tooltip("How close (world units) to a waypoint counts as \"arrived\" and advances to the next one. " +
             "Too small and fast/wide units can overshoot and pivot back, causing jams - widen this " +
             "before shrinking unit speed if you see units turning around near a waypoint.")]
    [SerializeField] private float waypointArriveDistance = 4f;

    [Tooltip("How far a chased target must move before we bother repathing to it. " +
             "Avoids calling SetDestination every single frame while chasing.")]
    [SerializeField] private float targetRepathThreshold = 0.25f;


    private NavMeshAgent _agent;
    private LaneWaypoints _lane;
    private int _waypointIndex;
    private int _direction = 1;

    //Only serialized for easy viewing in editor without needed to activate debug mode
    [SerializeField] private Transform target;
    private Transform _attackingTarget;
    private Vector3 _lastChaseDestination;
    private float _scanTimer;
    private MoveState _state = MoveState.FollowingWaypoints;
    private Unit _unit;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _unit = GetComponent<Unit>();
        if (!_agent)
        {
            print("Missing NavMeshAgent on " + gameObject.name);
            gameObject.SetActive(false);
        }

        if (!_unit)
        {
            print("Missing unit on " + gameObject.name);
            gameObject.SetActive(false);
        }
        
        //Setting here in case this is forgotten on a prefab in the future
        _unit.isMobile = true;

        // Force the correct setup in code so every unit prefab works right regardless of
        // how its NavMeshAgent was configured in the inspector.
        _agent.updateRotation = true;   // agent handles smooth turning via its own angularSpeed
        _agent.updateUpAxis = false;    // flat lane - don't let it tilt to navmesh normal
        _agent.autoBraking = false;     // we decide when to stop (waypoint/attack state), not the agent's arrival curve
        _agent.stoppingDistance = 0f;
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
        
        _agent.speed = _unit.movementSpeed;
        SetWaypointDestination();
    }

    private void Update()
    {
        if (_state == MoveState.Attacking)
            TickAttacking();
        else
            TickFollowingWaypoints();
    }

    public void SetTarget(Transform newTarget)
    {
        if (!newTarget)
        {
            if (_attackingTarget)
            {
                _unit.combatController.StopAttack();
                _attackingTarget = null;
            }
            _state = MoveState.FollowingWaypoints;
            target = null;
            return;
        }
        _state = MoveState.Attacking;
        target = newTarget;
    }

    private void TickAttacking()
    {
        if (!target)
        {
            ResumeWaypoints();
            return;
        }
        
        float distance = Vector3.Distance(transform.position, target.position) - target.localScale[0] / 2;

        if (distance > _unit.agroRadius)
        {
            // Wandered out of range - give up and resume the lane from where we left off.
            target = null;
            ResumeWaypoints();
            return;
        }

        if (distance > _unit.attackRange)
        {
            if (_attackingTarget)
            {
                // Pushed back out of attack range - stop attacking until we're back in range.
                _unit.combatController.StopAttack();
                _attackingTarget = null;
            }
            ChaseTarget(target.position);
        }
        else
        {
            if (target != _attackingTarget)
            {
                _unit.combatController.StartAttack(target);
                _attackingTarget = target;
            }
            // In attack range - hold position. Agent avoidance still keeps units piling up
            // on the same target from overlapping each other.
            StopAgent();
        }
    }

    private void TickFollowingWaypoints()
    {
        if (!_lane|| _lane.WaypointCount == 0)
        {
            StopAgent();
            print("Either lane is null or we have no waypoints setup");
            return;
        }

        Transform waypoint = _lane.GetWaypoint(_waypointIndex);
        if (!waypoint)
        {
            StopAgent(); // Reached the end of the lane (arrived at the base / spawn, depending on direction).
            return;
        }

        //TODO: Trim this and replace it with callable function upon entering trigger boxes that will be setup across the lanes before each waypoint
        if (Vector3.Distance(transform.position, waypoint.position) <= waypointArriveDistance)
        {
            _waypointIndex += _direction;
            SetWaypointDestination();
        }
    }

    private void ResumeWaypoints()
    {
        _state = MoveState.FollowingWaypoints;
        _unit.combatController.StopAttack();
        _attackingTarget = null;
        SetWaypointDestination();
    }

    private void SetWaypointDestination()
    {
        Transform waypoint = _lane ? _lane.GetWaypoint(_waypointIndex) : null;
        if (!waypoint)
        {
            StopAgent();
            return;
        }

        _agent.isStopped = false;
        _agent.SetDestination(waypoint.position);
    }

    private void ChaseTarget(Vector3 destination)
    {
        _agent.isStopped = false;

        if ((destination - _lastChaseDestination).sqrMagnitude < targetRepathThreshold * targetRepathThreshold)
            return;

        _lastChaseDestination = destination;
        _agent.SetDestination(destination);
    }

    private void StopAgent()
    {
        _agent.isStopped = true;
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