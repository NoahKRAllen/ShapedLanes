using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    [SerializeField] private Unit playerMelee;
    [SerializeField] private Unit playerRanged;
    [SerializeField] private Transform allPlayerUnits;
    public void UpgradeMeleeHealth()
    {
        if (!playerMelee)
        {
            print($"Upgrade Melee Health was called with no prefab assigned.");
            return;
        }
        //Hard coding the cost for now, I know its bad I just want to get this practice project done
        if (ResourceManager.ResourceManagerInstance.CheckResource() < 10)
        {
            print($"Not enough funds for upgrading Melee Health");
            return;
        }
        ResourceManager.ResourceManagerInstance.RemoveFromResource(10);
        playerMelee.health += 10;
        playerMelee.maxHealth += 10;
        foreach (Transform unit in allPlayerUnits)
        {
            if (unit.CompareTag("Melee"))
            {
                unit.GetComponent<Unit>().health += 10;
                unit.GetComponent<Unit>().maxHealth += 10;
            }
        }
    }
    public void UpgradeRangedDamage()
    {
        if (!playerRanged)
        {
            print($"Upgrade Ranged Damage was called with no prefab assigned.");
            return;
        }
        //Hard coding the cost for now, I know its bad I just want to get this practice project done
        if (ResourceManager.ResourceManagerInstance.CheckResource() < 15)
        {
            print($"Not enough funds for upgrading Ranged Damage");
            return;
        }
        ResourceManager.ResourceManagerInstance.RemoveFromResource(15);
        playerRanged.damage += 5;
        foreach (Transform unit in allPlayerUnits)
        {
            if (unit.CompareTag("Ranged"))
            {
                unit.GetComponent<Unit>().damage += 5;
            }
        }
    }
}
