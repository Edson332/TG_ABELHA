using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // Adicionado para usar Listas
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject upgradeMenuPanel;
    public GameObject upgradeTAB;
    public GameObject buyTAB;

    [Header("Dependencies")]
    public AchievementManager achievementManager; // Must be assigned in Inspector
    
    // Usar Listas é mais flexível que arrays, mas a lógica é a mesma
    public List<GameObject> achievementPanels = new List<GameObject>(); 
    [Header("Buttons")]
    public List<Button> menuButtons = new List<Button>();

    [Header("Notifications")]
    public GameObject achievementNotificationSymbol;

    void Start()
    {
        // Garante que o achievementManager foi atribuído
        if (achievementManager == null)
        {
            Debug.LogError("AchievementManager não foi atribuído no Inspector do MenuManager!", this);
            return;
        }

        // Inicializa todos os painéis de achievement como inativos
        foreach (GameObject panel in achievementPanels)
        {
            if(panel != null) panel.SetActive(false);
        }
        
        // Configura os listeners dos botões
        for (int i = 0; i < menuButtons.Count; i++)
        {
            int index = i; // Importante para o closure
            if(menuButtons[i] != null)
            {
                menuButtons[i].onClick.RemoveAllListeners(); // Limpa listeners antigos para segurança
                menuButtons[i].onClick.AddListener(() => ShowAchievementPanel(index));
            }
        }

        UpdateMenuButtons();
    }

    void Update()
    {

        UpdateMenuButtons();

        if (achievementManager != null && achievementNotificationSymbol != null)
        {
            // Mostra o símbolo se houver conquistas novas, esconde se não houver.
            bool shouldShowNotification = achievementManager.HasUnviewedAchievements();
            if (achievementNotificationSymbol.activeSelf != shouldShowNotification)
            {
                achievementNotificationSymbol.SetActive(shouldShowNotification);
            }
        }
    }

    /// <summary>
    /// Atualiza a aparência e a interatividade de cada botão com base na conquista correspondente.
    /// </summary>
    public void UpdateMenuButtons()
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            if (i < achievementManager.achievements.Length)
            {
                Achievement achievement = achievementManager.achievements[i];
                Button button = menuButtons[i];
                
                if (button == null) continue; // Pula se o botão não estiver na lista

                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                Image buttonImage = button.GetComponent<Image>();

                if (achievement.isUnlocked)
                {
                    button.interactable = true;
                    if(buttonText != null) buttonText.text = achievement.title;
                    if(buttonImage != null) buttonImage.color = Color.white;
                }
                else
                {
                    button.interactable = false;
                    if(buttonText != null) buttonText.text = "???";
                    if(buttonImage != null) buttonImage.color = Color.grey;
                }
            }
        }
    }

    /// <summary>
    /// Mostra o painel de conquista correspondente.
    /// </summary>
    public void ShowAchievementPanel(int panelIndex)
    {
        if (panelIndex >= 0 && panelIndex < achievementManager.achievements.Length)
        {
            if (achievementManager.achievements[panelIndex].isUnlocked)
            {

                achievementManager.achievements[panelIndex].hasBeenViewed = true;
                Debug.Log($"Conquista '{achievementManager.achievements[panelIndex].title}' marcada como vista.");
                mainMenuPanel.SetActive(false);
                // Desativa todos os painéis
                foreach (GameObject panel in achievementPanels)
                {
                    if (panel != null) panel.SetActive(false);
                }
                // Ativa o painel correspondente e atualiza a exibição
                if (panelIndex < achievementPanels.Count && achievementPanels[panelIndex] != null)
                {
                    achievementPanels[panelIndex].SetActive(true);
                    UpdateAchievementDisplay(panelIndex);
                }
            }
            else
            {
                Debug.Log("Este achievement ainda não foi alcançado!");
            }
        }
    }
    
    // ### MÉTODO CORRIGIDO ###
    /// <summary>
    /// Atualiza o conteúdo de um painel de conquista específico.
    /// </summary>
    public void UpdateAchievementDisplay(int panelIndex)
    {
        Achievement achievement = achievementManager.achievements[panelIndex];
        GameObject currentPanel = achievementPanels[panelIndex];
        
        // Em vez de transform.Find, vamos buscar os componentes em todos os filhos.
        // Isso é mais robusto e funciona dentro de Scroll Views.
        
        TextMeshProUGUI titleText = FindComponentInChild<TextMeshProUGUI>(currentPanel, "Title");
        TextMeshProUGUI descText = FindComponentInChild<TextMeshProUGUI>(currentPanel, "Description");
        Image iconImage = FindComponentInChild<Image>(currentPanel, "Icon");
        
        if (titleText != null) titleText.text = achievement.title;
        if (descText != null) descText.text = achievement.description;
        if (iconImage != null) iconImage.sprite = achievement.icon;
        
        // Alterar a cor de fundo do painel conforme o status
        Image panelImage = currentPanel.GetComponent<Image>();
        if(panelImage != null)
            panelImage.color = achievement.isUnlocked ? Color.white : Color.gray;
    }

    /// <summary>
    /// Método auxiliar genérico para encontrar um componente em um filho com um nome específico.
    /// </summary>
    private T FindComponentInChild<T>(GameObject parent, string childName) where T : Component
    {
        T[] components = parent.GetComponentsInChildren<T>(true);
        foreach (T component in components)
        {
            if (component.gameObject.name == childName)
            {
                return component;
            }
        }
        Debug.LogWarning($"Não foi possível encontrar o componente '{typeof(T).Name}' no objeto filho chamado '{childName}' dentro de '{parent.name}'");
        return null;
    }

    // --- Seus outros métodos de UI (sem alterações) ---
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        foreach (GameObject panel in achievementPanels)
        {
            if(panel != null) panel.SetActive(false);
        }
    }
    public void ShowUpgradeMenu()
    {
        if (upgradeMenuPanel != null)
            upgradeMenuPanel.SetActive(!upgradeMenuPanel.activeSelf);
    }
    public void ShowUpgradeTAB()
    {
        if (upgradeTAB != null)
        {
            upgradeTAB.SetActive(true);
            if (buyTAB != null) buyTAB.SetActive(false);
        }
    }
    public void ShowBuyTAB()
    {
        if (buyTAB != null)
        {
            buyTAB.SetActive(true);
            if (upgradeTAB != null) upgradeTAB.SetActive(false);
        }
    }
    public void BackToMainMenu()
    {
        ShowMainMenu();
    }
}