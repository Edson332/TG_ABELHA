using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public int nectar = 0;
    public int carriedHoney = 0;
    public int storedHoney = 0;

    public int honeyPerCycle = 1;  // Padrão: 1 mel por ciclo
    public int beeCarryLimit = 10; // Padrão: 10 mel carregado por vez

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddNectar(int amount)
    {
        nectar += amount;
    }

    public void ConvertNectarToHoney()
    {
        if (nectar >= honeyPerCycle)
        {
            nectar -= honeyPerCycle;
            carriedHoney += honeyPerCycle;
        }
    }

    public void DepositHoney(int amount)
    {
        if (carriedHoney >= amount)
        {
            carriedHoney -= amount;
            storedHoney += amount;
        }
    }

    public void UpgradeHoneyProduction(int extraAmount)
    {
        honeyPerCycle += extraAmount;
    }

    public void UpgradeCarryLimit(int extraLimit)
    {
        beeCarryLimit += extraLimit;
    }
}
