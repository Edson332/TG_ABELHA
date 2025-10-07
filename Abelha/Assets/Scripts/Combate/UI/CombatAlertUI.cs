// Scripts/UI/CombatAlertUI.cs
using UnityEngine;
using TMPro; // ADICIONADO para usar TextMeshPro

public class CombatAlertUI : MonoBehaviour
{
    [Header("Elementos do Painel de Alerta")]
    public GameObject alertPanel;
    public TextMeshProUGUI waveNameText; // MODIFICADO de Text para TextMeshProUGUI
    public TextMeshProUGUI rewardText;   // MODIFICADO de Text para TextMeshProUGUI
    // Adicione aqui referências a outros textos se precisar

    [Header("Referência para a UI de Seleção")]
    public SquadSelectionUI squadSelectionUI;

    private EnemyWaveSO _currentOfferedWave;

    void Start()
    {
        if (alertPanel != null) alertPanel.SetActive(false);
        if (squadSelectionUI == null) Debug.LogError("SquadSelectionUI não atribuído ao CombatAlertUI!");
    }

    public void ShowAlert(EnemyWaveSO waveData)
    {
        if (waveData == null || alertPanel == null)
        {
            Debug.LogError("Dados da onda ou painel de alerta nulos. Não é possível mostrar alerta.");
            if (InvasionScheduler.Instancia != null) InvasionScheduler.Instancia.AlertDecisionMade();
            return;
        }

        _currentOfferedWave = waveData;

        if (waveNameText != null) waveNameText.text = $"Ameaça: {waveData.waveName}";
        if (rewardText != null) rewardText.text = $"Recompensa (Vitória): {waveData.honeyReward} Mel";
        
        alertPanel.SetActive(true);
        // Time.timeScale = 0f; // Opcional: pausar o jogo
    }

    public void OnDefendButtonPressed()
    {
        if (UIManager.Instancia != null) UIManager.Instancia.HideAllManagedPanels();
        alertPanel.SetActive(false);
        // Time.timeScale = 1f; // Opcional: despausar o jogo

        if (squadSelectionUI != null && _currentOfferedWave != null)
        {
            squadSelectionUI.ShowSelectionScreen(_currentOfferedWave);
        }
        else
        {
            Debug.LogError("SquadSelectionUI ou _currentOfferedWave nulos ao tentar defender.");
        }
    }

    public void OnIgnoreButtonPressed()
    {
        alertPanel.SetActive(false);
        // Time.timeScale = 1f; // Opcional: despausar o jogo
        Debug.Log("Invasão ignorada.");

        if (InvasionScheduler.Instancia != null)
        {
            InvasionScheduler.Instancia.AlertDecisionMade();
        }
    }
}