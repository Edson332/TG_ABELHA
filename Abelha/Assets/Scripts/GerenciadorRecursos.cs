using System.Collections.Generic;
using UnityEngine;

// Define os tipos de recurso
public enum TipoRecurso
{
    Nectar,
    MelProcessado,
    Mel,
    GeleiaReal
}

public class GerenciadorRecursos : MonoBehaviour
{

    [Header("Gatilhos de Tutorial")]
    [Tooltip("Tutorial a ser exibido na primeira vez que o jogador coleta Mel.")]
    public TutorialStepSO primeiroMelTutorial;

    private bool _primeiroMelTutorialDisparado = false;
    // Instância única para acesso global
    public static GerenciadorRecursos Instancia { get; private set; }

    // Dicionário que armazena os recursos usando o enum como chave
    private Dictionary<TipoRecurso, float> recursos = new Dictionary<TipoRecurso, float>();

    private void Awake()
    {
        // Garante que exista somente uma instância
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        // Inicializa os recursos com valores padrão (0 neste caso)
        InicializarRecurso(TipoRecurso.Nectar, 0);
        InicializarRecurso(TipoRecurso.MelProcessado, 0);
        InicializarRecurso(TipoRecurso.Mel, 0);
        InicializarRecurso(TipoRecurso.GeleiaReal, 0);
    }

    /// <summary>
    /// Inicializa um recurso com um valor inicial (caso ainda não exista).
    /// </summary>
    public void InicializarRecurso(TipoRecurso tipo, float valorInicial = 0)
    {
        if (!recursos.ContainsKey(tipo))
        {
            recursos.Add(tipo, valorInicial);
        }
    }

    /// <summary>
    /// Adiciona uma quantidade ao recurso especificado.
    /// </summary>
    public void AdicionarRecurso(TipoRecurso tipo, float valor)
    {
        if (recursos.ContainsKey(tipo))
        {
            recursos[tipo] += valor;
        }
        else
        {
            recursos.Add(tipo, valor);
        }

            if (tipo == TipoRecurso.Mel && !_primeiroMelTutorialDisparado)
    {
        if (recursos[tipo] > 0) // Ou uma condição mais específica como >= 1
        {
            if (TutorialManager.Instancia != null)
            {
                TutorialManager.Instancia.RequestTutorial(primeiroMelTutorial);
            }
            _primeiroMelTutorialDisparado = true; // Garante que esta verificação só ocorra uma vez
        }
    }
    }

    public void ResetRecursos()
    {
        // Cria uma lista de chaves para evitar modificar o dicionário enquanto itera
        List<TipoRecurso> keys = new List<TipoRecurso>(recursos.Keys);
        foreach (var key in keys)
        {
            recursos[key] = 0;
            recursos[TipoRecurso.GeleiaReal] = 0;
        }
        Debug.Log("Todos os recursos foram resetados para 0.");
        
    }

    public bool RemoverRecurso(TipoRecurso tipo, float valor)
    {
        if (recursos.ContainsKey(tipo) && recursos[tipo] >= valor)
        {
            recursos[tipo] -= valor;
            return true;
        }
        return false;
    }

    public void SetRecurso(TipoRecurso tipo, float quantidade)
    {
        if (recursos.ContainsKey(tipo))
        {
            recursos[tipo] = quantidade;
        }
        else
        {
            recursos.Add(tipo, quantidade);
        }
        // Opcional: Disparar um evento aqui para que a UI de recursos seja atualizada imediatamente.
        // Ex: OnRecursoAtualizado?.Invoke(tipo, quantidade);
    }
    /// <summary>
    /// Retorna o valor atual do recurso especificado.
    /// </summary>
    public float ObterRecurso(TipoRecurso tipo)
    {
        if (recursos.ContainsKey(tipo))
        {
            return recursos[tipo];
        }
        return 0;
    }
}
