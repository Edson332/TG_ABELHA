using UnityEngine;
using UnityEngine.UI; // Para Button, Image
using TMPro; // Para TextMeshProUGUI

public class BeeShopItemUI : MonoBehaviour
{
    [SerializeField] private Image beeIconImage;
    [SerializeField] private TextMeshProUGUI beeNameText;
    [SerializeField] private TextMeshProUGUI beeFunctionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject lockedOverlay; // Um painel/imagem para mostrar quando bloqueado

    private BeeData currentBeeData;
    private ShopManager shopManager; // Referência ao gerenciador principal

    public void Setup(BeeData beeData, ShopManager manager)
    {
        currentBeeData = beeData;
        shopManager = manager;

        // Configura listener do botão UMA VEZ
        buyButton.onClick.RemoveAllListeners(); // Limpa listeners antigos para evitar duplicação
        buyButton.onClick.AddListener(OnBuyButtonClicked);

        UpdateUI();
    }

    // Atualiza a aparência do item com base nos dados atuais
    public void UpdateUI()
    {
        if (currentBeeData == null || shopManager == null || PlayerData.Instance == null) return; // Checagem de segurança

        beeIconImage.sprite = currentBeeData.beeIcon;
        beeNameText.text = currentBeeData.beeName;
        beeFunctionText.text = $"Função: {currentBeeData.beeFunction}";
        priceText.text = $"Preço: 🍯 {currentBeeData.buyCost}";

        int currentQuantity = PlayerData.Instance.GetBeeCount(currentBeeData);
        int maxQuantity = currentBeeData.maxQuantity;
        quantityText.text = $"Qtd: {currentQuantity} / {maxQuantity}";

        // --- Lógica de Bloqueio/Compra ---
        bool isAffordable = PlayerData.Instance.HasEnoughHoney(currentBeeData.buyCost);
        bool canBuyMore = currentQuantity < maxQuantity;
        bool isLocked = false; // Adicionar lógica de desbloqueio aqui se necessário (ex: nível do jogador)

        // Atualiza o overlay de bloqueio
        lockedOverlay?.SetActive(isLocked); // '?' previne erro se não houver overlay

        // Habilita/Desabilita botão de compra
        buyButton.interactable = !isLocked && isAffordable && canBuyMore;

        // Muda o texto do botão se não puder comprar mais
        if (!canBuyMore)
        {
            // Poderia alterar o texto do botão para "Máximo"
            // buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Máximo";
        }
         else if (!isAffordable)
        {
             // Poderia indicar que falta mel, talvez mudando a cor do preço
        }
    }

    private void OnBuyButtonClicked()
    {
        // Chama a função de compra no ShopManager
        shopManager.TryBuyBee(currentBeeData);
    }
}