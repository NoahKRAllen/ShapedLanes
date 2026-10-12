using System;
using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager ResourceManagerInstance;
    
    [SerializeField] int resourceTotal;
    [SerializeField] private TextMeshProUGUI resourceTotalText;
    [SerializeField] private float incomeTimer;
    private float _currentTimer;
    [SerializeField] private int passiveIncome;

    private void Awake()
    {
        if (ResourceManagerInstance != null && ResourceManagerInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        ResourceManagerInstance = this;
    }

    private void Start()
    {
        resourceTotal = 100;
        resourceTotalText.text = "$" + resourceTotal.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (resourceTotal.ToString() != resourceTotalText.text)
        {
            resourceTotalText.text = "$" + resourceTotal.ToString();
        }
        _currentTimer += Time.deltaTime;
        if (_currentTimer > incomeTimer)
        {
            resourceTotal += passiveIncome;
            _currentTimer = 0;
        }
    }
    
    //TODO: See if I can find a way to not have the add money function be public
    //This is called anytime enemy unit dies
    public void AddToResource(int amount)
    {
        resourceTotal += amount;
    }

    public void RemoveFromResource(int amount)
    {
        resourceTotal -= amount;
    }
    //This is called anytime button to spawn is clicked
    public int CheckResource()
    {
        return resourceTotal;
    }
}
