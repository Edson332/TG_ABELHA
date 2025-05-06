using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq; // Para usar Except

[RequireComponent(typeof(NavMeshAgent))]
public class QueenBeeController : MonoBehaviour
{
    // --- Singleton Instance ---
    public static QueenBeeController Instancia { get; private set; }

    [Header("Configuração da Aura")]
    public float auraRadius = 8f; // Raio da aura de boost
    public LayerMask beeLayerMask; // Configurar no Inspector para a layer das abelhas Worker/Producer
    public float checkInterval = 0.25f; // Intervalo para verificar quem está na aura (segundos)

    [Header("Bônus da Aura")]
    [Tooltip("Multiplicador na quantidade de néctar coletado (1.5 = +50%)")]
    public float nectarAmountMultiplier = 1.5f;
    [Tooltip("Multiplicador no tempo de coleta/produção (0.8 = 20% mais rápido)")]
    public float actionTimeMultiplier = 0.8f;

    [Header("Visual")]
    public GameObject auraVisualEffect; // Arraste o GameObject do efeito visual aqui

    private NavMeshAgent agent;
    private Collider[] _collidersInAura = new Collider[50]; // Prealoca array para OverlapSphere
    private HashSet<IBoostableByQueen> _beesInAura = new HashSet<IBoostableByQueen>(); // Rastreia quem está DENTRO
    private float _checkTimer;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Debug.LogWarning("Já existe uma Abelha Rainha! Destruindo esta.");
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        // Opcional: DontDestroyOnLoad(gameObject); se a rainha persistir entre cenas

        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        UpdateAuraVisualScale();
        _checkTimer = Random.Range(0, checkInterval); // Para escalonar as verificações iniciais
    }

    void Update()
    {
        _checkTimer -= Time.deltaTime;
        if (_checkTimer <= 0f)
        {
            CheckBeesInAura();
            _checkTimer = checkInterval;
        }
    }

    void OnDestroy()
    {
        // Garante que, se a rainha for destruída, as abelhas saibam que saíram da aura
        NotifyBeesOfExit(_beesInAura.ToList()); // Converte para lista para iterar
        if (Instancia == this)
        {
            Instancia = null;
        }
    }

    /// <summary>
    /// Move a Rainha para o destino especificado no NavMesh.
    /// Chamado pelo PlayerInputController.
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }

    /// <summary>
    /// Verifica quais abelhas estão dentro da aura e notifica entradas/saídas.
    /// </summary>
    private void CheckBeesInAura()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, auraRadius, _collidersInAura, beeLayerMask);

        HashSet<IBoostableByQueen> currentBeesFound = new HashSet<IBoostableByQueen>();

        for (int i = 0; i < count; i++)
        {
            // Tenta obter a interface IBoostableByQueen da abelha encontrada
            IBoostableByQueen boostable = _collidersInAura[i].GetComponent<IBoostableByQueen>();
            if (boostable != null)
            {
                currentBeesFound.Add(boostable);
            }
        }

        // Determina quem entrou e quem saiu desde a última checagem
        List<IBoostableByQueen> newlyEntered = currentBeesFound.Except(_beesInAura).ToList();
        List<IBoostableByQueen> justExited = _beesInAura.Except(currentBeesFound).ToList();

        // Notifica as abelhas
        NotifyBeesOfEntry(newlyEntered);
        NotifyBeesOfExit(justExited);

        // Atualiza o conjunto de quem está atualmente na aura
        _beesInAura = currentBeesFound;

        // Limpa a parte não usada do array para evitar referências antigas (boa prática)
        // System.Array.Clear(_collidersInAura, count, _collidersInAura.Length - count);
    }

    private void NotifyBeesOfEntry(List<IBoostableByQueen> bees)
    {
        foreach (var bee in bees)
        {
            if (bee != null) // Verifica se a abelha não foi destruída entre a detecção e a notificação
            {
               // Debug.Log($"Abelha {((MonoBehaviour)bee).gameObject.name} entrou na aura.");
                bee.EnterQueenAura(nectarAmountMultiplier, actionTimeMultiplier);
            }
        }
    }

    private void NotifyBeesOfExit(List<IBoostableByQueen> bees)
    {
        foreach (var bee in bees)
        {
             if (bee != null)
             {
               // Debug.Log($"Abelha {((MonoBehaviour)bee).gameObject.name} saiu da aura.");
                bee.ExitQueenAura();
             }
        }
    }


    /// <summary>
    /// Ajusta a escala do efeito visual da aura (se houver).
    /// </summary>
    private void UpdateAuraVisualScale()
    {
        if (auraVisualEffect != null)
        {
            // Assume que a escala base do efeito corresponde a um raio de 1 unidade
            // Ajuste esta lógica se o seu efeito visual escalar diferente
            auraVisualEffect.transform.localScale = Vector3.one * auraRadius * 2; // *2 porque escala é diâmetro
        }
    }

    // Para visualizar a aura no Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.25f); // Amarelo transparente
        Gizmos.DrawSphere(transform.position, auraRadius);
    }
}

// --- Interface para Abelhas que podem receber o Boost ---
// Isso permite que a Rainha interaja com qualquer tipo de abelha (Worker, Producer, etc.)
// sem conhecer a classe específica, desde que implementem a interface.
public interface IBoostableByQueen
{
    /// <summary>
    /// Chamado pela Rainha quando a abelha entra na aura.
    /// </summary>
    /// <param name="amountMultiplier">Multiplicador para quantidade (ex: néctar).</param>
    /// <param name="timeMultiplier">Multiplicador para tempo (menor = mais rápido).</param>
    void EnterQueenAura(float amountMultiplier, float timeMultiplier);

    /// <summary>
    /// Chamado pela Rainha quando a abelha sai da aura.
    /// </summary>
    void ExitQueenAura();
}