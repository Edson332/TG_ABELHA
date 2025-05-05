using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WorkerBee : MonoBehaviour, BeeStatsUpdater
{
    [Header("Identificação")]
    // !! IMPORTANTE: Este nome DEVE CORRESPONDER ao beeTypeName no BeeUpgradeData asset !!
    public string beeTypeName = "WorkerBee";

    [Header("Locais de Destino")]
    public Transform flower;
    public Transform honeycomb;
    public Transform hive;

    [Header("Parâmetros Base (Antes dos Upgrades)")]
    public float baseCollectionTime = 2f;
    public float baseProcessingTime = 2f;
    public float baseDepositTime = 2f;
    public float baseCollectionAmount = 1f;
    public float baseSpeed = 3.5f;

    [Header("Configuração Adicional")]
    public float initialDelay = 0f;
    public float destinationOffsetRadius = 1f;

    private NavMeshAgent agent;
    private float _effectiveCollectionAmount;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // A velocidade inicial será definida no OnEnable ao registrar
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(beeTypeName)) {
            Debug.LogError($"Abelha {gameObject.name} não tem um beeTypeName definido!");
            this.enabled = false; // Desabilita a abelha se não tiver tipo
            return;
        }
        agent.avoidancePriority = Random.Range(0, 100);
        StartCoroutine(StartWorker());
    }

    private void OnEnable()
    {
        // Garante que o Gerenciador já existe
        if (GerenciadorUpgrades.Instancia != null && !string.IsNullOrEmpty(beeTypeName))
        {
             // Registra a abelha COM seu tipo
            GerenciadorUpgrades.Instancia.RegistrarAbelha(this, beeTypeName);
        } else if (GerenciadorUpgrades.Instancia == null) {
             Debug.LogWarning($"GerenciadorUpgrades não encontrado ao habilitar {beeTypeName} {gameObject.name}. Registro adiado.");
             // Poderia tentar registrar novamente no Start ou Update se necessário
        }
    }

    private void OnDisable()
    {
        if (GerenciadorUpgrades.Instancia != null && !string.IsNullOrEmpty(beeTypeName))
        {
            GerenciadorUpgrades.Instancia.DesregistrarAbelha(this, beeTypeName);
        }
    }

    public void AtualizarVelocidade(float multiplicador)
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.speed = baseSpeed * multiplicador;
    }

    private IEnumerator StartWorker()
    {
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);
        // Espera um frame para garantir que o registro no OnEnable ocorreu
        yield return null;
        // Verifica se o agente está ativo e no NavMesh antes de iniciar a rotina
        if (agent != null && agent.isOnNavMesh) {
            StartCoroutine(WorkerRoutine());
        } else {
            Debug.LogError($"WorkerBee {gameObject.name} ({beeTypeName}) não pôde iniciar rotina. Agent: {agent}, IsOnNavMesh: {agent?.isOnNavMesh}");
        }
    }


    private IEnumerator WorkerRoutine()
    {
        // Pega os multiplicadores específicos deste tipo de abelha UMA VEZ no início do loop
        // (se os upgrades forem comprados durante a execução, eles serão pegos no próximo loop)
        float nectarMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.NectarColetado);
        float productionMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.MelProduzido);


        while (true) // O loop principal da abelha
        {
            // Calcula a quantidade efetiva no início de cada ciclo COMPLETO
             _effectiveCollectionAmount = baseCollectionAmount * nectarMultiplier;

            // 1. Coletar Nectar
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(flower.position)));
            yield return new WaitForSeconds(baseCollectionTime);
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Nectar, _effectiveCollectionAmount);

            // 2. Processar Nectar
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(honeycomb.position)));
            yield return new WaitForSeconds(baseProcessingTime);
            if (GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Nectar, _effectiveCollectionAmount))
            {
                GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.MelProcessado, _effectiveCollectionAmount);
            } else {
                 //Debug.LogWarning($"{beeTypeName} {gameObject.name} sem Nectar para processar.");
                 yield return new WaitForSeconds(1f); continue;
            }

            // 3. Depositar Mel
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(hive.position)));
            yield return new WaitForSeconds(baseDepositTime);
            float melFinalAmount = _effectiveCollectionAmount * productionMultiplier; // Aplica bônus de produção aqui
            if(GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.MelProcessado, _effectiveCollectionAmount))
            {
                 GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, melFinalAmount);
            } else {
                //Debug.LogWarning($"{beeTypeName} {gameObject.name} sem Mel Processado para depositar.");
                 yield return new WaitForSeconds(1f); continue;
            }

            // Pega os multiplicadores novamente para o próximo ciclo, caso tenham mudado
             nectarMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.NectarColetado);
             productionMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.MelProduzido);

            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }

    // Funções GetRandomDestination e MoveToTarget (sem alterações)
     private Vector3 GetRandomDestination(Vector3 basePosition)
    {
        Vector2 randomOffset = Random.insideUnitCircle * destinationOffsetRadius;
        Vector3 targetPos = basePosition + new Vector3(randomOffset.x, 0, randomOffset.y);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, destinationOffsetRadius * 2, NavMesh.AllAreas)) { return hit.position; }
        return basePosition;
    }

    private IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        if (!agent.isOnNavMesh) { yield break; }
        agent.SetDestination(targetPosition);
        while (agent.pathPending || (agent.hasPath && agent.remainingDistance > agent.stoppingDistance))
        {
            if (!agent.hasPath && agent.pathStatus != NavMeshPathStatus.PathComplete) { yield break; }
            yield return null;
        }
    }
}