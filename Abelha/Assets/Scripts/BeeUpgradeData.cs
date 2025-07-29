// Scripts/GameSystems/BeeUpgradeData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NovaBeeUpgradeData", menuName = "Idle Bee Game/Bee Upgrade Data", order = 1)]
public class BeeUpgradeData : ScriptableObject
{
    [Header("Identificação")]
    [Tooltip("Nome de exibição para este tipo de abelha (usado em UIs). Ex: Abelha Trabalhadora")]
    public string combatantName; // Adicionado para nomes de exibição
    [Tooltip("ID interno do tipo de abelha. DEVE CORRESPONDER ao usado no BeeManager. Ex: WorkerBee")]
    public string beeTypeName; 

    [Header("Níveis Atuais (Salvos no Asset)")]
    public int nivelNectarColetado = 0;
    public int nivelMelProduzido = 0;
    public int nivelVelocidade = 0;
    public int nivelVidaCombate = 0;
    public int nivelAtaqueCombate = 0;

    [Header("Configuração Base - Néctar Coletado")]
    public float custoBaseNectar = 10f;
    public float fatorCustoNectar = 1.5f;
    public float bonusNectarPorNivel = 0.1f; // +10%

    [Header("Configuração Base - Produção Mel")]
    public float custoBaseProducao = 15f;
    public float fatorCustoProducao = 1.6f;
    public float bonusProducaoPorNivel = 0.1f; // +10%

    [Header("Configuração Base - Velocidade")]
    public float custoBaseVelocidade = 20f;
    public float fatorCustoVelocidade = 1.7f;
    public float bonusVelocidadePorNivel = 0.08f; // +8%

    [Header("Configuração Base - Vida de Combate")]
    public float custoBaseVida = 25f;
    public float fatorCustoVida = 1.8f;
    public float bonusVidaPorNivel = 0.15f; // +15% HP por nível

    [Header("Configuração Base - Ataque de Combate")]
    public float custoBaseAtaque = 30f;
    public float fatorCustoAtaque = 2.0f;
    public float bonusAtaquePorNivel = 0.12f; // +12% ATK por nível

    
    public float GetMultiplier(TipoUpgrade tipo)
    {
        switch (tipo)
        {
            case TipoUpgrade.NectarColetado:
                return 1f + (nivelNectarColetado * bonusNectarPorNivel);
            case TipoUpgrade.MelProduzido:
                return 1f + (nivelMelProduzido * bonusProducaoPorNivel);
            case TipoUpgrade.VelocidadeMovimento:
                return 1f + (nivelVelocidade * bonusVelocidadePorNivel);
            case TipoUpgrade.VidaCombate:
                return 1f + (nivelVidaCombate * bonusVidaPorNivel);
            case TipoUpgrade.AtaqueCombate:
                return 1f + (nivelAtaqueCombate * bonusAtaquePorNivel);
            default: return 1f;
        }
    }
    
    public float GetCost(TipoUpgrade tipo)
    {
        switch (tipo)
        {
            case TipoUpgrade.NectarColetado:
                return custoBaseNectar * Mathf.Pow(fatorCustoNectar, nivelNectarColetado);
            case TipoUpgrade.MelProduzido:
                return custoBaseProducao * Mathf.Pow(fatorCustoProducao, nivelMelProduzido);
            case TipoUpgrade.VelocidadeMovimento:
                return custoBaseVelocidade * Mathf.Pow(fatorCustoVelocidade, nivelVelocidade);
            case TipoUpgrade.VidaCombate:
                return custoBaseVida * Mathf.Pow(fatorCustoVida, nivelVidaCombate);
            case TipoUpgrade.AtaqueCombate:
                return custoBaseAtaque * Mathf.Pow(fatorCustoAtaque, nivelAtaqueCombate);
            default: return float.MaxValue;
        }
    }

    public void IncrementLevel(TipoUpgrade tipo)
    {
        switch (tipo)
        {
            case TipoUpgrade.NectarColetado: nivelNectarColetado++; break;
            case TipoUpgrade.MelProduzido: nivelMelProduzido++; break;
            case TipoUpgrade.VelocidadeMovimento: nivelVelocidade++; break;
            case TipoUpgrade.VidaCombate: nivelVidaCombate++; break;
            case TipoUpgrade.AtaqueCombate: nivelAtaqueCombate++; break;
        }
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    public void ResetLevels()
    {
        nivelNectarColetado = 0;
        nivelMelProduzido = 0;
        nivelVelocidade = 0;
        nivelVidaCombate = 0;
        nivelAtaqueCombate = 0;
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}