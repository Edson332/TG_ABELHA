// Scripts/CombatSystem/EnemyWaveSO.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyGroup
{
    public EnemyCombatDataSO enemyData; // O tipo de inimigo
    public int quantity = 1;          // Quantos desse inimigo
    // Poderia adicionar aqui um 'spawnPointIndex' se tiver múltiplos pontos de spawn para inimigos
}

[CreateAssetMenu(fileName = "NovaEnemyWave", menuName = "Idle Bee Game/Combat/Enemy Wave")]
public class EnemyWaveSO : ScriptableObject
{
    public string waveName = "Onda de Invasão Padrão";
    public List<EnemyGroup> enemyGroups = new List<EnemyGroup>();

    [Header("Recompensas da Onda")]
    public int honeyReward = 50;
    // Adicione outras recompensas (pólen, itens especiais, etc.)
}