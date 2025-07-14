
using System.Collections.Generic;

// [System.Serializable] é essencial! Ele diz ao Unity que esta classe
// e seus campos podem ser convertidos para formatos como JSON.
[System.Serializable]
public class GameData
{
    // --- Recursos ---
    // Usamos um dicionário serializável ou uma lista para os recursos.
    // JsonUtility do Unity não serializa dicionários diretamente, então uma lista é mais fácil.
    public List<ResourceData> resourceAmounts;

    // --- Abelhas ---
    public List<BeeCountData> beeCounts;

    // --- Upgrades ---
    public List<BeeUpgradeSaveData> beeUpgradeLevels;

    // --- Tutoriais ---
    // HashSet também não é serializado por JsonUtility, então usamos uma lista.
    public List<string> completedTutorialIDs;

    // --- Construtor Padrão ---
    // Define os valores para um jogo novo.
    public GameData()
    {
        resourceAmounts = new List<ResourceData>();
        beeCounts = new List<BeeCountData>();
        beeUpgradeLevels = new List<BeeUpgradeSaveData>();
        completedTutorialIDs = new List<string>();
    }
}


// --- Classes Auxiliares para Serialização ---

[System.Serializable]
public class ResourceData
{
    public TipoRecurso type;
    public float amount;
}

[System.Serializable]
public class BeeCountData
{
    public string beeType;
    public int currentCount;
    // maxCount geralmente não é salvo, pois é parte do design, a menos que seja atualizável.
}

[System.Serializable]
public class BeeUpgradeSaveData
{
    public string beeTypeName;
    public int nectarLevel;
    public int productionLevel;
    public int speedLevel;

    public int combatHealthLevel; // --- ADICIONADO ---
    public int combatAttackLevel; 
}
