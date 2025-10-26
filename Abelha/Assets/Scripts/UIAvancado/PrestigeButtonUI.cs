// Scripts/UI/PrestigeButtonUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrestigeButtonUI : MonoBehaviour
{
    public Button prestigeButton;
    public TextMeshProUGUI buttonText; // Texto para mostrar a recompensa

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
        }
    }

    void OnPrestigeClicked()
    {
        // Mostra uma confirmação antes de resetar (MUITO RECOMENDADO)
        // Aqui, apenas chamamos diretamente para simplificar
        Debug.Log("Botão Ascender clicado!");
        PrestigeManager.Instancia.PerformPrestige();
    }
}