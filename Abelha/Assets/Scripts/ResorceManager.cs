using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public int nectar = 0;
    public int carriedHoney = 0;  // Mel sendo carregado pelas abelhas
    public int storedHoney = 0;   // Mel depositado e disponível para upgrades

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

    public void ConvertNectarToHoney(int amount)
    {
        if (nectar >= amount)
        {
            nectar -= amount;
            carriedHoney += amount;
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
}
