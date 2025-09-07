using UnityEngine;
using UnityEngine.UI; // Se usar UI padrão
using TMPro; 

public class RecursosUI : MonoBehaviour
{
    [Header("Referências UI")]
    public TextMeshProUGUI nectarText; // Ou TextMeshProUGUI
    public TextMeshProUGUI melProcessadoText; // Ou TextMeshProUGUI
    public TextMeshProUGUI melText; // Ou TextMeshProUGUI

    // Cache para evitar chamar GetComponent todo frame (opcional mas bom)
    private GerenciadorRecursos _gerenciadorRecursos;

    void Start()
    {
        // Tenta encontrar a instância no início
        _gerenciadorRecursos = GerenciadorRecursos.Instancia;
        if (_gerenciadorRecursos == null)
        {
            Debug.LogError("GerenciadorRecursos não encontrado na cena!");
            this.enabled = false; // Desabilita o script se não encontrar o gerenciador
            return;
        }

        // Atualiza a UI uma vez no início
        AtualizarTextosRecursos();
    }

    void Update()
    {
        // Atualiza a UI a cada frame (ou pode ser otimizado para atualizar
        // apenas quando os recursos mudam, usando eventos)
        AtualizarTextosRecursos();
    }

    void AtualizarTextosRecursos()
    {
        if (_gerenciadorRecursos == null) return; // Segurança

        // Obtem os valores
        float nectarValor = _gerenciadorRecursos.ObterRecurso(TipoRecurso.Nectar);
        float melProcessadoValor = _gerenciadorRecursos.ObterRecurso(TipoRecurso.MelProcessado);
        float melValor = _gerenciadorRecursos.ObterRecurso(TipoRecurso.Mel);

        // Atualiza os textos formatando para UMA casa decimal ("F1")
        if (nectarText != null) {
             // ToString("F1") formata para 1 casa decimal
             // CultureInfo.InvariantCulture garante que o separador decimal seja '.'
             nectarText.text = $"Néctar: {nectarValor.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}";
        }

        if (melProcessadoText != null) {
            melProcessadoText.text = $"Processando: {melProcessadoValor.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}";
        }

         if (melText != null) {
            melText.text = $"Mel: {melValor.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}";
        }
    }
}