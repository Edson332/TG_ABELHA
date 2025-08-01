// Scripts/UI/BuyBeeTooltipTrigger.cs
using UnityEngine;
using System.Text;

[RequireComponent(typeof(TooltipTrigger))] 
public class BuyBeeTooltipTrigger : MonoBehaviour
{
    [Header("Configuração da Abelha")]
    [Tooltip("O 'beeType' exato da abelha que este botão compra (ex: WorkerBee, EuropaBee).")]
    public string targetBeeType;

    private TooltipTrigger _tooltipTrigger;
    private StringBuilder _stringBuilder = new StringBuilder();

    void Awake()
    {
        _tooltipTrigger = GetComponent<TooltipTrigger>();
        if (_tooltipTrigger == null)
        {
            Debug.LogError($"O componente TooltipTrigger não foi encontrado no objeto {gameObject.name}!", this);
        }
    }

    // OnMouseEnter ainda é útil para uma atualização imediata
    void OnMouseEnter()
    {
        UpdateTooltipText();
    }
    
    // --- CORREÇÃO ADICIONADA ---
    // O método Update garante que o texto do tooltip seja sempre o mais recente,
    // espelhando o comportamento do UpgradeTooltipTrigger que funciona.
    void Update()
    {
        // Atualiza o texto que será exibido pelo TooltipTrigger.
        // A lógica de delay e exibição ainda é controlada pelo TooltipTrigger.
        UpdateTooltipText();
    }
    // --- FIM DA CORREÇÃO ---

    /// <summary>
    /// Coleta os dados mais recentes do BeeManager e constrói o texto para o tooltip.
    /// </summary>
    private void UpdateTooltipText()
    {
        if (BeeManager.Instancia == null) return;

        var beeData = BeeManager.Instancia.beeTypes.Find(b => b.beeType == targetBeeType);
        if (beeData == null)
        {
            // Se os dados não forem encontrados, não faz nada para evitar erros.
            // O ideal é garantir que o targetBeeType esteja sempre correto no Inspector.
            return;
        }

        // --- Monta o Texto do Header ---
        string headerText = beeData.displayName;

        // --- Monta o Texto do Corpo ---
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"<color=#FFD700>Custo:</color> {beeData.cost} Mel");
        _stringBuilder.AppendLine($"Possui: {beeData.currentCount} / {beeData.maxCount}");

        if (beeData.incomePerSecond > 0)
        {
            _stringBuilder.AppendLine(); // Espaçamento
            _stringBuilder.AppendLine($"Gera: <color=#32CD32>{beeData.incomePerSecond:F1} Mel/s</color>");
        }

        // --- Atualiza o Conteúdo do TooltipTrigger ---
        _tooltipTrigger.SetTooltipContent(_stringBuilder.ToString(), headerText);
    }
}