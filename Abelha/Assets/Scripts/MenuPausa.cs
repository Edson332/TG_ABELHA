// Usings necessários
using UnityEngine;
using UnityEngine.SceneManagement; // --- ADICIONADO --- Para recarregar a cena

public class MenuPausa : MonoBehaviour
{
    public GameObject painelMenuPausa;

    private bool jogoPausado = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (jogoPausado)
            {
                VoltarAoJogo();
            }
            else
            {
                Pausar();
            }
        }
    }

    void Pausar()
    {
        painelMenuPausa.SetActive(true);
        Time.timeScale = 0f;
        jogoPausado = true;
        if (UIManager.Instancia != null) UIManager.Instancia.SetGamePaused(true);
    }

    public void VoltarAoJogo()
    {
        painelMenuPausa.SetActive(false);
        Time.timeScale = 1f; // Volta o tempo ao normal
        jogoPausado = false;
        if (UIManager.Instancia != null) UIManager.Instancia.SetGamePaused(false);
    }

    // --- MÉTODO MODIFICADO ---
    /// <summary>
    /// Salva o progresso e fecha o jogo. Chamado pelo botão "Sair".
    /// </summary>
    public void SairDoJogo()
    {
        // Garante que o tempo volte ao normal antes de sair, para evitar problemas
        Time.timeScale = 1f;

        // Tenta salvar o jogo antes de sair
        if (SaveLoadManager.Instancia != null)
        {
            SaveLoadManager.Instancia.SaveGameData();
        }

        SceneManager.LoadScene("Menu");

    
    }

    // --- NOVO MÉTODO ---
    /// <summary>
    /// Deleta o arquivo de save e reinicia o jogo. Chamado pelo botão "Deletar Save".
    /// </summary>
    public void HandleDeleteSaveAndRestart()
    {
        // Garante que o tempo volte ao normal antes de qualquer ação
        Time.timeScale = 1f;

        Debug.Log("--- INICIANDO RESET COMPLETO DO JOGO ---");

        // 1. Deleta o arquivo de save no disco
        SaveSystem.DeleteSaveFile();

        // 2. Reseta os ScriptableObjects de upgrade em memória
        if (GerenciadorUpgrades.Instancia != null)
        {
            GerenciadorUpgrades.Instancia.ResetAllUpgradeData();
        }
        else
        {
            Debug.LogWarning("GerenciadorUpgrades não encontrado para resetar os dados de upgrade em memória. Se a cena for recarregada, eles podem persistir.");
        }

        PlayerPrefs.DeleteKey("QueenPurchased");
        PlayerPrefs.DeleteKey("GuardPurchased");
        // Ou, para limpar TODOS os PlayerPrefs: PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs resetados.");
        // 3. Recarrega a cena atual para resetar todos os MonoBehaviours e começar do zero.
        Debug.Log("Recarregando a cena para finalizar o reset...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}