using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// --- MUDANÇA 1: Implementar a interface IBoostableByQueen ---
[RequireComponent(typeof(NavMeshAgent))]
public class ProducerBee : MonoBehaviour, BeeStatsUpdater, IBoostableByQueen
{
    [Header("Identificação")]
    [Tooltip("ID interno do tipo. DEVE CORRESPONDER ao usado nos ScriptableObjects de Upgrade e no BeeManager.")]
    public string beeTypeName = "ProducerBee";

    // --- MUDANÇA 2: Adicionar campo para o VFX da Aura ---
    [Header("Efeitos Visuais")]
    [Tooltip("O GameObject do VFX da aura da rainha, que é filho desta abelha.")]
    public GameObject auraVFX;

    [Header("Parâmetros Base (Antes dos Upgrades)")]
    public float baseProductionTime = 2f;
    public float baseDepositTime = 2f;
    public float baseCollectionTime = 2f;
    public float baseProductionAmount = 1f;
    public float baseSpeed = 3.0f;

    [Header("Configuração Adicional")]
    public float initialDelay = 0f;
    public float destinationOffsetRadius = 1f;

    // Referências de destino privadas
    private Transform _honeycombTarget;
    private Transform _hiveTarget;

    private NavMeshAgent agent;
    private float _effectiveProductionAmount;

    // --- MUDANÇA 3: Adicionar campos para controlar o estado da Aura ---
    private bool _isInQueenAura = false;
    private float _queenAmountMultiplier = 1f;
    private float _queenTimeMultiplier = 1f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(beeTypeName))
        {
            Debug.LogError($"Abelha {gameObject.name} não tem um beeTypeName definido!", this);
            this.enabled = false;
            return;
        }

        if (LocationsManager.Instancia != null)
        {
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
        // Garante que o estado da aura seja resetado se a abelha for desativada
        ExitQueenAura();
    }

    // --- MUDANÇA 4: Implementar os métodos da interface da Aura ---
    public void EnterQueenAura(float amountMultiplier, float timeMultiplier)
    {
        _isInQueenAura = true;
        _queenAmountMultiplier = amountMultiplier;
        _queenTimeMultiplier = timeMultiplier;
        if (auraVFX != null) auraVFX.SetActive(true);
    }

    public void ExitQueenAura()
    {
        _isInQueenAura = false;
        _queenAmountMultiplier = 1f;
        _queenTimeMultiplier = 1f;
        if (auraVFX != null) auraVFX.SetActive(false);
    }
    // --- FIM DA MUDANÇA 4 ---

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
            // --- MUDANÇA 5: Aplicar os bônus da aura nos cálculos ---
            _effectiveProductionAmount = baseProductionAmount; // A quantidade base que ela tenta processar
            
            // Multiplicadores dos upgrades
            float productionUpgradeMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.MelProduzido);
            float nectarUpgradeMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.NectarColetado);

            // Multiplicadores FINAIS, combinando upgrades e bônus da rainha
            float finalProductionMultiplier = productionUpgradeMultiplier * (_isInQueenAura ? _queenAmountMultiplier : 1f);
            float finalNectarMultiplier = nectarUpgradeMultiplier * (_isInQueenAura ? _queenAmountMultiplier : 1f);
            float finalTimeMultiplier = _isInQueenAura ? _queenTimeMultiplier : 1f;

            // 1. Tenta pegar Néctar do estoque global
            if (GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Nectar, _effectiveProductionAmount))
            {
                // Processar
                yield return StartCoroutine(MoveToTarget(GetRandomDestination(_honeycombTarget.position)));
                yield return new WaitForSeconds(baseProductionTime * finalTimeMultiplier); // Tempo reduzido pela aura
                
                // Depositar
                yield return StartCoroutine(MoveToTarget(GetRandomDestination(_hiveTarget.position)));
                yield return new WaitForSeconds(baseDepositTime * finalTimeMultiplier); // Tempo reduzido pela aura
                float melFinalAmount = _effectiveProductionAmount * finalProductionMultiplier; // Produção aumentada por upgrades e aura
                GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, melFinalAmount);
                GameEvents.ReportResourceCollected(transform, melFinalAmount, TipoRecurso.Mel);
            }
            else
            {
                // 2. Se não conseguiu, vai coletar Néctar para o estoque
                Transform targetFlower = LocationsManager.Instancia.GetRandomFlowerTarget();
                if (targetFlower == null)
                {
                    yield return new WaitForSeconds(5f);
                    continue;
                }

                yield return StartCoroutine(MoveToTarget(GetRandomDestination(targetFlower.position)));
                yield return new WaitForSeconds(baseCollectionTime * finalTimeMultiplier); // Tempo reduzido pela aura
                float nectarColetado = _effectiveProductionAmount * finalNectarMultiplier; // Coleta aumentada por upgrades e aura
                GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Nectar, nectarColetado);
                yield return new WaitForSeconds(0.5f);
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