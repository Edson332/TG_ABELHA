// Scripts/CombatSystem/EnemyCombatDataSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NovoEnemyCombatData", menuName = "Idle Bee Game/Combat/Enemy Combat Data")]
public class EnemyCombatDataSO : CombatantBaseDataSO
{
    [Header("Configurações Específicas do Inimigo")]
    public float attackInterval = 2.0f; // Em segundos, quão frequentemente ataca (se for em tempo real)
                                        // Para turnos, isso pode não ser usado inicialmente.
    // Outros comportamentos específicos de inimigos podem vir aqui
}