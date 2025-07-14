// Scripts/UI/SquadSelectionUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SquadSelectionUI : MonoBehaviour
{
    [Header("Elementos do Painel")]
    public GameObject selectionPanel;
    public int maxSquadSize = 3;

    [Header("Tutorial")]
    [Tooltip("Tutorial a ser exibido na primeira vez que esta tela abrir.")]
    public TutorialStepSO squadSelectionTutorial; // <<--- CAMPO ADICIONADO

    [Header("Lista de Abelhas Disponíveis")]
    public GameObject availableBeesContent;
    public GameObject beeSelectionEntryPrefab;

    [Header("Display do Esquadrão Selecionado")]
    public List<Image> selectedSquadSlots;
    public Button fightButton;
    public TextMeshProUGUI squadSizeText; // Assumindo que você já usa TextMeshPro aqui

    private List<PlayerBeeCombatDataSO> _currentSelectedSquad = new List<PlayerBeeCombatDataSO>();
    private EnemyWaveSO _targetWave;
    private Dictionary<string, int> _beeCountInSquad = new Dictionary<string, int>();

    void Start()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
    }

    /// <summary>
    /// Mostra e popula a tela de seleção de esquadrão. AGORA TAMBÉM DISPARA O TUTORIAL.
    /// </summary>
    public void ShowSelectionScreen(EnemyWaveSO waveData)
    {
        _targetWave = waveData;
        _currentSelectedSquad.Clear();
        _beeCountInSquad.Clear();

        // --- LÓGICA DO TUTORIAL (ADICIONADA AQUI) ---
        // Verifica se o tutorial deve ser mostrado antes de exibir o painel
        if (squadSelectionTutorial != null &&
            TutorialManager.Instancia != null &&
            !TutorialManager.Instancia.HasCompletedTutorial(squadSelectionTutorial.tutorialID))
        {
            // A UI de seleção só será mostrada após o tutorial ser concluído.
            // O TutorialManager pausa o jogo. Quando o jogador clica "Continuar",
            // o jogo despausa e o código abaixo continua a ser percebido.
            TutorialManager.Instancia.RequestTutorial(squadSelectionTutorial);
        }
        // --- FIM DA LÓGICA DO TUTORIAL ---
        
        PopulateAvailableBeesList();
        UpdateSelectedSquadDisplay();
        
        selectionPanel.SetActive(true);
    }

    private void PopulateAvailableBeesList()
    {
        foreach (Transform child in availableBeesContent.transform)
        {
            Destroy(child.gameObject);
        }

        if (CombatManager.Instancia == null || BeeManager.Instancia == null) return;

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
    
    // ... (O resto dos métodos AddBeeToSquad, RemoveLastBeeFromSquad, UpdateSelectedSquadDisplay,
    // OnFightButtonPressed, OnCancelButtonPressed permanecem EXATAMENTE IGUAIS) ...
    public void AddBeeToSquad(PlayerBeeCombatDataSO beeDataToAdd)
    {
        var beeDataFromManager = BeeManager.Instancia.beeTypes.Find(b => b.beeType == beeDataToAdd.beeTypeNameForUpgrades);
        if (beeDataFromManager == null) return;

        int currentCountInSquad = _beeCountInSquad.ContainsKey(beeDataToAdd.beeTypeNameForUpgrades) ? _beeCountInSquad[beeDataToAdd.beeTypeNameForUpgrades] : 0;

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
            if (_beeCountInSquad.ContainsKey(beeToRemove.beeTypeNameForUpgrades))
            {
                _beeCountInSquad[beeToRemove.beeTypeNameForUpgrades]--;
            }
            UpdateSelectedSquadDisplay();
        }
    }

    private void UpdateSelectedSquadDisplay()
    {
        for (int i = 0; i < selectedSquadSlots.Count; i++)
        {
            if (i < _currentSelectedSquad.Count)
            {
                selectedSquadSlots[i].sprite = _currentSelectedSquad[i].icon;
                selectedSquadSlots[i].enabled = true;
            }
            else
            {
                selectedSquadSlots[i].sprite = null; 
                selectedSquadSlots[i].enabled = false;
            }
        }

        if (squadSizeText != null) squadSizeText.text = $"{_currentSelectedSquad.Count} / {maxSquadSize}";
        if (fightButton != null) fightButton.interactable = _currentSelectedSquad.Count > 0;

        if (availableBeesContent == null) return;
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
        }
    }

    public void OnCancelButtonPressed()
    {
        Debug.Log("Seleção de esquadrão cancelada.");
        selectionPanel.SetActive(false);
        
        if (InvasionScheduler.Instancia != null)
        {
            InvasionScheduler.Instancia.AlertDecisionMade();
        }
    }
}