using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    [Tooltip("Texto onde serão exibidos os recursos e a contagem de abelhas")]
    public TextMeshProUGUI resourceText;
    public TextMeshProUGUI resourceBeeText;
    void Update()
    {
        if (resourceText == null || GerenciadorRecursos.Instancia == null)
            return;

        // Monta as linhas de recursos
        string display = 
            $"Néctar: {GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Nectar)}\n" +
            $"Mel Processado: {GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.MelProcessado)}\n" +
            $"Mel: {GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Mel)}";

        // Se o BeeManager existir, adiciona a contagem de abelhas
        if (BeeManager.Instancia != null)
        {   
            
            // GetBeeCountString retorna algo como:
            // "WorkerBee 2/3\nProducerBee 1/5\n..."
            string beeCounts = BeeManager.Instancia.GetBeeCountString();
            string displayBee = "\n\nAbelhas:\n" + beeCounts;
            resourceBeeText.text = displayBee;
        }
        
        resourceText.text = display;
    }
}