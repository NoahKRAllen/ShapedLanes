using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
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
    
    
    
    private void OnDrawGizmosSelected()
    {
        if (!_unit) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _unit.agroRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _unit.attackRange);
    }
}
