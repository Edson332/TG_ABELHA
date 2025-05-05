using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Para usar Linq (Find)

// O Enum continua o mesmo
public enum TipoUpgrade
{
    NectarColetado,
    MelProduzido,
    VelocidadeMovimento
}

public class GerenciadorUpgrades : MonoBehaviour
{
    // --- Singleton ---
    public static GerenciadorUpgrades Instancia { get; private set; }

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
        InitializeBeeTrackers();
    }
    // --- Fim Singleton ---

    [Header("Dados de Upgrade por Tipo de Abelha")]
    // Arraste os assets BeeUpgradeData criados aqui no Inspector
    public List<BeeUpgradeData> todosTiposUpgradeData;

    // Dicionário para acesso rápido aos dados pelo nome do tipo
    private Dictionary<string, BeeUpgradeData> _upgradeDataMap;
    // Dicionário para rastrear abelhas ativas por tipo (para atualização de velocidade)
    private Dictionary<string, List<BeeStatsUpdater>> _abelhasAtivasPorTipo;


    private void InitializeBeeTrackers()
    {
        _upgradeDataMap = new Dictionary<string, BeeUpgradeData>();
        _abelhasAtivasPorTipo = new Dictionary<string, List<BeeStatsUpdater>>();

        foreach (var data in todosTiposUpgradeData)
        {
            if (data != null && !string.IsNullOrEmpty(data.beeTypeName))
            {
                if (!_upgradeDataMap.ContainsKey(data.beeTypeName))
                {
                    _upgradeDataMap.Add(data.beeTypeName, data);
                    // Inicializa a lista de rastreamento para este tipo
                    _abelhasAtivasPorTipo.Add(data.beeTypeName, new List<BeeStatsUpdater>());
                     Debug.Log($"Registrado tipo de abelha para upgrades: {data.beeTypeName}");
                }
                else
                {
                     Debug.LogWarning($"Tipo de abelha duplicado encontrado nos dados de upgrade: {data.beeTypeName}");
                }
            } else {
                 Debug.LogError("Um BeeUpgradeData na lista está nulo ou sem beeTypeName!");
            }
        }
    }

    // --- Funções Públicas para obter dados e comprar upgrades ---

    // Encontra os dados do tipo de abelha especificado
    private BeeUpgradeData GetDataForType(string beeType)
    {
        BeeUpgradeData data;
        _upgradeDataMap.TryGetValue(beeType, out data);
         if(data == null) {
             Debug.LogError($"Dados de upgrade não encontrados para o tipo: {beeType}. Verifique o nome e se o Asset foi adicionado ao GerenciadorUpgrades.");
         }
        return data;
    }

    public float GetMultiplier(string beeType, TipoUpgrade tipoUpgrade)
    {
        BeeUpgradeData data = GetDataForType(beeType);
        return (data != null) ? data.GetMultiplier(tipoUpgrade) : 1f; // Retorna 1x se não encontrar
    }

    public float GetCost(string beeType, TipoUpgrade tipoUpgrade)
    {
        BeeUpgradeData data = GetDataForType(beeType);
        return (data != null) ? data.GetCost(tipoUpgrade) : float.MaxValue; // Retorna custo infinito se não encontrar
    }

     public int GetLevel(string beeType, TipoUpgrade tipoUpgrade)
    {
        BeeUpgradeData data = GetDataForType(beeType);
        if (data == null) return 0;

        switch(tipoUpgrade)
        {
            case TipoUpgrade.NectarColetado: return data.nivelNectarColetado;
            case TipoUpgrade.MelProduzido: return data.nivelMelProduzido;
            case TipoUpgrade.VelocidadeMovimento: return data.nivelVelocidade;
            default: return 0;
        }
    }

    public bool TentarComprarUpgrade(string beeType, TipoUpgrade tipoUpgrade)
    {
        BeeUpgradeData data = GetDataForType(beeType);
        if (data == null) return false; // Tipo não encontrado

        float custo = data.GetCost(tipoUpgrade);

        // Usar Mel para pagar (pode ser configurável se necessário)
        if (GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Mel, custo))
        {
            data.IncrementLevel(tipoUpgrade);
            Debug.Log($"Upgrade {tipoUpgrade} comprado para {beeType}! Novo Nível: {GetLevel(beeType, tipoUpgrade)}");

            // Se for upgrade de velocidade, notifica as abelhas *daquele tipo*
            if (tipoUpgrade == TipoUpgrade.VelocidadeMovimento)
            {
                NotificarAtualizacaoVelocidade(beeType);
            }

            // TODO: Atualizar a UI específica para este tipo/upgrade
            return true;
        }
        else
        {
            Debug.Log($"Recursos insuficientes para comprar {tipoUpgrade} para {beeType}. Custo: {custo:F1}"); // Formata custo aqui
            return false;
        }
    }

    // --- Funções de Registro/Desregistro de Abelhas (por tipo) ---

    public void RegistrarAbelha(BeeStatsUpdater abelha, string beeType)
    {
        if (_abelhasAtivasPorTipo.ContainsKey(beeType))
        {
            if (!_abelhasAtivasPorTipo[beeType].Contains(abelha))
            {
                _abelhasAtivasPorTipo[beeType].Add(abelha);
                 // Atualiza a velocidade da abelha imediatamente ao registrar
                 float currentMultiplier = GetMultiplier(beeType, TipoUpgrade.VelocidadeMovimento);
                 abelha.AtualizarVelocidade(currentMultiplier);
            }
        } else {
             Debug.LogWarning($"Tentativa de registrar abelha de tipo não gerenciado: {beeType}");
        }
    }

    public void DesregistrarAbelha(BeeStatsUpdater abelha, string beeType)
    {
         if (_abelhasAtivasPorTipo.ContainsKey(beeType))
        {
             if (_abelhasAtivasPorTipo[beeType].Contains(abelha))
             {
                _abelhasAtivasPorTipo[beeType].Remove(abelha);
             }
        }
    }

    // --- Notificação de Velocidade (por tipo) ---
    private void NotificarAtualizacaoVelocidade(string beeType)
    {
        if (_abelhasAtivasPorTipo.ContainsKey(beeType))
        {
            float novoMultiplicador = GetMultiplier(beeType, TipoUpgrade.VelocidadeMovimento);
            List<BeeStatsUpdater> abelhasDoTipo = _abelhasAtivasPorTipo[beeType];

             Debug.Log($"Notificando {abelhasDoTipo.Count} abelhas do tipo {beeType} sobre atualização de velocidade (Multiplicador: {novoMultiplicador:F2}).");

            // Itera de trás para frente para segurança ao remover (embora não removamos aqui)
            for (int i = abelhasDoTipo.Count - 1; i >= 0; i--)
            {
                if (abelhasDoTipo[i] != null) // Verifica se a abelha ainda existe
                {
                    abelhasDoTipo[i].AtualizarVelocidade(novoMultiplicador);
                }
                else
                {
                     // Remove referências nulas se necessário (melhor fazer no Desregistrar)
                     // abelhasDoTipo.RemoveAt(i); // Cuidado ao modificar lista durante iteração
                }
            }
        }
    }

     // --- Funções para UI (Exemplos) ---
    // Você precisará criar botões na UI que chamem estas funções, passando o tipo de abelha correto
    public void ComprarUpgradeNectarUI(string beeType) {
        TentarComprarUpgrade(beeType, TipoUpgrade.NectarColetado);
    }
     public void ComprarUpgradeProducaoUI(string beeType) {
        TentarComprarUpgrade(beeType, TipoUpgrade.MelProduzido);
    }
    public void ComprarUpgradeVelocidadeUI(string beeType) {
        TentarComprarUpgrade(beeType, TipoUpgrade.VelocidadeMovimento);
    }
}

// A interface continua a mesma
public interface BeeStatsUpdater
{
    void AtualizarVelocidade(float multiplicador);
}