// Scripts/Managers/RoyalJellyShopManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class RoyalJellyShopManager : MonoBehaviour
{
    public static RoyalJellyShopManager Instancia { get; private set; }

    [Header("Configuração")]
    [Tooltip("Lista de todas as perguntas possíveis para os upgrades.")]
    public List<QuizQuestionSO> allQuizQuestions;
    [Tooltip("Lista de todos os upgrades disponíveis para compra com Geleia Real.")]
    public List<RoyalJellyUpgradeSO> allRoyalJellyUpgrades;

    [Header("Referências de Upgrades Visuais")]
    public List<GameObject> honeycombVisualUpgrades;
    public List<GameObject> flowerVisualUpgrades;
    public TutorialStepSO primeiroQuizTutorial;

    // Dicionário para guardar o nível atual de cada upgrade
    private Dictionary<string, int> _upgradeLevels = new Dictionary<string, int>();

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        // TODO: Carregar os _upgradeLevels do Save
    }

    /// <summary>
    /// Chamado pela UI ao tentar comprar um upgrade. Inicia o processo do quiz.
    /// </summary>
    public void TryPurchaseUpgrade(RoyalJellyUpgradeSO upgrade)
    {
        int currentLevel = GetUpgradeLevel(upgrade.upgradeID);
        if (currentLevel >= upgrade.GetMaxLevel()) { Debug.Log("Upgrade já está no nível máximo."); return; }

        int cost = upgrade.costPerLevel[currentLevel];
        if (GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.GeleiaReal) < cost) { Debug.Log("Geleia Real insuficiente."); return; }

        if (primeiroQuizTutorial != null && 
            TutorialManager.Instancia != null &&
            !TutorialManager.Instancia.HasCompletedTutorial(primeiroQuizTutorial.tutorialID))
        {
            // Se não foi mostrado, exibe o tutorial e passa a lógica do quiz como um "callback",
            // para que o quiz só comece DEPOIS que o jogador fechar o tutorial.
            TutorialManager.Instancia.RequestTutorial(primeiroQuizTutorial, () => {
                StartTheQuiz(upgrade);
            });
        }
        else
        {
            // Se o tutorial já foi visto, inicia o quiz diretamente.
            StartTheQuiz(upgrade);
        }
    }

    private void StartTheQuiz(RoyalJellyUpgradeSO upgrade)
    {
        QuizQuestionSO randomQuestion = allQuizQuestions[Random.Range(0, allQuizQuestions.Count)];

        QuizManager.Instancia.StartQuiz(randomQuestion, (wasCorrect) => {
            FinalizePurchase(upgrade, wasCorrect);
        });
    }

    /// <summary>
    /// Este método é o CALLBACK, executado após o jogador responder ao quiz.
    /// </summary>
    private void FinalizePurchase(RoyalJellyUpgradeSO upgrade, bool correctAnswer)
    {
        int currentLevel = GetUpgradeLevel(upgrade.upgradeID);
        int cost = upgrade.costPerLevel[currentLevel];
        
        GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.GeleiaReal, cost);

        int newLevel = currentLevel + 1;
        _upgradeLevels[upgrade.upgradeID] = newLevel;
        Debug.Log($"Upgrade '{upgrade.displayName}' comprado! Novo nível: {newLevel}");

        if (correctAnswer)
        {
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.GeleiaReal, Mathf.RoundToInt(cost * 0.05f));
        }

        // Ativa o upgrade visual correspondente ao novo nível
        ActivateVisualForUpgradeLevel(upgrade, newLevel);

        // TODO: Salvar o progresso
    }

    public List<RoyalJellyUpgradeSaveData> GetUpgradeLevelsForSave()
    {
        // Converte o dicionário para a lista serializável
        return _upgradeLevels.Select(kvp => new RoyalJellyUpgradeSaveData { upgradeID = kvp.Key, level = kvp.Value }).ToList();
    }

    // --- NOVO MÉTODO PARA CARREGAR ---
    /// <summary>
    /// Carrega os níveis salvos dos upgrades de Geleia Real.
    /// </summary>
    public void LoadUpgradeLevels(List<RoyalJellyUpgradeSaveData> savedData)
    {
        _upgradeLevels = new Dictionary<string, int>(); // Limpa os níveis antigos
        if (savedData == null) return;

        // Preenche o dicionário com os dados carregados
        foreach (var savedEntry in savedData)
        {
            _upgradeLevels[savedEntry.upgradeID] = savedEntry.level;

            // --- ATIVA OS VISUAIS CORRESPONDENTES ---
            // Encontra o SO do upgrade para saber qual visual ativar
            var upgradeSO = allRoyalJellyUpgrades.Find(u => u.upgradeID == savedEntry.upgradeID);
            if(upgradeSO != null)
            {
                // Reativa os visuais com base no nível carregado
                for (int level = 1; level <= savedEntry.level; level++)
                {
                     ActivateVisualForUpgradeLevel(upgradeSO, level);
                }
            }
        }
        Debug.Log("Níveis de upgrades de Geleia Real carregados.");
    }
    
    // Método auxiliar para ativar visual (renomeado do antigo ActivateVisualForUpgrade)
     private void ActivateVisualForUpgradeLevel(RoyalJellyUpgradeSO upgrade, int levelToActivate)
    {
        int visualIndex = levelToActivate - 1; // Nível 1 ativa o índice 0

        switch (upgrade.visualEffect)
        {
            case VisualUpgradeType.Honeycomb:
                if (visualIndex >= 0 && visualIndex < honeycombVisualUpgrades.Count && honeycombVisualUpgrades[visualIndex] != null)
                {
                    honeycombVisualUpgrades[visualIndex].SetActive(true);
                }
                break;
            case VisualUpgradeType.Flower:
                 if (visualIndex >= 0 && visualIndex < flowerVisualUpgrades.Count && flowerVisualUpgrades[visualIndex] != null)
                {
                    flowerVisualUpgrades[visualIndex].SetActive(true);
                }
                break;
        }
    }
    
    // O método FinalizePurchase agora chama o método auxiliar de ativação visual


    public int GetUpgradeLevel(string upgradeID)
    {
        _upgradeLevels.TryGetValue(upgradeID, out int level);
        return level;
    }
    

     public float GetGlobalProductionBonus()
    {
        // Esta é uma implementação simples. Você pode torná-la mais robusta.
        // Encontra o upgrade de produção global na sua lista de upgrades.
        var prodUpgrade = allRoyalJellyUpgrades.Find(u => u.upgradeID == "GLOBAL_PRODUCTION_BONUS"); // Use um ID que você definiu
        if (prodUpgrade != null)
        {
            int level = GetUpgradeLevel(prodUpgrade.upgradeID);
            // Assumindo que effectValue é o bônus por nível (ex: 0.05 para 5%)
            return level * 0.05f; // <<-- Você precisará de uma forma de obter este '0.05' do SO
        }
        return 0f; // Nenhum bônus
    }
    
    // Adicione métodos similares para outros bônus globais, como os de combate
    public float GetGlobalCombatHPBonus()
    {
        var hpUpgrade = allRoyalJellyUpgrades.Find(u => u.upgradeID == "GLOBAL_COMBAT_HP_BONUS");
        if (hpUpgrade != null)
        {
            int level = GetUpgradeLevel(hpUpgrade.upgradeID);
            return level * 0.1f; // Ex: +10% de HP por nível
        }
        return 0f;
    }

    public float GetGlobalCombatAttackBonus()
    {
        var attackUpgrade = allRoyalJellyUpgrades.Find(u => u.upgradeID == "GLOBAL_COMBAT_ATTACK_BONUS");
        if (attackUpgrade != null)
        {
            int level = GetUpgradeLevel(attackUpgrade.upgradeID);
            return level * 0.1f; // Ex: +10% de Ataque por nível
        }
        return 0f;
    }
}