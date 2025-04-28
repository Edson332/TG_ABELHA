using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Para List

public class BeeEvolutionItemUI : MonoBehaviour
{
    [SerializeField] private Image beeIconImage;
    [SerializeField] private TextMeshProUGUI beeNameAndQuantityText;
    [SerializeField] private Transform upgradeCategoriesParent; // O objeto pai onde as linhas de upgrade serão instanciadas
    [SerializeField] private GameObject upgradeCategoryPrefab; // Arraste o prefab da linha de upgrade aqui

    private BeeData currentBeeData;
    private ShopManager shopManager;
    // Lista para manter referência às UIs de categoria instanciadas
    private List<UpgradeCategoryUI> instantiatedUpgradeUIs = new List<UpgradeCategoryUI>();

    public void Setup(BeeData beeData, ShopManager manager)
    {
        currentBeeData = beeData;
        shopManager = manager;

        // Limpa upgrades antigos antes de adicionar novos
        foreach (Transform child in upgradeCategoriesParent)
        {
            Destroy(child.gameObject);
        }
        instantiatedUpgradeUIs.Clear();

        UpdateUI(); // Atualiza nome/ícone e cria as linhas de upgrade
    }

    public void UpdateUI()
    {
        if (currentBeeData == null || shopManager == null || PlayerData.Instance == null) return;

        beeIconImage.sprite = currentBeeData.beeIcon;
        int currentQuantity = PlayerData.Instance.GetBeeCount(currentBeeData);
        beeNameAndQuantityText.text = $"{currentBeeData.beeName} ({currentQuantity})";

        // Se as categorias ainda não foram criadas, cria agora
        if (instantiatedUpgradeUIs.Count == 0 && currentBeeData.upgradeCategories.Count > 0)
        {
             foreach (var categoryData in currentBeeData.upgradeCategories)
            {
                GameObject upgradeInstance = Instantiate(upgradeCategoryPrefab, upgradeCategoriesParent);
                UpgradeCategoryUI upgradeUI = upgradeInstance.GetComponent<UpgradeCategoryUI>();
                if (upgradeUI != null)
                {
                    upgradeUI.Setup(currentBeeData, categoryData, shopManager);
                    instantiatedUpgradeUIs.Add(upgradeUI); // Guarda a referência
                }
                else
                {
                    Debug.LogError("Prefab de Categoria de Upgrade não contém o script UpgradeCategoryUI!", upgradeCategoryPrefab);
                }
            }
        }
        else // Se já foram criadas, apenas atualiza cada uma
        {
            foreach(var upgradeUI in instantiatedUpgradeUIs)
            {
                upgradeUI.UpdateUI();
            }
        }
    }
}