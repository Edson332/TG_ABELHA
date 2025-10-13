// Scripts/UI/CombatResultsUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

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
    private StringBuilder _rewardsStringBuilder = new StringBuilder();
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
            _rewardsStringBuilder.Clear(); // Limpa o texto anterior

            if (completedWave != null)
            {
                // Adiciona a recompensa de Mel, se houver
                if (completedWave.honeyReward > 0)
                {
                    _rewardsStringBuilder.AppendLine($"- {completedWave.honeyReward} Mel");
                }

                // Adiciona a recompensa de Geleia Real, se houver
                if (completedWave.royalJellyReward > 0)
                {
                    _rewardsStringBuilder.AppendLine($"- {completedWave.royalJellyReward} Geleia Real");
                }
            }

            // Se nenhuma recompensa foi adicionada, mostra uma mensagem padrão
            if (_rewardsStringBuilder.Length == 0)
            {
                _rewardsStringBuilder.AppendLine("Nenhuma recompensa nesta batalha.");
            }

            rewardsText.text = _rewardsStringBuilder.ToString();
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

