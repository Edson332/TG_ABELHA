// Scripts/UI/UpgradeTooltipTrigger.cs
using UnityEngine;
using System.Text; // Usado para construir strings de forma eficiente

// Garante que este script sempre tenha um TooltipTrigger no mesmo GameObject
[RequireComponent(typeof(TooltipTrigger))] 
public class UpgradeTooltipTrigger : MonoBehaviour
{
    [Header("Configuração do Upgrade")]
    [Tooltip("O 'beeTypeName' exato da abelha que este upgrade afeta.")]
    public string targetBeeType;
    [Tooltip("O tipo de upgrade que este componente irá mostrar.")]
    public TipoUpgrade targetUpgradeType;

    private TooltipTrigger _tooltipTrigger;
    private StringBuilder _sb = new StringBuilder(); // Para construir o texto do tooltip

    void Awake()
    {
        // Pega a referência do script TooltipTrigger que está no mesmo objeto
        _tooltipTrigger = GetComponent<TooltipTrigger>();
    }

    // OnMouseEnter é um bom lugar para atualizar o texto, pois só roda quando necessário
    void OnMouseEnter()
    {
        UpdateTooltipText();
    }
    
    // Update é um fallback para garantir que o texto seja atualizado se os recursos mudarem
    // enquanto o mouse já está sobre o botão.
    void Update()
    {
        // Você pode otimizar isso no futuro com eventos se houver problemas de performance,
        // mas para uma UI de upgrades, é perfeitamente aceitável.
        UpdateTooltipText();
    }

    /// <summary>
    /// Coleta os dados mais recentes dos seus Gerenciadores e constrói o texto para o tooltip.
    /// </summary>
    private void UpdateTooltipText()
    {
        if (GerenciadorUpgrades.Instancia == null) return;

        // --- Busca os Dados ---
        int currentLevel = GerenciadorUpgrades.Instancia.GetLevel(targetBeeType, targetUpgradeType);
        float currentCost = GerenciadorUpgrades.Instancia.GetCost(targetBeeType, targetUpgradeType);
        var beeData = GerenciadorUpgrades.Instancia.todosTiposUpgradeData.Find(d => d.beeTypeName == targetBeeType);
        if (beeData == null) return;

        // --- Monta o Texto do Header ---
        string headerText = $"{GetUpgradeDisplayName(targetUpgradeType)} (Nível {currentLevel})";

        // --- Monta o Texto do Corpo (usando StringBuilder para performance) ---
        _sb.Clear();
        _sb.AppendLine($"<color=#FFD700>Custo:</color> {currentCost:F0} Mel"); // Cor dourada para o custo
        _sb.AppendLine(); // Linha em branco para espaçamento

        float bonusPerLevel = GetBonusValueFromData(beeData, targetUpgradeType);
        float currentBonus = currentLevel * bonusPerLevel * 100; // Em porcentagem
        float nextLevelBonus = (currentLevel + 1) * bonusPerLevel * 100;

        _sb.AppendLine($"Bônus Atual: <color=#32CD32>+{currentBonus:F0}%</color>"); // Cor verde para bônus
        _sb.AppendLine($"Próximo Nível: <color=#32CD32>+{nextLevelBonus:F0}%</color>");

        // --- Atualiza o Conteúdo do TooltipTrigger ---
        _tooltipTrigger.SetTooltipContent(_sb.ToString(), headerText);
    }
    
    // Funções auxiliares para obter nomes e valores
    private string GetUpgradeDisplayName(TipoUpgrade type)
    {
        switch (type)
        {
            case TipoUpgrade.NectarColetado: return "Coleta de Néctar";
            case TipoUpgrade.MelProduzido: return "Produção de Mel";
            case TipoUpgrade.VelocidadeMovimento: return "Velocidade";
            case TipoUpgrade.VidaCombate: return "Vitalidade de Combate";
            case TipoUpgrade.AtaqueCombate: return "Força de Combate";
            default: return "Upgrade Desconhecido";
        }
    }

    private float GetBonusValueFromData(BeeUpgradeData data, TipoUpgrade type)
    {
        switch(type)
        {
            case TipoUpgrade.NectarColetado: return data.bonusNectarPorNivel;
            case TipoUpgrade.MelProduzido: return data.bonusProducaoPorNivel;
            case TipoUpgrade.VelocidadeMovimento: return data.bonusVelocidadePorNivel;
            case TipoUpgrade.VidaCombate: return data.bonusVidaPorNivel;
            case TipoUpgrade.AtaqueCombate: return data.bonusAtaquePorNivel;
            default: return 0f;
        }
    }
}