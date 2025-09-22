using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq; // Para usar Except

public interface IAuraAffectable
{
    void EnterQueenAura(float amountMultiplier, float timeMultiplier);
    void ExitQueenAura();
}

[RequireComponent(typeof(NavMeshAgent))]
public class QueenBeeController : MonoBehaviour
{
    // --- Singleton Instance ---
public static QueenBeeController Instancia { get; private set; }

    [Header("Configuração da Aura")]
    public float auraRadius = 8f;
    public LayerMask beeLayerMask;
    public float checkInterval = 0.25f;

    [Header("Bônus da Aura")]
    public float nectarAmountMultiplier = 1.5f;
    public float actionTimeMultiplier = 0.8f;

    [Header("Visual")]
    public GameObject auraVisualEffect;
    
    private NavMeshAgent agent;
    private Collider[] _collidersInAura = new Collider[150]; // Aumente se tiver muitas abelhas
    private HashSet<GameObject> _beesInAura = new HashSet<GameObject>(); // Rastreia GameObjects
    private float _checkTimer;

    private const string AURA_VFX_KEY = "AuraVFXEnabled";

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
        UpdateVisualsBasedOnSetting();
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
    public void UpdateVisualsBasedOnSetting()
    {
        if (auraVisualEffect != null)
        {
            // Lê a preferência do jogador. Padrão é 1 (ativo).
            bool isEnabled = PlayerPrefs.GetInt(AURA_VFX_KEY, 1) == 1;
            // Ativa ou desativa o objeto do efeito visual
            auraVisualEffect.SetActive(isEnabled);
        }
    }
    /// <summary>
    /// Verifica quais abelhas estão dentro da aura e notifica entradas/saídas.
    /// </summary>
 private void CheckBeesInAura()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, auraRadius, _collidersInAura, beeLayerMask);
        HashSet<GameObject> currentBeesFound = new HashSet<GameObject>();

        for (int i = 0; i < count; i++)
        {
            currentBeesFound.Add(_collidersInAura[i].gameObject);
        }

        // Compara a lista atual com a anterior para ver quem entrou e quem saiu
        foreach (var beeGO in currentBeesFound)
        {
            // Se não estava na lista antes, é uma nova entrada
            if (!_beesInAura.Contains(beeGO))
            {
                NotifyBeeEntry(beeGO);
            }
        }
        foreach (var beeGO in _beesInAura)
        {
            // Se estava na lista antiga, mas não na atual, acabou de sair
            if (!currentBeesFound.Contains(beeGO))
            {
                NotifyBeeExit(beeGO);
            }
        }
        
        _beesInAura = currentBeesFound;
    }

    private void NotifyBeeEntry(GameObject beeGO)
    {
        // Tenta notificar todos os tipos de abelhas
        beeGO.GetComponent<IBoostableByQueen>()?.EnterQueenAura(nectarAmountMultiplier, actionTimeMultiplier);
        beeGO.GetComponent<PassiveBee>()?.EnterQueenAura();
    }

    private void NotifyBeeExit(GameObject beeGO)
    {
        if (beeGO == null) return; // Abelha pode ter sido destruída
        // Tenta notificar todos os tipos de abelhas
        beeGO.GetComponent<IBoostableByQueen>()?.ExitQueenAura();
        beeGO.GetComponent<PassiveBee>()?.ExitQueenAura();
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