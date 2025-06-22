// Scripts/UI/SquadSelectionUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using TMPro; // Descomente se usar TextMeshPro

public class SquadSelectionUI : MonoBehaviour
{
    [Header("Elementos do Painel")]
    public GameObject selectionPanel;
    public int maxSquadSize = 3;

    [Header("Lista de Abelhas Disponíveis")]
    public GameObject availableBeesContent; // O objeto com o Layout Group onde a lista será populada
    public GameObject beeSelectionEntryPrefab; // O prefab de um item da lista (com o script BeeSelectionEntryUI)

    [Header("Display do Esquadrão Selecionado")]
    public List<Image> selectedSquadSlots; // Lista de Images para mostrar os ícones do esquadrão
    public Button fightButton;
    public Text squadSizeText; // Ou TextMeshProUGUI

    private List<PlayerBeeCombatDataSO> _currentSelectedSquad = new List<PlayerBeeCombatDataSO>();
    private EnemyWaveSO _targetWave;
    private Dictionary<string, int> _beeCountInSquad = new Dictionary<string, int>();

    void Start()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
    }

    /// <summary>
    /// Mostra e popula a tela de seleção de esquadrão.
    /// </summary>
    public void ShowSelectionScreen(EnemyWaveSO waveData)
    {
        _targetWave = waveData;
        _currentSelectedSquad.Clear();
        _beeCountInSquad.Clear();
        
        PopulateAvailableBeesList();
        UpdateSelectedSquadDisplay();
        
        selectionPanel.SetActive(true);
    }

    private void PopulateAvailableBeesList()
    {
        // Limpa a lista antiga
        foreach (Transform child in availableBeesContent.transform)
        {
            Destroy(child.gameObject);
        }

        if (CombatManager.Instancia == null || BeeManager.Instancia == null)
        {
            Debug.LogError("CombatManager ou BeeManager não encontrados!");
            return;
        }

        // Popula com base nos dados de combate configurados no CombatManager
        foreach (var beeCombatData in CombatManager.Instancia.allPlayerBeeCombatData)
        {
            var beeDataFromManager = BeeManager.Instancia.beeTypes.Find(b => b.beeType == beeCombatData.beeTypeNameForUpgrades);
            if (beeDataFromManager != null && beeDataFromManager.currentCount > 0)
            {
                GameObject entryGO = Instantiate(beeSelectionEntryPrefab, availableBeesContent.transform);
                BeeSelectionEntryUI entryUI = entryGO.GetComponent<BeeSelectionEntryUI>();
                if (entryUI != null)
                {
                    entryUI.Setup(beeCombatData, beeDataFromManager.currentCount, this);
                }
            }
        }
    }

    /// <summary>
    /// Chamado pelo BeeSelectionEntryUI para adicionar uma abelha ao esquadrão.
    /// </summary>
    public void AddBeeToSquad(PlayerBeeCombatDataSO beeDataToAdd)
    {
        var beeDataFromManager = BeeManager.Instancia.beeTypes.Find(b => b.beeType == beeDataToAdd.beeTypeNameForUpgrades);
        if (beeDataFromManager == null) return;

        int currentCountInSquad = _beeCountInSquad.ContainsKey(beeDataToAdd.beeTypeNameForUpgrades) ? _beeCountInSquad[beeDataToAdd.beeTypeNameForUpgrades] : 0;

        // Verifica se ainda há espaço no esquadrão e se o jogador possui abelhas daquele tipo disponíveis
        if (_currentSelectedSquad.Count < maxSquadSize && currentCountInSquad < beeDataFromManager.currentCount)
        {
            _currentSelectedSquad.Add(beeDataToAdd);
            _beeCountInSquad[beeDataToAdd.beeTypeNameForUpgrades] = currentCountInSquad + 1;
            UpdateSelectedSquadDisplay();
        }
        else
        {
            Debug.Log("Esquadrão cheio ou não há mais abelhas desse tipo disponíveis.");
        }
    }

    public void RemoveLastBeeFromSquad()
    {
        if (_currentSelectedSquad.Count > 0)
        {
            PlayerBeeCombatDataSO beeToRemove = _currentSelectedSquad[_currentSelectedSquad.Count - 1];
            _currentSelectedSquad.RemoveAt(_currentSelectedSquad.Count - 1);
            _beeCountInSquad[beeToRemove.beeTypeNameForUpgrades]--;
            UpdateSelectedSquadDisplay();
        }
    }

    private void UpdateSelectedSquadDisplay()
    {
        // Atualiza os slots de ícones
        for (int i = 0; i < selectedSquadSlots.Count; i++)
        {
            if (i < _currentSelectedSquad.Count)
            {
                selectedSquadSlots[i].sprite = _currentSelectedSquad[i].icon;
                selectedSquadSlots[i].enabled = true;
            }
            else
            {
                selectedSquadSlots[i].sprite = null; // Ou um sprite de slot vazio
                selectedSquadSlots[i].enabled = false;
            }
        }

        // Atualiza o texto do tamanho e o botão de lutar
        if (squadSizeText != null) squadSizeText.text = $"{_currentSelectedSquad.Count} / {maxSquadSize}";
        if (fightButton != null) fightButton.interactable = _currentSelectedSquad.Count > 0;

        // Atualiza a interatividade dos botões de adicionar
        foreach (Transform child in availableBeesContent.transform)
        {
            BeeSelectionEntryUI entryUI = child.GetComponent<BeeSelectionEntryUI>();
            if (entryUI != null)
            {
                entryUI.SetInteractable(_currentSelectedSquad.Count < maxSquadSize);
            }
        }
    }

    public void OnFightButtonPressed()
    {
        if (_currentSelectedSquad.Count > 0 && CombatManager.Instancia != null)
        {
            Debug.Log("Iniciando combate...");
            selectionPanel.SetActive(false);
            CombatManager.Instancia.StartNewCombat(_targetWave, _currentSelectedSquad);
            // Não precisa notificar o InvasionScheduler aqui, pois o combate começou.
            // O scheduler já está pausado pois o CombatManager.isCombatActive será true.
        }
    }

    public void OnCancelButtonPressed()
    {
        Debug.Log("Seleção de esquadrão cancelada.");
        selectionPanel.SetActive(false);
        // Notifica o scheduler que uma decisão foi tomada e o combate foi cancelado
        if (InvasionScheduler.Instancia != null)
        {
            InvasionScheduler.Instancia.AlertDecisionMade();
        }
    }
}