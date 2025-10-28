// Scripts/UI/ConfirmationPopup.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Para usar Action

public class ConfirmationPopup : MonoBehaviour
{
    public static ConfirmationPopup Instancia { get; private set; }

    [Header("UI Components")]
    public GameObject popupPanel;
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;

    private Action _onConfirmAction; // A ação a ser executada se confirmar

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        popupPanel.SetActive(false); // Garante que comece desativado
    }

    /// <summary>
    /// Mostra o pop-up de confirmação.
    /// </summary>
    /// <param name="message">A mensagem a ser exibida.</param>
    /// <param name="onConfirm">A ação a ser executada se o jogador clicar em Confirmar.</param>
    public void Show(string message, Action onConfirm)
    {
        messageText.text = message;
        _onConfirmAction = onConfirm;

        // Configura os botões
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirm);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(OnCancel);

        popupPanel.SetActive(true);
        Time.timeScale = 0f; // Pausa o jogo
    }

    private void OnConfirm()
    {
        popupPanel.SetActive(false);
        Time.timeScale = 1f; // Despausa
        _onConfirmAction?.Invoke(); // Executa a ação
    }

    private void OnCancel()
    {
        popupPanel.SetActive(false);
        Time.timeScale = 1f; // Despausa
        _onConfirmAction = null; // Limpa a ação
    }
}