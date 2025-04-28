using UnityEngine;
using UnityEngine.UI; // Para Button
using System.Collections.Generic; // Para List
using TMPro; // Para TextMeshProUGUI

public class ShopManager : MonoBehaviour
{
    [Header("Referências Gerais UI")]
    [SerializeField] private GameObject shopPanel; // O painel principal da loja
    [SerializeField] private Button buySectionButton;
    [SerializeField] private Button evolveSectionButton;
    [SerializeField] private GameObject buySectionPanel; // Painel da seção de compra
    [SerializeField] private GameObject evolveSectionPanel; // Painel da seção de evolução
    [SerializeField] private TextMeshProUGUI honeyText; // Onde mostrar o mel do jogador

    [Header("Conteúdo das Seções")]
    [SerializeField] private Transform buyContentParent; // Onde instanciar itens de compra (dentro do ScrollView)
    [SerializeField] private Transform evolveContentParent; // Onde instanciar itens de evolução (dentro do ScrollView)

    [Header("Prefabs e Dados")]
    [SerializeField] private GameObject beeShopItemPrefab; // Prefab do item de compra
    [SerializeField] private GameObject beeEvolutionItemPrefab; // Prefab do item de evolução
    [SerializeField] private List<BeeData> allBeeTypes; // Arraste TODOS os ScriptableObjects BeeData aqui

    // Listas para guardar referências aos itens instanciados (para poder atualizá-los)
    private List<BeeShopItemUI> instantiatedShopItems = new List<BeeShopItemUI>();
    private List<BeeEvolutionItemUI> instantiatedEvolutionItems = new List<BeeEvolutionItemUI>();


