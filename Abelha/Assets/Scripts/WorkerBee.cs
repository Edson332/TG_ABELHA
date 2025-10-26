using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WorkerBee : MonoBehaviour, BeeStatsUpdater, IBoostableByQueen
{
    [Header("Identificação")]
    [Tooltip("ID interno do tipo. DEVE CORRESPONDER ao usado nos ScriptableObjects de Upgrade e no BeeManager.")]
    public string beeTypeName = "WorkerBee";

    [Header("Parâmetros Base (Antes dos Upgrades)")]
    public float baseCollectionTime = 2f;
    public float baseProcessingTime = 2f;
    public float baseDepositTime = 2f;
    public float baseCollectionAmount = 1f;
    public float baseSpeed = 3.5f;

    [Header("Configuração Adicional")]
    public float initialDelay = 0f;
    public float destinationOffsetRadius = 1f;

    // Referências de destino agora são privadas e obtidas do LocationsManager
    private Transform _honeycombTarget;
    private Transform _hiveTarget;
    
    private NavMeshAgent agent;
    private float _effectiveCollectionAmount;
    private float _currentNectarMultiplierFromUpgrades = 1f;
    private float _currentProductionMultiplierFromUpgrades = 1f;

    // Campos para a Aura da Rainha
    private bool _isInQueenAura = false;
    private float _queenAmountMultiplier = 1f;
    private float _queenTimeMultiplier = 1f;

    [Header("Efeitos Visuais")]
    [Tooltip("O GameObject do VFX da aura da rainha, que é filho desta abelha.")]
    public GameObject auraVFX;

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

        // Pega as referências dos destinos do nosso Singleton LocationsManager
        if (LocationsManager.Instancia != null)
        {
            // Note que não pegamos mais a flor aqui, pois ela é selecionada aleatoriamente a cada ciclo.
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
        StartCoroutine(StartWorker());
    }
    public float GetAverageHoneyPerSecond()
    {
        // 1. Calcula a quantidade de mel por ciclo
        _effectiveCollectionAmount = baseCollectionAmount * _currentNectarMultiplierFromUpgrades;
        float finalCollectionAmount = _effectiveCollectionAmount * (_isInQueenAura ? _queenAmountMultiplier : 1f);
        float finalProductionMultiplier = _currentProductionMultiplierFromUpgrades * (_isInQueenAura ? _queenAmountMultiplier : 1f);
        float melPerCycle = finalCollectionAmount * finalProductionMultiplier;

        // 2. Calcula o tempo total estimado do ciclo
        // (Isso é uma simplificação, não considera o tempo de viagem)
        float finalCollectionTime = baseCollectionTime * (_isInQueenAura ? _queenTimeMultiplier : 1f);
        float finalProcessingTime = baseProcessingTime * (_isInQueenAura ? _queenTimeMultiplier : 1f);
        float finalDepositTime = baseDepositTime * (_isInQueenAura ? _queenTimeMultiplier : 1f);
        float totalCycleTime = finalCollectionTime + finalProcessingTime + finalDepositTime + 1.0f; // Adiciona um tempo estimado para viagem/pausa

        // 3. Calcula Mel por Segundo
        if (totalCycleTime <= 0) return 0; // Evita divisão por zero
        return melPerCycle / totalCycleTime;
    }

    private void OnEnable()
    {
        if (GerenciadorUpgrades.Instancia != null)
        {
            GerenciadorUpgrades.Instancia.RegistrarAbelha(this, beeTypeName);
            UpdateUpgradeMultipliers();
        }
    }

    private void OnDisable()
    {
        if (GerenciadorUpgrades.Instancia != null)
        {
            GerenciadorUpgrades.Instancia.DesregistrarAbelha(this, beeTypeName);
        }
        ExitQueenAura(); // Garante que o estado de buff seja resetado
    }

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

    public void AtualizarVelocidade(float multiplicador)
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.speed = baseSpeed * multiplicador;
    }

    private void UpdateUpgradeMultipliers()
    {
         if (GerenciadorUpgrades.Instancia != null)
         {
             _currentNectarMultiplierFromUpgrades = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.NectarColetado);
             _currentProductionMultiplierFromUpgrades = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.MelProduzido);
         }
    }

    private IEnumerator StartWorker()
    {
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);
        yield return null; // Garante que o Start() de outros scripts tenha rodado
        if (agent != null && agent.isOnNavMesh)
        {
             UpdateUpgradeMultipliers();
             StartCoroutine(WorkerRoutine());
        }
    }

    private IEnumerator WorkerRoutine()
    {
        while (true)
        {
            // --- 1. Coleta de Néctar ---
            Transform targetFlower = LocationsManager.Instancia.GetRandomFlowerTarget();
            if (targetFlower == null)
            {
                Debug.LogWarning("Nenhuma flor encontrada, esperando...", this);
                yield return new WaitForSeconds(5f);
                continue; // Pula para a próxima iteração do loop
            }
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(targetFlower.position)));
            
            _effectiveCollectionAmount = baseCollectionAmount * _currentNectarMultiplierFromUpgrades;
            float finalCollectionAmount = _effectiveCollectionAmount * (_isInQueenAura ? _queenAmountMultiplier : 1f);
            float finalCollectionTime = baseCollectionTime * (_isInQueenAura ? _queenTimeMultiplier : 1f);

            yield return new WaitForSeconds(finalCollectionTime);
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Nectar, finalCollectionAmount);
            GameEvents.ReportResourceCollected(transform, finalCollectionAmount, TipoRecurso.Nectar);


            // --- 2. Processamento ---
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(_honeycombTarget.position)));
            float finalProcessingTime = baseProcessingTime * (_isInQueenAura ? _queenTimeMultiplier : 1f);
            yield return new WaitForSeconds(finalProcessingTime);

            // A abelha só processa o que ela mesma acabou de coletar
            if (GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Nectar, finalCollectionAmount))
            {
                // Não salvamos MelProcessado, então a abelha o "carrega" conceitualmente
            } 
            else 
            { 
                // Se o néctar não estiver lá (improvável neste fluxo), ela volta a coletar
                yield return new WaitForSeconds(1f); 
                continue; 
            }

            // --- 3. Depósito de Mel ---
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(_hiveTarget.position)));
            
            float finalDepositTime = baseDepositTime * (_isInQueenAura ? _queenTimeMultiplier : 1f);
            float finalProductionMultiplier = _currentProductionMultiplierFromUpgrades * (_isInQueenAura ? _queenAmountMultiplier : 1f);
            float melFinalAmount = finalCollectionAmount * finalProductionMultiplier;
            
            yield return new WaitForSeconds(finalDepositTime);
            // Adiciona o mel final diretamente. Não interagimos com o recurso MelProcessado.
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, melFinalAmount);
            GameEvents.ReportResourceCollected(transform, melFinalAmount, TipoRecurso.Mel);
            
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }

    private Vector3 GetRandomDestination(Vector3 basePosition)
    {
        Vector2 randomOffset = Random.insideUnitCircle * destinationOffsetRadius;
        Vector3 targetPos = basePosition + new Vector3(randomOffset.x, 0, randomOffset.y);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, destinationOffsetRadius * 2, NavMesh.AllAreas)) 
        { 
            return hit.position; 
        }
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