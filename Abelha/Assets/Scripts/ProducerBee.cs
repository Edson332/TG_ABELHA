using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ProducerBee : MonoBehaviour, BeeStatsUpdater
{
    [Header("Identificação")]
    // !! IMPORTANTE: Este nome DEVE CORRESPONDER ao beeTypeName no BeeUpgradeData asset !!
    public string beeTypeName = "ProducerBee";

    [Header("Locais de Destino")]
    public Transform flower;
    public Transform honeycomb;
    public Transform hive;

    [Header("Parâmetros Base (Antes dos Upgrades)")]
    public float baseProductionTime = 2f;
    public float baseDepositTime = 2f;
    public float baseCollectionTime = 2f;
    public float baseProductionAmount = 1f; // Quanto Nectar ela tenta processar por ciclo
    public float baseSpeed = 3.0f;

    [Header("Configuração Adicional")]
    public float initialDelay = 0f;
    public float destinationOffsetRadius = 1f;

    private NavMeshAgent agent;
    private float _effectiveProductionAmount;

     private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
         if (string.IsNullOrEmpty(beeTypeName)) {
            Debug.LogError($"Abelha {gameObject.name} não tem um beeTypeName definido!");
            this.enabled = false;
            return;
        }
        agent.avoidancePriority = Random.Range(0, 100);
        StartCoroutine(StartProducer());
    }

     // Registro e Desregistro (Igual WorkerBee)
    private void OnEnable()
    {
        if (GerenciadorUpgrades.Instancia != null && !string.IsNullOrEmpty(beeTypeName))
        {
            GerenciadorUpgrades.Instancia.RegistrarAbelha(this, beeTypeName);
        } else if (GerenciadorUpgrades.Instancia == null) {
            Debug.LogWarning($"GerenciadorUpgrades não encontrado ao habilitar {beeTypeName} {gameObject.name}.");
        }
    }

    private void OnDisable()
    {
        if (GerenciadorUpgrades.Instancia != null && !string.IsNullOrEmpty(beeTypeName))
        {
            GerenciadorUpgrades.Instancia.DesregistrarAbelha(this, beeTypeName);
        }
    }

    // Método da interface
    public void AtualizarVelocidade(float multiplicador)
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.speed = baseSpeed * multiplicador;
    }

    private IEnumerator StartProducer()
    {
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);
        yield return null; // Espera registro
         if (agent != null && agent.isOnNavMesh) {
            StartCoroutine(ProducerRoutine());
        } else {
             Debug.LogError($"ProducerBee {gameObject.name} ({beeTypeName}) não pôde iniciar rotina. Agent: {agent}, IsOnNavMesh: {agent?.isOnNavMesh}");
        }
    }

    private IEnumerator ProducerRoutine()
    {
        // Pega os multiplicadores no início
        float productionMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.MelProduzido);
        float nectarMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.NectarColetado); // Para coleta opcional

        while (true)
        {
             // Quanto ela TENTA processar (base, não afetado por upgrades de coleta aqui)
            _effectiveProductionAmount = baseProductionAmount;

            // 1. Verifica estoque e tenta pegar Nectar
            if (GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Nectar) >= _effectiveProductionAmount)
            {
                if (GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Nectar, _effectiveProductionAmount))
                {
                    // Conseguiu pegar Nectar, processa
                    yield return StartCoroutine(MoveToTarget(GetRandomDestination(honeycomb.position)));
                    yield return new WaitForSeconds(baseProductionTime);
                    GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.MelProcessado, _effectiveProductionAmount);

                    // 2. Deposita Mel
                    yield return StartCoroutine(MoveToTarget(GetRandomDestination(hive.position)));
                    yield return new WaitForSeconds(baseDepositTime);
                    float melFinalAmount = _effectiveProductionAmount * productionMultiplier; // Aplica bônus
                    if(GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.MelProcessado, _effectiveProductionAmount))
                    {
                        GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, melFinalAmount);
                    } else {
                        //Debug.LogWarning($"{beeTypeName} {gameObject.name}: Mel Processado sumiu. Devolvendo Nectar.");
                        GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Nectar, _effectiveProductionAmount);
                        yield return new WaitForSeconds(1f); continue;
                    }
                }
                else
                {
                    //Debug.Log($"{beeTypeName} {gameObject.name}: Nectar sumiu antes de pegar. Tentando de novo...");
                    yield return new WaitForSeconds(0.5f); continue;
                }
            }
            else // Falta Nectar no estoque
            {
                //Debug.Log($"{beeTypeName} {gameObject.name}: Nectar insuficiente. Indo coletar...");
                yield return StartCoroutine(MoveToTarget(GetRandomDestination(flower.position)));
                yield return new WaitForSeconds(baseCollectionTime);
                // Coleta com bônus de coleta DESTE TIPO de abelha
                float nectarColetado = baseProductionAmount * nectarMultiplier;
                GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Nectar, nectarColetado);
                yield return new WaitForSeconds(0.5f); continue;
            }

             // Pega multiplicadores novamente para o próximo ciclo
             productionMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.MelProduzido);
             nectarMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.NectarColetado);

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