// Scripts/UI/BeeSelectionEntryUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro; // ADICIONADO para usar TextMeshPro

public class BeeSelectionEntryUI : MonoBehaviour
{
    [Header("Referências Visuais")]
    public Image beeIcon;
    public TextMeshProUGUI beeNameText; // MODIFICADO de Text para TextMeshProUGUI
    public TextMeshProUGUI beeCountText; // MODIFICADO de Text para TextMeshProUGUI
    public Button addButton;

    private PlayerBeeCombatDataSO _beeData;
    private SquadSelectionUI _squadSelectionUI;

    /// <summary>
    /// Configura esta entrada da lista com os dados de uma abelha e a referência da UI principal.
    /// </summary>
    public void Setup(PlayerBeeCombatDataSO data, int currentOwnedCount, SquadSelectionUI mainUI)
    {
        _beeData = data;
        _squadSelectionUI = mainUI;

        if (beeIcon != null) beeIcon.sprite = data.icon; 
        if (beeNameText != null) beeNameText.text = data.combatantName;
        if (beeCountText != null) beeCountText.text = $"Disponíveis: {currentOwnedCount}";

        // Adiciona o listener ao botão programaticamente
        if (addButton != null)
        {
            // Remove listeners antigos para evitar chamadas duplicadas ao repopular a lista
            addButton.onClick.RemoveAllListeners(); 
            addButton.onClick.AddListener(OnAddButtonPressed);
        }
    }

    private void OnAddButtonPressed()
    {
        if (_squadSelectionUI != null && _beeData != null)
        {
            // Pede para a UI principal adicionar esta abelha ao esquadrão
            _squadSelectionUI.AddBeeToSquad(_beeData);
        }
    }

    /// <summary>
    /// Atualiza a interatividade do botão Adicionar.
    /// </summary>
    public void SetInteractable(bool isInteractable)
    {
        if (addButton != null)
        {
            addButton.interactable = isInteractable;
        }
    }
}