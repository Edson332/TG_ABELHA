// Scripts/UI/RoyalJellyUpgradeTooltipTrigger.cs
using UnityEngine;
using System.Text;

[RequireComponent(typeof(TooltipTrigger))]
public class RoyalJellyUpgradeTooltipTrigger : MonoBehaviour
{
    [Header("Configuração do Upgrade")]
    [Tooltip("Arraste para cá o ScriptableObject do upgrade que este botão representa.")]
    public RoyalJellyUpgradeSO targetUpgradeSO;

    private TooltipTrigger _tooltipTrigger;
    private StringBuilder _stringBuilder = new StringBuilder();

    void Awake()
    {
        _tooltipTrigger = GetComponent<TooltipTrigger>();
        if (targetUpgradeSO == null)
        {
            Debug.LogError("Nenhum RoyalJellyUpgradeSO foi atribuído a este gatilho de tooltip!", this);
            this.enabled = false;
        }
    }

    void OnMouseEnter()
    {
        UpdateTooltipText();
    }
    
    void Update()
    {
        // Atualiza constantemente para refletir mudanças no nível ou recursos
        UpdateTooltipText();
    }

    private void UpdateTooltipText()
    {
        if (RoyalJellyShopManager.Instancia == null) return;

        // --- Busca os Dados ---
        int currentLevel = RoyalJellyShopManager.Instancia.GetUpgradeLevel(targetUpgradeSO.upgradeID);
        int maxLevel = targetUpgradeSO.GetMaxLevel();

        // --- Monta o Texto do Header ---
        string headerText = $"{targetUpgradeSO.displayName} (Nível {currentLevel}/{maxLevel})";

        // --- Monta o Texto do Corpo ---
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"<b>{targetUpgradeSO.description}</b>"); // Descrição em negrito
        _stringBuilder.AppendLine(); // Espaçamento

        if (currentLevel < maxLevel)
        {
            int nextCost = targetUpgradeSO.costPerLevel[currentLevel];
            _stringBuilder.AppendLine($"<color=#FF00FF>Custo do Próximo Nível:</color> {nextCost} Geleia Real"); // Cor magenta
            // Adicione aqui o que o próximo nível faz, se a informação estiver no SO
            // Ex: _stringBuilder.AppendLine("Efeito: +1 Favo de Mel");
        }
        else
        {
            _stringBuilder.AppendLine("<color=green>NÍVEL MÁXIMO ATINGIDO</color>");
        }

        // --- Atualiza o Conteúdo do TooltipTrigger ---
        _tooltipTrigger.SetTooltipContent(_stringBuilder.ToString(), headerText);
    }
}