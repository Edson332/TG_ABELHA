// Scripts/CombatSystem/PlayerBeeCombatDataSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NovaPlayerBeeCombatData", menuName = "Idle Bee Game/Combat/Player Bee Combat Data")]
public class PlayerBeeCombatDataSO : CombatantBaseDataSO
{
    [Header("Configurações Específicas da Abelha do Jogador")]
    [Tooltip("DEVE corresponder ao 'beeTypeName' no BeeUpgradeData e BeeManager para buscar upgrades.")]
    public string beeTypeNameForUpgrades = "WorkerBee"; // Ex: WorkerBee, ProducerBee, QueenBee
    // Poderíamos adicionar aqui modificadores específicos de combate que não vêm dos upgrades globais
}