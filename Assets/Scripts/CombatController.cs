using UnityEngine;

public class CombatController : MonoBehaviour
{
    private bool _withinRange;
    private Unit _unit;
    private float _nextAttackTime;
    
    [Tooltip("This is for readonly purposes to see what is actively being targeted")]
    [SerializeField] private CombatController target;

    void Awake()
    {
        _unit = GetComponent<Unit>();
        if (_unit != null) return;
        print("Missing Unit class on "  + gameObject.name);
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (_unit.health <= 0)
        {
            Die();
        }
        if (_nextAttackTime > 0.0f)
        {
            _nextAttackTime -= Time.deltaTime;
        }

        if (!(_nextAttackTime <= 0.0f) || !_withinRange) return;
        _nextAttackTime = 1f / _unit.attackSpeed;
        if (!target) return;
        print(_unit.name + " attacking " + target.name);
        target.TakeDamage(_unit.damage);
    }

    private void TakeDamage(float incomingDamage)
    {
        print(_unit.name + " damaged " + incomingDamage);
        _unit.health -= incomingDamage;
        print(_unit.name + "left with " + _unit.health);
    }

    private void Die()
    {
        ResourceManager.ResourceManagerInstance.AddToResource(_unit.killValue);
        Destroy(gameObject);
    }
    
    public void StartAttack(Transform newTarget)
    {
        if (!newTarget)
        {
            //May be able to remove this but would need to do null checks on every time this is called to ensure no crashing on null references
            print($"{gameObject.name} tried to start attack on null target");
            return;
        }
        _withinRange = true;
        target = newTarget.GetComponent<CombatController>();
    }

    public void StopAttack()
    {
        _withinRange = false;
        target = null;
    }
    
    
    
    /*
     * Potential future fix if we run into issues with the GetComponent<CombatController>() call as we ramp up unit amounts
     * We would just be able to do the TryGetValue to grab the target's CombatController instead but I'm unfamiliar with this so I went with some minimal checks instead for now
    private static readonly Dictionary<Transform, CombatController> Registry = new();

    private void OnEnable() => Registry[transform] = this;
    private void OnDisable() => Registry.Remove(transform);

    public void StartAttack(Transform newTarget)
    {
        if (!newTarget)
        {
            print($"{gameObject.name} tried to start attack on null target");
            return;
        }

        if (!Registry.TryGetValue(newTarget, out var targetController))
        {
            print($"No CombatController registered for {newTarget.name}");
            return;
        }

        _withinRange = true;
        target = targetController;
    }
     */
}
