// Scripts/UI/CombatResultsUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatResultsUI : MonoBehaviour
{
    [Header("Referências do Painel")]
    public GameObject resultsPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button continueButton;

    [Header("Seção de Recompensas")]
    public GameObject rewardsSection; // O objeto pai de todos os textos de recompensa
    public TextMeshProUGUI rewardsText;

    [Header("Cores para Feedback")]
    public Color victoryColor = Color.yellow;
    public Color defeatColor = Color.gray;

    void Start()
    {
        // Garante que o painel comece desativado
        resultsPanel.SetActive(false);

        // Adiciona o listener ao botão
        continueButton.onClick.AddListener(OnContinueButtonPressed);
    }

    /// <summary>
    /// Mostra o painel de resultados com as informações corretas.
    /// Chamado pelo CombatManager.
    /// </summary>
    public void ShowResults(bool playerWon, EnemyWaveSO completedWave)
    {
        resultsPanel.SetActive(true);

        if (playerWon)
        {
            // --- Configurações de Vitória ---
            titleText.text = "VITÓRIA!";
            titleText.color = victoryColor;
            messageText.text = "Você defendeu a colmeia com sucesso!";
            
            // Mostra e preenche a seção de recompensas
            rewardsSection.SetActive(true);
            if (completedWave != null)
            {
                rewardsText.text = $"- {completedWave.honeyReward} Mel";
                // Adicione outras recompensas aqui se houver
            }
            else
            {
                rewardsText.text = "Nenhuma recompensa especificada.";
            }
        }
        else
        {
            // --- Configurações de Derrota ---
            titleText.text = "DERROTA...";
            titleText.color = defeatColor;
            messageText.text = "Os invasores foram fortes demais desta vez. Prepare-se melhor para a próxima!";
            
            // Esconde a seção de recompensas
            rewardsSection.SetActive(false);
        }
    }

    /// <summary>
    /// Chamado quando o botão "Continuar" é pressionado.
    /// </summary>
    private void OnContinueButtonPressed()
    {
        resultsPanel.SetActive(false);
        
        // Notifica o CombatManager para finalizar a sequência de combate (limpar a cena, trocar câmera, etc.)
        if (CombatManager.Instancia != null)
        {
            CombatManager.Instancia.ConcludeCombatAfterResults();
        }
    }
}