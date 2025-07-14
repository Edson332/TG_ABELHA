// Scripts/GameSystems/SaveLoadManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Essencial para usar .ToList() em um HashSet

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instancia { get; private set; }

    [Tooltip("Salva o jogo automaticamente ao fechar a aplicação.")]
    public bool saveOnQuit = true;

    // Referências para os gerentes
    private GerenciadorRecursos _recursos;
    private BeeManager _beeManager;
    private GerenciadorUpgrades _upgrades;
    private TutorialManager _tutorialManager;


    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Cache das instâncias dos gerentes
        _recursos = GerenciadorRecursos.Instancia;
        _beeManager = BeeManager.Instancia;
        _upgrades = GerenciadorUpgrades.Instancia;
        _tutorialManager = TutorialManager.Instancia;

        LoadGameData();
    }

    private void OnApplicationQuit()
    {
        if (saveOnQuit)
        {
            SaveGameData();
        }
    }

    public void SaveGameData()
    {
        if (!AreManagersReady()) {
            Debug.LogError("Um ou mais gerentes não estão prontos. Save abortado.");
            return;
        }

        GameData data = new GameData();

        // 1. Coletar Recursos (exceto Mel Processado)
        foreach(TipoRecurso tipo in System.Enum.GetValues(typeof(TipoRecurso)))
        {
            if (tipo == TipoRecurso.MelProcessado) continue;
            data.resourceAmounts.Add(new ResourceData { type = tipo, amount = _recursos.ObterRecurso(tipo) });
        }

        // 2. Coletar Contagem de Abelhas
        foreach(var beeTypeData in _beeManager.beeTypes)
        {
            data.beeCounts.Add(new BeeCountData { beeType = beeTypeData.beeType, currentCount = beeTypeData.currentCount });
        }

        // 3. Coletar Níveis de Upgrade
        foreach(var upgradeDataSO in _upgrades.todosTiposUpgradeData)
        {
            data.beeUpgradeLevels.Add(new BeeUpgradeSaveData
            {
                beeTypeName = upgradeDataSO.beeTypeName,
                nectarLevel = upgradeDataSO.nivelNectarColetado,
                productionLevel = upgradeDataSO.nivelMelProduzido,
                speedLevel = upgradeDataSO.nivelVelocidade,
                combatHealthLevel = upgradeDataSO.nivelVidaCombate,   
                combatAttackLevel = upgradeDataSO.nivelAtaqueCombate 
            });
        }

        // --- 4. COLETAR TUTORIAIS COMPLETOS ---
        if (_tutorialManager != null)
        {
            // Converte o HashSet para uma Lista para poder salvar em JSON
            data.completedTutorialIDs = _tutorialManager.GetCompletedTutorials().ToList(); 
        }

        // Salvar os dados coletados
        SaveSystem.SaveGame(data);
    }

    public void LoadGameData()
    {
        if (!AreManagersReady()) {
            Debug.LogError("Um ou mais gerentes não estão prontos. Load abortado.");
            return;
        }

        GameData data = SaveSystem.LoadGame();

        if (data != null)
        {
            // Jogo salvo encontrado, aplicando dados...
            Debug.Log("Aplicando dados salvos aos sistemas do jogo...");

            foreach(var resource in data.resourceAmounts) { _recursos.SetRecurso(resource.type, resource.amount); }
            foreach(var beeCount in data.beeCounts) { _beeManager.SetCurrentCount(beeCount.beeType, beeCount.currentCount); }
            if (_upgrades != null) _upgrades.LoadAllUpgradeLevels(data.beeUpgradeLevels);
            
            // --- 4. APLICAR TUTORIAIS COMPLETOS ---
            if (_tutorialManager != null)
            {
                _tutorialManager.LoadCompletedTutorials(data.completedTutorialIDs);
            }
            
            Debug.Log("Carregamento de dados concluído. Recriando abelhas na cena...");
            if (_beeManager != null) _beeManager.RespawnBeesFromSaveData();
        }
        else
        {
            // Nenhum dado salvo, configura um novo jogo.
            SetupNewGame();
        }
    }

    private void SetupNewGame()
    {
        Debug.Log("Nenhum dado salvo encontrado. Configurando um novo jogo com estado inicial.");

        // Reseta os sistemas para um estado limpo
        if (_recursos != null) _recursos.ResetRecursos();
        if (_upgrades != null) _upgrades.ResetAllUpgradeData();
        if (_tutorialManager != null) _tutorialManager.LoadCompletedTutorials(new List<string>()); // Reseta para uma lista vazia
        
        // Define a contagem inicial de abelhas
        if (_beeManager != null)
        {
            _beeManager.SetCurrentCount("WorkerBee", 1);
            _beeManager.SetCurrentCount("ProducerBee", 1);
            var queenData = _beeManager.beeTypes.Find(b => b.beeType == "QueenBee");
            if (queenData != null) _beeManager.SetCurrentCount("QueenBee", 0);
        
            // Cria as abelhas iniciais na cena
            _beeManager.RespawnBeesFromSaveData();
        }
    }

    private bool AreManagersReady()
    {
        // Verifica se os singletons essenciais foram inicializados.
        return _recursos != null && _beeManager != null && _upgrades != null && _tutorialManager != null;
    }
}