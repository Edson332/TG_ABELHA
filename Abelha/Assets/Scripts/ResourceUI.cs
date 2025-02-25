using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    public TextMeshProUGUI resourceText;

    void Update()
    {
        if (resourceText != null && GerenciadorRecursos.Instancia != null)
        {
            resourceText.text = $"Néctar: {GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Nectar)}\n" +
                                $"Mel Processado: {GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.MelProcessado)}\n" +
                                $"Mel: {GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Mel)}";
        }
    }
}
