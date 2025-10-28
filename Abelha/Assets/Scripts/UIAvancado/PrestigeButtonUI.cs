// Scripts/UI/PrestigeButtonUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class PrestigeButtonUI : MonoBehaviour
{
    public Button prestigeButton;
    public TextMeshProUGUI buttonText; // Texto para mostrar a recompensa
    private TooltipTrigger _tooltipTrigger;
    private StringBuilder _sb = new StringBuilder();

    
    void Awake()
    {
        _tooltipTrigger = GetComponent<TooltipTrigger>(); // Pega o trigger do tooltip
        if (_tooltipTrigger == null) Debug.LogWarning("TooltipTrigger não encontrado neste botão.", this);
    }
    void Start()
    {
        if (prestigeButton == null) prestigeButton = GetComponent<Button>();
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();

        prestigeButton.onClick.AddListener(OnPrestigeClicked);
    }

    void Update()
    {
        // Verifica se o jogador pode fazer prestígio
        bool canPrestige = PrestigeManager.Instancia.CanPrestige();

        // Ativa/Desativa o botão
        prestigeButton.gameObject.SetActive(canPrestige);

        if (canPrestige)
        {
            // Mostra a recompensa no botão
            float reward = PrestigeManager.Instancia.CalculateRoyalJellyReward();
            buttonText.text = $"Ascender!\n(+{reward:F0} Geleia Real)";
            if (_tooltipTrigger != null)
            {
                _sb.Clear();
                _sb.AppendLine("Reseta Mel, Néctar, Abelhas e Melhorias normais.");
                _sb.AppendLine($"Ganha <color=#FF00FF>+{reward:F0} Geleia Real</color>."); // Cor magenta
                _sb.AppendLine();
                _sb.AppendLine("<b><color=yellow>Importante:</color> Melhorias compradas com Geleia Real (na aba Estrutura) são PERMANENTES e não serão resetadas!</b>");
                
                _tooltipTrigger.SetTooltipContent(_sb.ToString(), "Ascender"); // Define o texto e um header
            }
        }

    
    }

    void OnPrestigeClicked()
    {
        if (ConfirmationPopup.Instancia == null)
        {
            Debug.LogError("ConfirmationPopup não encontrado na cena!");
            return;
        }

        // Monta a mensagem de confirmação
        float reward = PrestigeManager.Instancia.CalculateRoyalJellyReward();
        string message = $"Você tem certeza que deseja Ascender?\n\n" +
                         $"Seu Mel, Néctar, Abelhas e Melhorias normais serão resetados.\n" +
                         $"<color=#556B2F>Melhorias de Estrutura (Geleia Real) serão MANTIDAS.</color>\n\n" +
                         $"Você ganhará <color=#FF00FF>+{reward:F0} Geleia Real</color>.";

        // Mostra o pop-up. A ação a ser executada na confirmação é o PerformPrestige.
        ConfirmationPopup.Instancia.Show(message, () => {
            PrestigeManager.Instancia.PerformPrestige();
        });
    }
}