    void Start()
    {
        // Garante que os painéis começam no estado correto (loja fechada)
        shopPanel.SetActive(false);
        buySectionPanel.SetActive(true); // Deixa a seção de compra ativa por padrão DENTRO da loja
        evolveSectionPanel.SetActive(false);

        // Adiciona listeners aos botões de navegação
        buySectionButton.onClick.AddListener(ShowBuySection);
        evolveSectionButton.onClick.AddListener(ShowEvolveSection);

         // Se inscrever no evento do PlayerData para atualizar a UI quando os dados mudarem
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.OnPlayerDataUpdated += UpdateShopUI;
        }
        else
        {
            Debug.LogError("Instância de PlayerData não encontrada!");
        }
    }

     void OnDestroy()
    {
         // Boa prática: remover o listener quando o objeto for destruído
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.OnPlayerDataUpdated -= UpdateShopUI;
        }
    }

    public void ToggleShopPanel() // Chame isso de um botão "Loja" no seu jogo
    {
        bool isActive = !shopPanel.activeSelf;
        shopPanel.SetActive(isActive);

        if (isActive)
        {
            // Ao abrir, atualiza tudo e mostra a seção padrão (compra)
            UpdateShopUI();
            ShowBuySection(); // Garante que a aba correta está visível
        }
    }

    private void ShowBuySection()
    {
        buySectionPanel.SetActive(true);
        evolveSectionPanel.SetActive(false);
        // Poderia adicionar feedback visual aos botões (mudar cor, etc.)
        buySectionButton.interactable = false; // Exemplo simples: desabilita o botão ativo
        evolveSectionButton.interactable = true;
        PopulateBuyList(); // Atualiza a lista ao trocar para esta seção
    }

    private void ShowEvolveSection()
    {
        buySectionPanel.SetActive(false);
        evolveSectionPanel.SetActive(true);
        buySectionButton.interactable = true;
        evolveSectionButton.interactable = false;
        PopulateEvolveList(); // Atualiza a lista ao trocar para esta seção
    }

    // Atualiza toda a UI da loja (chamado ao abrir ou quando PlayerData muda)
    private void UpdateShopUI()
    {
         if (!shopPanel.activeSelf) return; // Não faz nada se a loja estiver fechada

        // Atualiza o texto do mel
        if (PlayerData.Instance != null)
        {
            honeyText.text = $"Mel: 🍯 {PlayerData.Instance.GetHoney()}";
        }

        // Atualiza os itens existentes nas listas
        foreach(var item in instantiatedShopItems) item.UpdateUI();
        foreach(var item in instantiatedEvolutionItems) item.UpdateUI();

        // Opcional: Se a estrutura das listas pode mudar (novas abelhas desbloqueadas),
        // talvez seja melhor repopular em vez de só atualizar.
         if (buySectionPanel.activeSelf) PopulateBuyList();
         if (evolveSectionPanel.activeSelf) PopulateEvolveList();
    }


    // Preenche a lista de compra
    private void PopulateBuyList()
    {
        // 1. Limpa itens antigos
        foreach (Transform child in buyContentParent)
        {
            Destroy(child.gameObject);
        }
        instantiatedShopItems.Clear();

        // 2. Cria itens novos para cada tipo de abelha
        foreach (BeeData beeData in allBeeTypes)
        {
            // Adicionar lógica aqui para mostrar apenas abelhas desbloqueadas, se necessário
            // if (IsBeeUnlocked(beeData)) { ... }

            GameObject itemInstance = Instantiate(beeShopItemPrefab, buyContentParent);
            BeeShopItemUI itemUI = itemInstance.GetComponent<BeeShopItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(beeData, this); // Passa os dados e a referência do manager
                instantiatedShopItems.Add(itemUI); // Guarda a referência
            }
        }
         // Chama UpdateUI uma vez após popular para garantir estado inicial correto dos botões
        UpdateShopUI();
    }

    // Preenche a lista de evolução
    private void PopulateEvolveList()
    {
        // 1. Limpa itens antigos
        foreach (Transform child in evolveContentParent)
        {
            Destroy(child.gameObject);
        }
        instantiatedEvolutionItems.Clear();

        // 2. Cria itens novos APENAS para abelhas que o jogador POSSUI
        if (PlayerData.Instance != null)
        {
            foreach (var kvp in PlayerData.Instance.ownedBees)
            {
                BeeData beeData = kvp.Key;
                int count = kvp.Value;

                if (count > 0) // Só mostra se o jogador tiver pelo menos uma
                {
                     // Verifica se a abelha tem upgrades definidos
                    if (beeData.upgradeCategories != null && beeData.upgradeCategories.Count > 0)
                    {
                        GameObject itemInstance = Instantiate(beeEvolutionItemPrefab, evolveContentParent);
                        BeeEvolutionItemUI itemUI = itemInstance.GetComponent<BeeEvolutionItemUI>();
                        if (itemUI != null)
                        {
                            itemUI.Setup(beeData, this);
                            instantiatedEvolutionItems.Add(itemUI);
                        }
                    }
                }
            }
        }
        // Chama UpdateUI uma vez após popular
        UpdateShopUI();
    }

    // --- Funções de Lógica da Loja (Chamadas pelos botões dos itens) ---

    public void TryBuyBee(BeeData beeData)
    {
        if (PlayerData.Instance == null) return;

        int cost = beeData.buyCost;
        int currentCount = PlayerData.Instance.GetBeeCount(beeData);
        int maxCount = beeData.maxQuantity;

        // Verifica condições
        if (currentCount < maxCount && PlayerData.Instance.HasEnoughHoney(cost))
        {
            PlayerData.Instance.SpendHoney(cost);
            PlayerData.Instance.AddBee(beeData);
            Debug.Log($"Comprou {beeData.beeName}!");
            // A UI será atualizada automaticamente pelo evento OnPlayerDataUpdated
            // UpdateShopUI(); // Ou chama manualmente se não usar eventos
        }
        else
        {
             Debug.Log($"Não foi possível comprar {beeData.beeName}. Máximo atingido ou mel insuficiente.");
             // Adicionar feedback visual/sonoro para o jogador aqui
        }
    }

    public void TryUpgradeBee(BeeData beeData, string categoryName)
    {
        if (PlayerData.Instance == null) return;

        UpgradeCategory category = beeData.GetUpgradeCategory(categoryName);
        if (category == null)
        {
            Debug.LogError($"Categoria de upgrade '{categoryName}' não encontrada para {beeData.beeName}");
            return;
        }

        int currentLevel = PlayerData.Instance.GetUpgradeLevel(beeData, categoryName);
        int maxLevel = category.maxLevel;

        if (currentLevel < maxLevel)
        {
            int cost = category.GetCostForLevel(currentLevel + 1);
            if (PlayerData.Instance.HasEnoughHoney(cost))
            {
                PlayerData.Instance.SpendHoney(cost);
                PlayerData.Instance.SetUpgradeLevel(beeData, categoryName, currentLevel + 1);
                Debug.Log($"Evoluiu {categoryName} de {beeData.beeName} para nível {currentLevel + 1}!");
                // A UI será atualizada automaticamente
                // UpdateShopUI(); // Ou chama manualmente
            }
            else
            {
                Debug.Log($"Mel insuficiente para evoluir {categoryName} de {beeData.beeName}.");
                // Feedback para o jogador
            }
        }
        else
        {
             Debug.Log($"{categoryName} de {beeData.beeName} já está no nível máximo.");
             // Feedback para o jogador
        }
    }
}