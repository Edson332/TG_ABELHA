// Scripts/UI/AchievementMenu.cs
using UnityEngine;

public class AchievementMenu : MonoBehaviour
{
    public static AchievementMenu Instancia { get; private set; }
    
    [Header("Painéis de Conquistas")]
    [Tooltip("O painel principal que contém a lista de conquistas primárias.")]
    public GameObject primaryAchievementPanel; // RENOMEADO para clareza

    [Tooltip("O painel secundário para as conquistas de abelhas raras.")]
    public GameObject secondaryAchievementPanel; // <<<--- NOVO CAMPO

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }
    
    // --- MÉTODOS PÚBLICOS PARA OS BOTÕES ---

    /// <summary>
    /// Abre/Fecha o menu de conquistas principal. Chamado pelo botão principal da UI.
    /// </summary>
    public void TogglePrimaryPanel()
    {
        // Se o painel primário já está ativo, fecha tudo.
        // Se não, abre o primário (e garante que o secundário esteja fechado).
        bool shouldBeActive = !primaryAchievementPanel.activeSelf;

        // Fecha outros menus principais, como o de upgrades
        if (MenuManager.Instancia != null)
        {
            MenuManager.Instancia.CloseUpgradePanels();
        }
        
        // Garante que o painel secundário esteja sempre fechado ao alternar o primário
        if (secondaryAchievementPanel != null)
        {
            secondaryAchievementPanel.SetActive(false);
        }

        // Define o estado do painel primário
        if (primaryAchievementPanel != null)
        {
            primaryAchievementPanel.SetActive(shouldBeActive);
        }
    }

    /// <summary>
    /// Fecha o painel primário e abre o secundário. Chamado pelo botão "Conquistas Raras".
    /// </summary>
    public void ShowSecondaryPanel()
    {
        if (primaryAchievementPanel != null)
        {
            primaryAchievementPanel.SetActive(false);
        }
        if (secondaryAchievementPanel != null)
        {
            secondaryAchievementPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Fecha o painel secundário e volta para o primário. Chamado pelo botão "Voltar".
    /// </summary>
    public void ShowPrimaryPanel()
    {
        if (secondaryAchievementPanel != null)
        {
            secondaryAchievementPanel.SetActive(false);
        }
        if (primaryAchievementPanel != null)
        {
            primaryAchievementPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Apenas fecha todos os painéis de conquista. Chamado por outros scripts (como o MenuManager).
    /// </summary>
    public void CloseAllPanels()
    {
        if (primaryAchievementPanel != null)
        {
            primaryAchievementPanel.SetActive(false);
        }
        if (secondaryAchievementPanel != null)
        {
            secondaryAchievementPanel.SetActive(false);
        }
    }
}