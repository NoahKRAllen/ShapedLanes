using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Unit))]
public class StationaryUnitController : MonoBehaviour
{

    [Tooltip("This is for readonly purposes to see what is actively being targeted")]
    [SerializeField] private Transform target;

    private Transform _attackingTarget;
    private Unit _unit;
    private bool _attacking = false;

    void Start()
    {
        _unit = GetComponent<Unit>();
        if (_unit == null)
        {
            print("Missing unit on "  + gameObject.name);
            gameObject.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!_attacking) return;
        //TODO: Decide on what is needed to be handled for checking if current target is within range, and if so, then triggering the combatController.StartAttack on the target
        //Most of the code from TickAttacking in UnitMovementController can be placed here, minus the movement parts
        TickAttacking();
    }

    private void TickAttacking()
    {
        if (!target)
        {
            return;
        }
        float distance = Vector3.Distance(transform.position, target.position) - target.localScale[0] / 2;

        //AgroRadius from stationary units will be equal to AttackRange, leaving in to keep similar to mobile units for future debugging
        if (distance > _unit.agroRadius)
        {
            // Wandered out of range - remove target until TargetingController finds a new target
            target = null;
            _attacking = false;
            if (_attackingTarget)
            {
                _unit.combatController.StopAttack();
                _attackingTarget = null;
            }
            return;
        }

        if (distance < _unit.attackRange && target != _attackingTarget)
        {
            _unit.combatController.StartAttack(target);
            _attackingTarget = target;
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        if (!newTarget)
        {
            target = null;
            _attacking = false;
            if (_attackingTarget)
            {
                _unit.combatController.StopAttack();
                _attackingTarget = null;
            }
            return;
        }
        target = newTarget;
        _attacking = true;
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
