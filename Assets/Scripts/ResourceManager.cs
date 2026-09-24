using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager ResourceManagerInstance;
    
    [SerializeField] int _resourceTotal;
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
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (_resourceTotal.ToString() != resourceTotalText.text)
        {
            resourceTotalText.text = _resourceTotal.ToString();
        }
        _currentTimer += Time.deltaTime;
        if (_currentTimer > incomeTimer)
        {
            _resourceTotal += passiveIncome;
            _currentTimer = 0;
        }
    }
    
    //TODO: See if I can find a way to not have the add money function be public
    //This is called anytime enemy unit dies
    public void AddToResource(int amount)
    {
        _resourceTotal += amount;
    }

    public void RemoveFromResource(int amount)
    {
        _resourceTotal -= amount;
    }
    //This is called anytime button to spawn is clicked
    public int CheckResource()
    {
        return _resourceTotal;
    }
}
