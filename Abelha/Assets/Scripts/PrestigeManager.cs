
using UnityEngine;
using UnityEngine.SceneManagement; // Para recarregar a cena
using System.Collections.Generic;

public class PrestigeManager : MonoBehaviour
{
    public static PrestigeManager Instancia { get; private set; }

    [Header("Configuração do Prestígio")]
    [Tooltip("A quantidade de Mel necessária para poder fazer o Prestígio.")]
    public double honeyRequiredForPrestige = 1000000; // 1 Milhão - Ajuste aqui para testar!
    
    [Tooltip("Fator divisor dentro da fórmula de raiz cúbica. Valores maiores = menos Geleia Real.")]
    public double formulaDivisor = 10000; // Ajuste para balanceamento

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        DontDestroyOnLoad(gameObject); // Essencial para que ele sobreviva ao recarregamento da cena
    }

    /// <summary>
    /// Verifica se o jogador tem Mel suficiente para fazer o Prestígio.
    /// </summary>
    public bool CanPrestige()
    {
        if (GerenciadorRecursos.Instancia == null) return false;
        return GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Mel) >= honeyRequiredForPrestige;
    }

    /// <summary>
    /// Calcula a quantidade de Geleia Real que o jogador ganhará se fizer Prestígio AGORA.
    /// Fórmula: RaizCúbica(MelAtual / Divisor)
    /// </summary>
    public float CalculateRoyalJellyReward()
    {
        if (GerenciadorRecursos.Instancia == null) return 0f;

        double currentHoney = GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Mel);
        if (currentHoney < honeyRequiredForPrestige) return 0f; // Não ganha nada se não atingiu o mínimo

        // Calcula a base para a raiz cúbica
        double baseValue = currentHoney / formulaDivisor;
        if (baseValue <= 0) return 0f;

        // Calcula a raiz cúbica (Math.Pow(base, 1.0/3.0))
        float reward = (float)System.Math.Pow(baseValue, 1.0 / 3.0);
        
        return Mathf.Floor(reward); // Arredonda para baixo para obter um número inteiro de Geleia
    }

    /// <summary>
    /// Executa o processo de Prestígio: calcula recompensa, reseta o progresso e adiciona a recompensa.
    /// </summary>
    public void PerformPrestige()
    {
        // 1. Verifica se pode fazer Prestígio
        if (!CanPrestige())
        {
            Debug.LogWarning("Tentativa de fazer Prestígio sem ter Mel suficiente.");
            return;
        }

        Debug.Log("--- INICIANDO PROCESSO DE PRESTÍGIO ---");

        // 2. Calcula e armazena a recompensa
        float jellyReward = CalculateRoyalJellyReward();
        Debug.Log($"Recompensa de Prestígio calculada: {jellyReward} Geleia Real.");

        // 3. Armazena a Geleia Real atual ANTES do reset
        float currentJelly = 0f;
        if (GerenciadorRecursos.Instancia != null)
        {
            currentJelly = GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.GeleiaReal);
        }

        // --- 4. EXECUTA O "SOFT RESET" ---
        // Reseta Recursos (exceto Geleia Real, que faremos manualmente)
        if (GerenciadorRecursos.Instancia != null) GerenciadorRecursos.Instancia.ResetRecursos();
        
        // Reseta Upgrades Normais (os Scriptable Objects)
        if (GerenciadorUpgrades.Instancia != null) GerenciadorUpgrades.Instancia.ResetAllUpgradeData();
        
        // Reseta Contagem de Abelhas (exceto as iniciais)
        if (BeeManager.Instancia != null)
        {
             // Zera a contagem de todas as abelhas
             foreach (var beeData in BeeManager.Instancia.beeTypes)
             {
                 BeeManager.Instancia.SetCurrentCount(beeData.beeType, 0);
             }
             // Poderia definir as iniciais aqui, mas o SetupNewGame já faz isso
        }

        // Reseta Tutoriais (opcional, talvez alguns devam persistir?)
        if (TutorialManager.Instancia != null) TutorialManager.Instancia.LoadCompletedTutorials(new List<string>());
        
        // Reseta Conquistas? (decisão de design - geralmente conquistas persistem)
        // if (AchievementManager.Instancia != null) AchievementManager.Instancia.LoadAchievementStatus(new List<AchievementSaveData>());
        
        // Reseta Upgrades de Geleia Real? (decisão de design - geralmente NÃO são resetados)
        // if (RoyalJellyShopManager.Instancia != null) RoyalJellyShopManager.Instancia.LoadUpgradeLevels(new List<RoyalJellyUpgradeSaveData>());

        // Reseta a flag da Rainha comprada (se estiver usando PlayerPrefs)
        PlayerPrefs.DeleteKey("QueenPurchased");

        Debug.Log("Progresso principal resetado.");
        // --- FIM DO SOFT RESET ---


        // 5. Adiciona a Recompensa de Prestígio à Geleia Real que o jogador já tinha
        if (GerenciadorRecursos.Instancia != null)
        {
            GerenciadorRecursos.Instancia.SetRecurso(TipoRecurso.GeleiaReal, currentJelly + jellyReward);
            Debug.Log($"Geleia Real final após Prestígio: {GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.GeleiaReal)}");
        }

        // 6. Força o salvamento do novo estado (importante!)
        if (SaveLoadManager.Instancia != null)
        {
            SaveLoadManager.Instancia.SaveGameData();
        }

        // 7. Recarrega a cena do jogo para aplicar o estado resetado
        Debug.Log("Recarregando cena do jogo...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Assume que a cena atual é a do jogo
    }
}