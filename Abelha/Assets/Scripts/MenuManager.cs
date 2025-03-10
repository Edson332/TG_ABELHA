using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    
    [Header("Dependencies")]
    public AchievementManager achievementManager; // Must be assigned in Inspector
    public GameObject[] achievementPanels;

    [Header("Buttons")]
    public Button[] menuButtons;

    void Start()
    {
        // Inicializa todos os painéis de achievement como inativos
        foreach (GameObject panel in achievementPanels)
        {
            panel.SetActive(false);
        }
        
        // Mostra o menu principal por padrão
        //ShowMainMenu();
        
        // Configura os listeners dos botões
        for (int i = 0; i < menuButtons.Length; i++)
        {
            int index = i; // Importante para o closure
            menuButtons[i].onClick.AddListener(() => ShowAchievementPanel(index));
        }

        // Atualiza os botões conforme o status dos achievements
        UpdateMenuButtons();
    }

    void Update()
    {
        // Atualiza os botões periodicamente (ou você pode chamar isso somente quando houver mudança)
        UpdateMenuButtons();
    }

    /// <summary>
    /// Atualiza a aparência e a interatividade de cada botão com base no achievement correspondente.
    /// Se o achievement estiver bloqueado, o botão ficará inativo, o texto será "???" e a cor ficará vermelha.
    /// </summary>
    public void UpdateMenuButtons()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (i < achievementManager.achievements.Length)
            {
                Achievement achievement = achievementManager.achievements[i];
                TextMeshProUGUI buttonText = menuButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                Image buttonImage = menuButtons[i].GetComponent<Image>();

                if (achievement.isUnlocked)
                {
                    menuButtons[i].interactable = true;
                    buttonText.text = achievement.title; // ou outro texto relevante
                    buttonImage.color = Color.white;
                }
                else
                {
                    menuButtons[i].interactable = false;
                    buttonText.text = "???";
                    buttonImage.color = Color.red;
                }
            }
        }
    }

    /// <summary>
    /// Mostra o painel de achievement correspondente, se o achievement já estiver desbloqueado.
    /// Caso contrário, exibe uma mensagem informando que ainda não foi alcançado.
    /// </summary>
    public void ShowAchievementPanel(int panelIndex)
    {
        // Só abre o painel se o achievement estiver desbloqueado
        if (panelIndex >= 0 && panelIndex < achievementManager.achievements.Length)
        {
            if (achievementManager.achievements[panelIndex].isUnlocked)
            {
                mainMenuPanel.SetActive(false);
                // Desativa todos os painéis
                foreach (GameObject panel in achievementPanels)
                {
                    panel.SetActive(false);
                }
                // Ativa o painel correspondente e atualiza a exibição
                achievementPanels[panelIndex].SetActive(true);
                UpdateAchievementDisplay(panelIndex);
            }
            else
            {
                Debug.Log("Este achievement ainda não foi alcançado!");
            }
        }
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        foreach (GameObject panel in achievementPanels)
        {
            panel.SetActive(false);
        }
    }

    public void UpdateAchievementDisplay(int panelIndex)
    {
        Achievement achievement = achievementManager.achievements[panelIndex];
        
        // Substitua o GetComponent<Text>() por GetComponent<TextMeshProUGUI>()
        TextMeshProUGUI titleText = achievementPanels[panelIndex].transform.Find("Title").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = achievementPanels[panelIndex].transform.Find("Description").GetComponent<TextMeshProUGUI>();
        Image iconImage = achievementPanels[panelIndex].transform.Find("Icon").GetComponent<Image>();
        
        titleText.text = achievement.title;
        descText.text = achievement.description;
        iconImage.sprite = achievement.icon;
        
        // Alterar a cor de fundo do painel conforme o status
        achievementPanels[panelIndex].GetComponent<Image>().color = 
            achievement.isUnlocked ? Color.white : Color.gray;
    }
    public void BackToMainMenu()
    {
        ShowMainMenu();
    }
}
