// Scripts/GameSystems/GameData.cs
using System.Collections.Generic;
using UnityEngine; // Adicionado para TipoRecurso, se estiver definido aqui

// --- ESTRUTURAS AUXILIARES ---
[System.Serializable]
public class ResourceData { public TipoRecurso type; public float amount; }

[System.Serializable]
public class BeeCountData { public string beeType; public int currentCount; }

[System.Serializable]
public class BeeUpgradeSaveData { public string beeTypeName; public int nectarLevel; public int productionLevel; public int speedLevel; public int combatHealthLevel; public int combatAttackLevel; }

[System.Serializable]
public class AchievementSaveData { public int index; public bool isUnlocked; public bool hasBeenViewed; }

[System.Serializable]
public class RoyalJellyUpgradeSaveData { public string upgradeID; public int level; }


// --- CLASSE PRINCIPAL GameData (COM O ATRIBUTO ESSENCIAL) ---
[System.Serializable] // <<<--- ESTA LINHA É CRUCIAL
public class GameData
{
    public List<ResourceData> resourceAmounts;
    public List<BeeCountData> beeCounts;
    public List<BeeUpgradeSaveData> beeUpgradeLevels;
    public List<string> completedTutorialIDs;
    // public bool hasQueenBeeBeenPurchasedEver; // Removido se não estiver usando
    public List<AchievementSaveData> achievementStatus;
    public List<RoyalJellyUpgradeSaveData> royalJellyUpgradeLevels;

    // Construtor Padrão
    public GameData()
    {
        resourceAmounts = new List<ResourceData>();
        beeCounts = new List<BeeCountData>();
        beeUpgradeLevels = new List<BeeUpgradeSaveData>();
        completedTutorialIDs = new List<string>();
        // hasQueenBeeBeenPurchasedEver = false; // Removido
        achievementStatus = new List<AchievementSaveData>();
        royalJellyUpgradeLevels = new List<RoyalJellyUpgradeSaveData>();
    }
}