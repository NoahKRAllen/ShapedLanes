using System;
using UnityEngine;

[System.Serializable]
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

    [Header("Controls")]
    public UnitMover unitMover;
    public CombatController combatController;
    public TargetingController targetingController;
    private void Awake()
    {
        if(!combatController) combatController = GetComponent<CombatController>();
        if(!targetingController) targetingController = GetComponent<TargetingController>();
        health = maxHealth;
    }

}
