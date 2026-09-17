using System;
using UnityEngine;

[Serializable]
public class Unit : MonoBehaviour
{
    public string unitName;
    [Header("Health")]
    public float health;
    public float maxHealth;
    
    [Header("Combat Stats")]
    public float damage;
    public float attackSpeed;
    public float attackRange;

    [Header("Movement Stats")]
    public float movementSpeed;
    public float agroRadius;
    public bool isMobile = false;
    [Header("Controls")]
    public UnitMovementController unitMovementController;
    public StationaryUnitController stationaryUnitController;
    public CombatController combatController;
    public TargetingController targetingController;
    
    private void Awake()
    {
        if(!combatController) combatController = GetComponent<CombatController>();
        if(!targetingController) targetingController = GetComponent<TargetingController>();
        health = maxHealth;
    }

}
