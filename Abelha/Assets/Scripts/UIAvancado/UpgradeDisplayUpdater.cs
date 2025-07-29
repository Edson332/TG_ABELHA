// Scripts/UI/UpgradeDisplayUpdater.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeDisplayUpdater : MonoBehaviour
{
    [Header("Configuração do Upgrade")]
    [Tooltip("O 'beeTypeName' exato da abelha que este upgrade afeta (ex: WorkerBee, ProducerBee).")]
    public string targetBeeType;
    [Tooltip("O tipo de upgrade que este componente irá mostrar.")]
    public TipoUpgrade targetUpgradeType;

    [Header("Referências da UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI effectText;
    public Button purchaseButton;

    private GerenciadorRecursos _recursos;
    private GerenciadorUpgrades _upgrades;

    void Start()
    {
        _recursos = GerenciadorRecursos.Instancia;
        _upgrades = GerenciadorUpgrades.Instancia;
        
        if (string.IsNullOrEmpty(targetBeeType))
        {
            Debug.LogError("Target Bee Type não foi definido neste componente de UI de upgrade!", this.gameObject);
            this.enabled = false;
        }
        UpdateDisplay();
    }

    void Update()
    {
        UpdateButtonInteractable();
    }

    public void UpdateDisplay()
    {
        if (_upgrades == null) return;

        int currentLevel = _upgrades.GetLevel(targetBeeType, targetUpgradeType);
        // ### CORREÇÃO AQUI ###
        float currentCost = _upgrades.GetCost(targetBeeType, targetUpgradeType);
        
        if (levelText != null) levelText.text = $"Nível {currentLevel}";
        if (costText != null) costText.text = $"Custo: {currentCost:F0}";

        if (effectText != null)
        {
            var beeData = _upgrades.todosTiposUpgradeData.Find(d => d.beeTypeName == targetBeeType);
            if (beeData != null)
            {
                float bonusPorNivel = GetBonusValueFromData(beeData, targetUpgradeType);
                float proximoBonus = (currentLevel + 1) * bonusPorNivel * 100;
                effectText.text = $"+{proximoBonus:F0}%";
            }
        }
        
        UpdateButtonInteractable();
    }

    private void UpdateButtonInteractable()
    {
        if (_recursos == null || _upgrades == null || purchaseButton == null) return;
        // ### CORREÇÃO AQUI ###
        float currentCost = _upgrades.GetCost(targetBeeType, targetUpgradeType);
        purchaseButton.interactable = _recursos.ObterRecurso(TipoRecurso.Mel) >= currentCost;
    }
    
    public void OnPurchase() {
        UpdateDisplay();
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