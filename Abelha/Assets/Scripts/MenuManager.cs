using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

// A classe auxiliar permanece a mesma
[System.Serializable]
public class AchievementUIEntry
{
    public Button button;
    public GameObject notificationIcon;
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instancia { get; private set; } 

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject upgradeMenuPanel;
    public GameObject upgradeTAB;
    public GameObject buyTAB;
    public GameObject structureTAB;

    [Header("Dependencies")]
    public AchievementManager achievementManager;
    public List<GameObject> achievementPanels = new List<GameObject>(); 

    [Header("UI de Conquistas")]
    [Tooltip("O ícone de notificação GLOBAL que fica sobre o botão principal de conquistas.")]
    public GameObject globalAchievementNotification;
    [Tooltip("Associe cada botão de conquista e seu respectivo ícone de notificação aqui.")]
    public List<AchievementUIEntry> achievementUIEntries = new List<AchievementUIEntry>();
    
    // A lista 'menuButtons' antiga foi removida por ser redundante.

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    void Start()
    {
        if (achievementManager == null)
        {
            Debug.LogError("AchievementManager não foi atribuído no Inspector do MenuManager!", this);
            return;
        }

        // Inicializa todos os painéis de detalhe como inativos
        foreach (GameObject panel in achievementPanels)
        {
            if(panel != null) panel.SetActive(false);
        }
        
        // Configura os listeners dos botões usando a nova lista unificada
        for (int i = 0; i < achievementUIEntries.Count; i++)
        {
            int index = i; // Importante para o closure
            if(achievementUIEntries[i].button != null)
            {
                achievementUIEntries[i].button.onClick.RemoveAllListeners();
                achievementUIEntries[i].button.onClick.AddListener(() => ShowAchievementPanel(index));
            }
        }

        // Chama a atualização uma vez no início para definir o estado inicial
        UpdateAllAchievementUI();
    }

    void Update()
    {
        // O Update agora chama o método de atualização correto e completo.
        UpdateAllAchievementUI();
    }

    /// <summary>
    /// Um método central que atualiza TUDO relacionado à UI de conquistas.
    /// </summary>
    public void UpdateAllAchievementUI()
    {
        if (achievementManager == null) return;

        // --- LÓGICA DO ÍCONE GLOBAL ---
        if (globalAchievementNotification != null)
        {
            bool shouldShowGlobal = achievementManager.HasUnviewedAchievements();
            if (globalAchievementNotification.activeSelf != shouldShowGlobal)
            {
                globalAchievementNotification.SetActive(shouldShowGlobal);
            }
        }

        // --- LÓGICA DOS BOTÕES E ÍCONES ESPECÍFICOS ---
        for (int i = 0; i < achievementUIEntries.Count; i++)
        {
            if (i >= achievementManager.achievements.Length) continue;

            Achievement achievement = achievementManager.achievements[i];
            AchievementUIEntry uiEntry = achievementUIEntries[i];
            
            if (uiEntry.button == null) continue;

            TextMeshProUGUI buttonText = uiEntry.button.GetComponentInChildren<TextMeshProUGUI>();
            Image buttonImage = uiEntry.button.GetComponent<Image>();

            // Atualiza a aparência do botão (bloqueado/desbloqueado)
            if (achievement.isUnlocked)
            {
                uiEntry.button.interactable = true;
                if(buttonText != null) buttonText.text = achievement.title;
                if(buttonImage != null) buttonImage.color = Color.white;
            }
            else
            {
                uiEntry.button.interactable = false;
                if(buttonText != null) buttonText.text = "???";
                if(buttonImage != null) buttonImage.color = Color.grey;
            }

            // Atualiza a visibilidade do ícone de notificação específico
            if (uiEntry.notificationIcon != null)
            {
                bool shouldShowSpecific = achievement.isUnlocked && !achievement.hasBeenViewed;
                if (uiEntry.notificationIcon.activeSelf != shouldShowSpecific)
                {
                    uiEntry.notificationIcon.SetActive(shouldShowSpecific);
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

    
    public void ShowMainMenu()
    {
        CloseUpgradePanels();
        if (AchievementMenu.Instancia != null)
        {
            AchievementMenu.Instancia.ClosePanel();
        }

        // Ativa o HUD principal do jogo se ele estiver desativado
        if (mainMenuPanel != null && !mainMenuPanel.activeSelf)
        {
            AchievementMenu.Instancia.TogglePanel();
            mainMenuPanel.SetActive(true);
        }
    }

    public void ShowStructureTAB()
    {
        if (structureTAB != null)
        {
            structureTAB.SetActive(true);
            // Garante que as outras abas estejam fechadas
            if (upgradeTAB != null) upgradeTAB.SetActive(false);
            if (buyTAB != null) buyTAB.SetActive(false);
        }
    }
    public void ToggleUpgradeMenu()
    {
        // 1. Verifica qual deve ser o próximo estado do painel de upgrades
        bool shouldBeActive = !upgradeMenuPanel.activeSelf;

        // 2. Fecha todos os outros painéis principais primeiro
        if (AchievementMenu.Instancia != null)
        {
            AchievementMenu.Instancia.ClosePanel();
        }
        // Adicione aqui outros painéis principais para fechar se houver no futuro

        // 3. Define o estado final do painel de upgrades
        if (upgradeMenuPanel != null)
        {
            upgradeMenuPanel.SetActive(shouldBeActive);
        }
    }
    
    public void CloseUpgradePanels()
    {
        if (upgradeMenuPanel != null)
        {
            upgradeMenuPanel.SetActive(false);
        }
    }
    public void ShowUpgradeTAB()
    {
        if (upgradeTAB != null)
        {
            upgradeTAB.SetActive(true);
            if (buyTAB != null) buyTAB.SetActive(false);
            if (structureTAB != null) structureTAB.SetActive(false);
        }
    }
    public void ShowBuyTAB()
    {
        if (buyTAB != null)
        {
            buyTAB.SetActive(true);
            if (upgradeTAB != null) upgradeTAB.SetActive(false);
            if (structureTAB != null) structureTAB.SetActive(false);
        }
    }
    public void BackToMainMenu()
    {
        mainMenuPanel.SetActive(false);
        
    }
}