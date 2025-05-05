using UnityEngine;

// Permite criar instâncias deste script como assets no menu do Unity
[CreateAssetMenu(fileName = "NovaBeeUpgradeData", menuName = "Idle Bee Game/Bee Upgrade Data", order = 1)]
public class BeeUpgradeData : ScriptableObject
{
    [Header("Identificação")]
    public string beeTypeName; // Ex: "WorkerBee", "ProducerBee" - DEVE CORRESPONDER ao usado nos scripts das abelhas

    [Header("Níveis Atuais (Salvos no Asset)")]
    public int nivelNectarColetado = 0;
    public int nivelMelProduzido = 0;
    public int nivelVelocidade = 0;

    [Header("Configuração Base - Néctar")]
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

    // --- Funções de Cálculo (específicas para este tipo) ---

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
        }
         // Importante: Marcar o ScriptableObject como "sujo" para que o Unity salve a mudança
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    // Opcional: Função para resetar níveis (para testes)
    public void ResetLevels()
    {
        nivelNectarColetado = 0;
        nivelMelProduzido = 0;
        nivelVelocidade = 0;
         #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}