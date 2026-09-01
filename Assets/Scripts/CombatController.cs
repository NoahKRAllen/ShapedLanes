using UnityEngine;

public class CombatController : MonoBehaviour
{
    private bool _withinRange;
    private Unit _unit;
    private float _nextAttackTime;
    
    //Only serialized for easy viewing in editor without needed to activate debug mode
    [SerializeField] private CombatController _target;

    void Awake()
    {
        _unit = GetComponent<Unit>();
        if (_unit != null) return;
        print("Missing unit on "  + gameObject.name);
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (_unit.health <= 0)
        {
            Destroy(gameObject);
        }
        if (_nextAttackTime > 0.0f)
        {
            _nextAttackTime -= Time.deltaTime;
        }

        if (!(_nextAttackTime <= 0.0f) || !_withinRange) return;
        if (_target)
        {
            print(_unit.name + " attacking " + _target.name);
            DealDamage();
            _nextAttackTime = 1f / _unit.attackSpeed;
                
        }
    }

    public void DealDamage(int incomingDamage = 0)
    {
        if (incomingDamage == 0)
        {
            _target.TakeDamage(_unit.damage);
        }
        else
        {
            _target.TakeDamage(incomingDamage);
        }
    }

    private void TakeDamage(float incomingDamage)
    {
        print(_unit.name + " damaged " + incomingDamage);
        _unit.health -= incomingDamage;
        print(_unit.name + "left with " + _unit.health);
        if (_unit.health > 0) return;
        print(_unit.name + " is dead");
        Destroy(gameObject);
    }
    
    public void StartAttack(Unit target)
    {
        _withinRange = true;
        _target = target.combatController;
    }

    public void StopAttack()
    {
        _withinRange = false;
        _target = null;
    }
}
