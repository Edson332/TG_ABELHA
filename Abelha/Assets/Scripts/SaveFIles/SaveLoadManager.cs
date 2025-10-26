// Scripts/GameSystems/SaveLoadManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instancia { get; private set; }

    [Tooltip("Salva o jogo automaticamente ao fechar a aplicação.")]
    public bool saveOnQuit = true;
    
    // Não precisamos mais das variáveis privadas _recursos, _beeManager, etc.
    // Vamos buscar as instâncias diretamente quando precisarmos delas.

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Apenas chama o Load no Start
        LoadGameData();
    }

    private void OnApplicationQuit()
    {
       if (saveOnQuit) SaveGameData();
    }

public void SaveGameData()
    {
        // Busca as instâncias MAIS ATUAIS dos gerentes
        GerenciadorRecursos recursos = GerenciadorRecursos.Instancia;
        BeeManager beeManager = BeeManager.Instancia;
        GerenciadorUpgrades upgrades = GerenciadorUpgrades.Instancia;
        TutorialManager tutorialManager = TutorialManager.Instancia;
        AchievementManager achievementManager = AchievementManager.Instancia;
        RoyalJellyShopManager royalJellyShopManager = RoyalJellyShopManager.Instancia;

        // --- VERIFICAÇÃO DETALHADA ---
        bool errorFound = false;
        if (recursos == null) { Debug.LogError("SALVAR ERRO: GerenciadorRecursos.Instancia é null!"); errorFound = true; }
        if (beeManager == null) { Debug.LogError("SALVAR ERRO: BeeManager.Instancia é null!"); errorFound = true; }
        if (upgrades == null) { Debug.LogError("SALVAR ERRO: GerenciadorUpgrades.Instancia é null!"); errorFound = true; }
        if (tutorialManager == null) { Debug.LogError("SALVAR ERRO: TutorialManager.Instancia é null!"); errorFound = true; }
        if (achievementManager == null) { Debug.LogError("SALVAR ERRO: AchievementManager.Instancia é null!"); errorFound = true; }
        if (royalJellyShopManager == null) { Debug.LogError("SALVAR ERRO: RoyalJellyShopManager.Instancia é null!"); errorFound = true; }

        if (errorFound)
        {
            Debug.LogError("SALVAR: Um ou mais gerentes não foram encontrados (Instancia == null). Save abortado.");
            return; // Aborta o save se algum gerente crucial faltar
        }
        // --- FIM DA VERIFICAÇÃO DETALHADA ---

        GameData data = new GameData();
        Debug.Log("Coletando dados para salvar...");

        // 1. Recursos
        foreach(TipoRecurso tipo in System.Enum.GetValues(typeof(TipoRecurso)))
        {
            if (tipo == TipoRecurso.MelProcessado) continue;
            data.resourceAmounts.Add(new ResourceData { type = tipo, amount = recursos.ObterRecurso(tipo) });
        }
        // 2. Contagem de Abelhas
        foreach(var beeTypeData in beeManager.beeTypes) { data.beeCounts.Add(new BeeCountData { beeType = beeTypeData.beeType, currentCount = beeTypeData.currentCount }); }
        // 3. Níveis de Upgrade de Abelhas
        foreach(var upgradeDataSO in upgrades.todosTiposUpgradeData) { data.beeUpgradeLevels.Add(new BeeUpgradeSaveData { beeTypeName = upgradeDataSO.beeTypeName, nectarLevel = upgradeDataSO.nivelNectarColetado, productionLevel = upgradeDataSO.nivelMelProduzido, speedLevel = upgradeDataSO.nivelVelocidade, combatHealthLevel = upgradeDataSO.nivelVidaCombate, combatAttackLevel = upgradeDataSO.nivelAtaqueCombate }); }
        // 4. Tutoriais Completos
        data.completedTutorialIDs = tutorialManager.GetCompletedTutorials().ToList();
        // 5. Marcos (Removido)
        // 6. Estado das Conquistas
        data.achievementStatus = achievementManager.GetAchievementsForSave();
        // 7. Níveis de Upgrades de Geleia Real
        data.royalJellyUpgradeLevels = royalJellyShopManager.GetUpgradeLevelsForSave();

        SaveSystem.SaveGame(data);
    }
    public void LoadGameData()
    {
        // Busca as instâncias MAIS ATUAIS dos gerentes
        GerenciadorRecursos recursos = GerenciadorRecursos.Instancia;
        BeeManager beeManager = BeeManager.Instancia;
        GerenciadorUpgrades upgrades = GerenciadorUpgrades.Instancia;
        TutorialManager tutorialManager = TutorialManager.Instancia;
        AchievementManager achievementManager = AchievementManager.Instancia;
        RoyalJellyShopManager royalJellyShopManager = RoyalJellyShopManager.Instancia;

        // Verifica se todos foram encontrados ANTES de prosseguir
        if (recursos == null || beeManager == null || upgrades == null || tutorialManager == null || achievementManager == null || royalJellyShopManager == null)
        {
            Debug.LogError("CARREGAR: Um ou mais gerentes não foram encontrados (Instancia == null). Load abortado. Tentando configurar novo jogo...");
            // Se não conseguiu carregar porque um gerente faltou, tenta iniciar um novo jogo
            // Isso pode acontecer se houver um erro grave na inicialização de outro script
            SetupNewGame(); 
            return;
        }

        GameData data = SaveSystem.LoadGame();

        if (data != null)
        {
            Debug.Log("Aplicando dados salvos...");
            // 1. Recursos
            foreach(var resource in data.resourceAmounts) { recursos.SetRecurso(resource.type, resource.amount); }
            // 2. Contagem de Abelhas
            foreach(var beeCount in data.beeCounts) { beeManager.SetCurrentCount(beeCount.beeType, beeCount.currentCount); }
            // 3. Níveis de Upgrade de Abelhas
            upgrades.LoadAllUpgradeLevels(data.beeUpgradeLevels);
            // 4. Tutoriais
            tutorialManager.LoadCompletedTutorials(data.completedTutorialIDs);
            // 5. Marcos (Removido)
            // 6. Estado das Conquistas
            achievementManager.LoadAchievementStatus(data.achievementStatus);
            // 7. Níveis de Upgrades de Geleia Real
            royalJellyShopManager.LoadUpgradeLevels(data.royalJellyUpgradeLevels);
            
            Debug.Log("Carregamento de dados concluído. Recriando abelhas...");
            beeManager.RespawnBeesFromSaveData();
        }
        else
        {
            SetupNewGame();
        }
    }

    private void SetupNewGame()
    {
        Debug.Log("Configurando novo jogo...");
        // Busca as instâncias novamente aqui para garantir que temos as referências
        GerenciadorRecursos recursos = GerenciadorRecursos.Instancia;
        BeeManager beeManager = BeeManager.Instancia;
        GerenciadorUpgrades upgrades = GerenciadorUpgrades.Instancia;
        TutorialManager tutorialManager = TutorialManager.Instancia;
        AchievementManager achievementManager = AchievementManager.Instancia;
        RoyalJellyShopManager royalJellyShopManager = RoyalJellyShopManager.Instancia;
        
        // Aplica resets apenas se as instâncias existirem
        recursos?.ResetRecursos();
        upgrades?.ResetAllUpgradeData();
        tutorialManager?.LoadCompletedTutorials(new List<string>());
        achievementManager?.LoadAchievementStatus(new List<AchievementSaveData>());
        royalJellyShopManager?.LoadUpgradeLevels(new List<RoyalJellyUpgradeSaveData>());
        PlayerPrefs.DeleteKey("QueenPurchased");

        if (beeManager != null)
        {
            beeManager.SetCurrentCount("WorkerBee", 1);
            beeManager.SetCurrentCount("ProducerBee", 1);
            beeManager.SetCurrentCount("GuardBee", 0);
            var queenData = beeManager.beeTypes.Find(b => b.beeType == "QueenBee");
            if (queenData != null) beeManager.SetCurrentCount("QueenBee", 0);
            
            beeManager.RespawnBeesFromSaveData();
        }
    }

    // O método AreManagersReady() não é mais estritamente necessário
    // pois verificamos os managers dentro de Save/Load agora.
    // Pode ser removido ou mantido para outras checagens se desejar.
}