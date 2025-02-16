using UnityEngine;

public class ClickUpgrade : MonoBehaviour
{
    public int upgradeCost = 10;
    public int extraNectar = 1;

    public void BuyUpgrade()
    {
        if (ResourceManager.Instance.storedHoney >= upgradeCost)
        {
            ResourceManager.Instance.storedHoney -= upgradeCost;
            FlowerClick[] flowers = FindObjectsOfType<FlowerClick>();
            foreach (FlowerClick flower in flowers)
            {
                flower.nectarPerClick += extraNectar;
            }
            Debug.Log("Upgrade comprado! Néctar por clique aumentado.");
        }
    }
}
