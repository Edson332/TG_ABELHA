using System.Collections.Generic;
using UnityEngine;

// Define os tipos de recurso
public enum TipoRecurso
{
    Nectar,
    MelProcessado,
    Mel
}

public class GerenciadorRecursos : MonoBehaviour
{
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
    }

    /// <summary>
    /// Remove uma quantidade do recurso especificado se houver saldo suficiente.
    /// Retorna true se a operação foi realizada; caso contrário, false.
    /// </summary>
    public bool RemoverRecurso(TipoRecurso tipo, float valor)
    {
        if (recursos.ContainsKey(tipo) && recursos[tipo] >= valor)
        {
            recursos[tipo] -= valor;
            return true;
        }
        return false;
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
