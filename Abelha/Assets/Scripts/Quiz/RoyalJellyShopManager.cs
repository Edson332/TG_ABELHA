// Scripts/Managers/RoyalJellyShopManager.cs
using System.Collections.Generic;
using UnityEngine;

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

        // Deduz o custo
        GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.GeleiaReal, cost);

        // Aumenta o nível do upgrade
        _upgradeLevels[upgrade.upgradeID] = currentLevel + 1;
        Debug.Log($"Upgrade '{upgrade.displayName}' comprado! Novo nível: {currentLevel + 1}");

        if (correctAnswer)
        {
            Debug.Log("Resposta correta! Aplicando bônus de 5%!");
            // TODO: Adicionar lógica para aplicar o bônus. Pode ser um buff temporário
            // ou um pequeno reembolso de recursos. Ex:
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.GeleiaReal, Mathf.RoundToInt(cost * 0.15f));
        }

        // Ativa o upgrade visual, se houver
        ActivateVisualForUpgrade(upgrade);

        // TODO: Salvar o progresso
    }

    private void ActivateVisualForUpgrade(RoyalJellyUpgradeSO upgrade)
    {
        int currentLevel = GetUpgradeLevel(upgrade.upgradeID); // O nível recém-adquirido
        int visualIndex = currentLevel - 1; // Nível 1 ativa o objeto no índice 0

        switch (upgrade.visualEffect)
        {
            case VisualUpgradeType.Honeycomb:
                if (visualIndex < honeycombVisualUpgrades.Count && honeycombVisualUpgrades[visualIndex] != null)
                {
                    honeycombVisualUpgrades[visualIndex].SetActive(true);
                }
                break;
            case VisualUpgradeType.Flower:
                if (visualIndex < flowerVisualUpgrades.Count && flowerVisualUpgrades[visualIndex] != null)
                {
                    flowerVisualUpgrades[visualIndex].SetActive(true);
                }
                break;
        }
    }

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