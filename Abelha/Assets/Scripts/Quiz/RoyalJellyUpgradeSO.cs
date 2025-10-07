// Scripts/GameSystems/RoyalJellyUpgradeSO.cs
using UnityEngine;
using System.Collections.Generic;

public enum VisualUpgradeType { None, Honeycomb, Flower }

[CreateAssetMenu(fileName = "NovoRJUpgrade", menuName = "Idle Bee Game/Royal Jelly Upgrade")]
public class RoyalJellyUpgradeSO : ScriptableObject
{
    public string upgradeID;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public VisualUpgradeType visualEffect; // O tipo de efeito visual que este upgrade ativa
    public List<int> costPerLevel; // Custo em Geleia Real para cada nível
    
    public int GetMaxLevel() { return costPerLevel.Count; }
}