using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private int _resourceTotal;
    [SerializeField] private TextMeshProUGUI resourceTotalText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_resourceTotal.ToString() != resourceTotalText.text)
        {
            resourceTotalText.text = _resourceTotal.ToString();
        }
    }
}
