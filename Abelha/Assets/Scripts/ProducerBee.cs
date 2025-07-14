using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ProducerBee : MonoBehaviour, BeeStatsUpdater // Adicione IBoostableByQueen se ela for afetada pela aura
{
    [Header("Identificação")]
    public string beeTypeName = "ProducerBee";

    [Header("Parâmetros Base (Antes dos Upgrades)")]
    public float baseProductionTime = 2f;
    public float baseDepositTime = 2f;
    public float baseCollectionTime = 2f;
    public float baseProductionAmount = 1f;
    public float baseSpeed = 3.0f;

    [Header("Configuração Adicional")]
    public float initialDelay = 0f;
    public float destinationOffsetRadius = 1f;

    // Referências de destino agora são privadas
    private Transform _flowerTarget;
    private Transform _honeycombTarget;
    private Transform _hiveTarget;

    private NavMeshAgent agent;
    private float _effectiveProductionAmount;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(beeTypeName))
        {
            Debug.LogError($"Abelha {gameObject.name} não tem um beeTypeName definido!");
            this.enabled = false;
            return;
        }

        // Pega as referências dos destinos do LocationsManager
        if (LocationsManager.Instancia != null)
        {
            _flowerTarget = LocationsManager.Instancia.flowerTarget;
            _honeycombTarget = LocationsManager.Instancia.honeycombTarget;
            _hiveTarget = LocationsManager.Instancia.hiveTarget;
        }
        else
        {
            Debug.LogError($"LocationsManager não encontrado! A abelha {gameObject.name} não terá destinos.", this);
            this.enabled = false;
            return;
        }
        
        agent.avoidancePriority = Random.Range(0, 100);
        StartCoroutine(StartProducer());
    }

    private void OnEnable()
    {
        if (GerenciadorUpgrades.Instancia != null)
        {
            GerenciadorUpgrades.Instancia.RegistrarAbelha(this, beeTypeName);
        }
    }

    private void OnDisable()
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

    private IEnumerator StartProducer()
    {
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);
        yield return null;
        if (agent != null && agent.isOnNavMesh)
        {
            StartCoroutine(ProducerRoutine());
        }
    }

    private IEnumerator ProducerRoutine()
    {
        while (true)
        {
            _effectiveProductionAmount = baseProductionAmount;
            float productionMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.MelProduzido);
            float nectarMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.NectarColetado);

            if (GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Nectar) >= _effectiveProductionAmount)
            {
                if (GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Nectar, _effectiveProductionAmount))
                {
                    // Processar
                    yield return StartCoroutine(MoveToTarget(GetRandomDestination(_honeycombTarget.position)));
                    yield return new WaitForSeconds(baseProductionTime);
                    GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.MelProcessado, _effectiveProductionAmount);

                    // Depositar
                    yield return StartCoroutine(MoveToTarget(GetRandomDestination(_hiveTarget.position)));
                    yield return new WaitForSeconds(baseDepositTime);
                    float melFinalAmount = _effectiveProductionAmount * productionMultiplier;
                    if (GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.MelProcessado, _effectiveProductionAmount))
                    {
                        GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, melFinalAmount);
                    }
                    else
                    {
                        GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Nectar, _effectiveProductionAmount);
                        yield return new WaitForSeconds(1f); continue;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(0.5f); continue;
                }
            }
            else
            {
                // Coletar Nectar se faltar no estoque
                yield return StartCoroutine(MoveToTarget(GetRandomDestination(_flowerTarget.position)));
                yield return new WaitForSeconds(baseCollectionTime);
                float nectarColetado = baseProductionAmount * nectarMultiplier;
                GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Nectar, nectarColetado);
                yield return new WaitForSeconds(0.5f); continue;
            }

            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }

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
        if (agent == null || !agent.isOnNavMesh) { yield break; }
        agent.SetDestination(targetPosition);
        while (agent.pathPending || (agent.hasPath && agent.remainingDistance > agent.stoppingDistance))
        {
            if (!agent.hasPath && agent.pathStatus != NavMeshPathStatus.PathComplete) { yield break; }
            yield return null;
        }
    }
}