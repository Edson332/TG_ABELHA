// Scripts/UI/AchievementMenu.cs
using UnityEngine;

public class AchievementMenu : MonoBehaviour
{
    public static AchievementMenu Instancia { get; private set; }
    
    [Tooltip("O painel principal que contém a lista de botões de conquista.")]
    public GameObject achievementPanel;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }
    
    /// <summary>
    /// Mostra/esconde o painel principal de conquistas. Chamado pelo botão da UI.
    /// </summary>
    public void TogglePanel()
    {
        bool shouldBeActive = !achievementPanel.activeSelf;

        // Primeiro, garante que o outro painel principal (Upgrades) esteja fechado.
        if (MenuManager.Instancia != null)
        {
            MenuManager.Instancia.CloseUpgradePanels();
        }
        
        // Então, ativa ou desativa o painel de conquistas.
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(shouldBeActive);
        }
    }

    /// <summary>
    /// Apenas fecha este painel. Chamado por outros scripts.
    /// </summary>
    public void ClosePanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }
    }
}