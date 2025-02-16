using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public int honeyProductionUpgradeCost = 10;
    public int carryLimitUpgradeCost = 15;

    public int honeyProductionIncrease = 1;
    public int carryLimitIncrease = 5;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void UpgradeHoneyProduction()
    {
        if (ResourceManager.Instance.storedHoney >= honeyProductionUpgradeCost)
        {
            ResourceManager.Instance.storedHoney -= honeyProductionUpgradeCost;
            ResourceManager.Instance.UpgradeHoneyProduction(honeyProductionIncrease);
            Debug.Log("Mel produzido por ciclo aumentado!");
        }
    }

    public void UpgradeCarryLimit()
    {
        if (ResourceManager.Instance.storedHoney >= carryLimitUpgradeCost)
        {
            ResourceManager.Instance.storedHoney -= carryLimitUpgradeCost;
            ResourceManager.Instance.UpgradeCarryLimit(carryLimitIncrease);
            Debug.Log("Capacidade de carga da abelha aumentada!");
        }
    }
}
