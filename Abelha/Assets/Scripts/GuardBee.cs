// Scripts/Bees/GuardBee.cs
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardBee : MonoBehaviour, BeeStatsUpdater // Não precisa de IBoostableByQueen se ela não coleta
{
    [Header("Identificação")]
    public string beeTypeName = "GuardBee";

    [Header("Parâmetros Base")]
    public float baseSpeed = 4.0f; // Um pouco mais rápida para parecer ágil
    
    [Header("Configuração Adicional")]
    public float initialDelay = 0f;

    private NavMeshAgent agent;
    private Transform _hiveTarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        // Pega o destino da colmeia
        if (LocationsManager.Instancia != null)
        {
            _hiveTarget = LocationsManager.Instancia.hiveTarget;
        }
        else
        {
            Debug.LogError($"LocationsManager não encontrado! A Abelha Guarda {gameObject.name} não terá destinos.", this);
            this.enabled = false;
            return;
        }
        
        // Comportamento simples: apenas anda aleatoriamente perto da colmeia
        InvokeRepeating(nameof(UpdatePatrolPoint), initialDelay + Random.Range(1f, 3f), Random.Range(5f, 10f));
    }

    void OnEnable()
    {
        if (GerenciadorUpgrades.Instancia != null)
        {
            GerenciadorUpgrades.Instancia.RegistrarAbelha(this, beeTypeName);
        }
    }

    void OnDisable()
    {
        if (GerenciadorUpgrades.Instancia != null)
        {
            GerenciadorUpgrades.Instancia.DesregistrarAbelha(this, beeTypeName);
        }
    }
    
    public void AtualizarVelocidade(float multiplicador)
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.speed = baseSpeed * multiplicador;
    }

    // A cada X segundos, encontra um novo ponto de patrulha perto da colmeia
    private void UpdatePatrolPoint()
    {
        if (agent != null && agent.isOnNavMesh && _hiveTarget != null)
        {
            Vector3 randomPoint = Random.insideUnitSphere * 5f; // Raio de patrulha de 5 unidades
            randomPoint += _hiveTarget.position;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
}