using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Para List

public class UpgradeCategoryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI categoryNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private List<Image> starImages; // Arraste as imagens das estrelas (vazias) aqui

    private BeeData currentBeeData;
    private UpgradeCategory currentCategoryData;
    private ShopManager shopManager;
    private int currentLevel;
    private int maxLevel;

    public void Setup(BeeData beeData, UpgradeCategory categoryData, ShopManager manager)
    {
        currentBeeData = beeData;
        currentCategoryData = categoryData;
        shopManager = manager;
        maxLevel = categoryData.maxLevel;

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (currentBeeData == null || currentCategoryData == null || shopManager == null || PlayerData.Instance == null) return;

        currentLevel = PlayerData.Instance.GetUpgradeLevel(currentBeeData, currentCategoryData.categoryName);

        categoryNameText.text = currentCategoryData.categoryName;
        levelText.text = $"Nível {currentLevel}/{maxLevel}";

        // Atualiza as estrelas
        for (int i = 0; i < starImages.Count; i++)
        {
            if (i < maxLevel) // Só mostra estrelas até o nível máximo
            {
                starImages[i].gameObject.SetActive(true);
                // Muda a cor/sprite se a estrela representa um nível alcançado
                starImages[i].color = (i < currentLevel) ? Color.yellow : Color.gray; // Exemplo simples de cor
            }
            else
            {
                starImages[i].gameObject.SetActive(false); // Esconde estrelas extras
            }
        }


        // --- Lógica do Botão de Upgrade ---
        if (currentLevel >= maxLevel)
        {
            // Nível Máximo
            upgradeButton.interactable = false;
            upgradeCostText.text = "Máximo";
        }
        else
        {
            int cost = currentCategoryData.GetCostForLevel(currentLevel + 1);
            bool isAffordable = PlayerData.Instance.HasEnoughHoney(cost);

            upgradeButton.interactable = isAffordable;
            upgradeCostText.text = $"Evoluir (🍯 {cost})";
        }
    }

    private void OnUpgradeButtonClicked()
    {
        shopManager.TryUpgradeBee(currentBeeData, currentCategoryData.categoryName);
    }
}