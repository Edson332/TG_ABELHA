// Scripts/UI/UIManager.cs
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instancia { get; private set; }

    [Header("Painéis Principais")]
    [Tooltip("Arraste para cá TODOS os painéis que devem ser mutuamente exclusivos (Loja, Melhorias, Conquistas, etc.).")]
    public List<GameObject> managedPanels;

    private bool _isGamePaused = false;

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    /// <summary>
    /// O método principal para mostrar um painel. Ele primeiro esconde todos os outros.
    /// </summary>
    /// <param name="panelToShow">O painel que deve se tornar visível.</param>
    public void ShowPanel(GameObject panelToShow)
    {
        // Não permite abrir novos painéis se o jogo estiver pausado
        if (_isGamePaused)
        {
            Debug.Log("Jogo está pausado. Abertura de novos painéis bloqueada.");
            return;
        }

        // 1. Esconde todos os painéis gerenciados
        foreach (var panel in managedPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        // 2. Mostra apenas o painel solicitado
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }
    }

    /// <summary>
    /// Esconde todos os painéis gerenciados. Útil para iniciar o combate, por exemplo.
    /// </summary>
    public void HideAllManagedPanels()
    {
        foreach (var panel in managedPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Método para ser chamado pelo MenuPausa para notificar o UIManager sobre o estado de pausa.
    /// </summary>
    public void SetGamePaused(bool isPaused)
    {
        _isGamePaused = isPaused;
    }
}