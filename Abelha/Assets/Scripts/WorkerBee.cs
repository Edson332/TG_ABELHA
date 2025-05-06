using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Adiciona a nova interface
[RequireComponent(typeof(NavMeshAgent))]
public class WorkerBee : MonoBehaviour, BeeStatsUpdater, IBoostableByQueen
{
    [Header("Identificação")]
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
    private float _effectiveCollectionAmount; // Quantidade base * upgrade
    private float _currentNectarMultiplierFromUpgrades = 1f; // Cache do multiplicador de upgrade
    private float _currentProductionMultiplierFromUpgrades = 1f; // Cache do multiplicador de upgrade

    // --- Queen Aura Fields ---
    private bool _isInQueenAura = false;
    private float _queenAmountMultiplier = 1f; // Multiplicador de quantidade da rainha (padrão 1)
    private float _queenTimeMultiplier = 1f;   // Multiplicador de tempo da rainha (padrão 1)
    // --- End Queen Aura Fields ---

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(beeTypeName)) {
             Debug.LogError($"Abelha {gameObject.name} sem beeTypeName!"); this.enabled = false; return;
        }
        agent.avoidancePriority = Random.Range(0, 100);
        StartCoroutine(StartWorker());
    }

    private void OnEnable()
    {
        if (GerenciadorUpgrades.Instancia != null && !string.IsNullOrEmpty(beeTypeName))
        {
            GerenciadorUpgrades.Instancia.RegistrarAbelha(this, beeTypeName);
            // Pega os multiplicadores de upgrade atuais ao habilitar
            UpdateUpgradeMultipliers();
        }
    }

    private void OnDisable()
    {
        if (GerenciadorUpgrades.Instancia != null && !string.IsNullOrEmpty(beeTypeName))
        {
            GerenciadorUpgrades.Instancia.DesregistrarAbelha(this, beeTypeName);
        }
         // Garante que sai da aura se for desabilitada
         ExitQueenAura();
    }

    // --- IBoostableByQueen Implementation ---
    public void EnterQueenAura(float amountMultiplier, float timeMultiplier)
    {
        _isInQueenAura = true;
        _queenAmountMultiplier = amountMultiplier;
        _queenTimeMultiplier = timeMultiplier;
         // Debug.Log($"{gameObject.name} entrou na aura. Boost: Qtd x{amountMultiplier}, Tempo x{timeMultiplier}");
    }

    public void ExitQueenAura()
    {
        _isInQueenAura = false;
        _queenAmountMultiplier = 1f; // Reseta para o padrão
        _queenTimeMultiplier = 1f;  // Reseta para o padrão
        // Debug.Log($"{gameObject.name} saiu da aura.");
    }
    // --- End IBoostableByQueen ---


    // --- BeeStatsUpdater Implementation ---
    public void AtualizarVelocidade(float multiplicador)
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.speed = baseSpeed * multiplicador;
    }
    // --- End BeeStatsUpdater ---


    /// <summary>
    /// Atualiza os multiplicadores vindos dos upgrades (chamado no OnEnable e após comprar upgrades).
    /// </summary>
    private void UpdateUpgradeMultipliers()
    {
         if (GerenciadorUpgrades.Instancia != null && !string.IsNullOrEmpty(beeTypeName))
         {
             _currentNectarMultiplierFromUpgrades = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.NectarColetado);
             _currentProductionMultiplierFromUpgrades = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.MelProduzido);
             // Poderia atualizar a velocidade aqui também se GerenciadorUpgrades não o fizesse no registro
         }
    }


    private IEnumerator StartWorker()
    {
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);
        yield return null; // Espera registro
        if (agent != null && agent.isOnNavMesh) {
             // Pega os multiplicadores iniciais ANTES de iniciar o loop
             UpdateUpgradeMultipliers();
             StartCoroutine(WorkerRoutine());
        } else {
             Debug.LogError($"WorkerBee {gameObject.name} ({beeTypeName}) não iniciou rotina. Agent: {agent}, IsOnNavMesh: {agent?.isOnNavMesh}");
        }
    }


    private IEnumerator WorkerRoutine()
    {
        while (true)
        {
            // --- Mover cálculos de quantidade/tempo base para mais perto da ação ---
            // float nectarMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.NectarColetado); // Obter multiplicadores de upgrade
            // float productionMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(beeTypeName, TipoUpgrade.MelProduzido);

            // Pega os multiplicadores de upgrade atuais (pode ser feito uma vez no início do loop se preferir)
             UpdateUpgradeMultipliers(); // Função que atualiza _currentNectarMultiplierFromUpgrades etc.

            // 1. Mover para a Flor
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(flower.position)));

            // --- CHEGOU NA FLOR: Calcular AGORA para a coleta ---
            _effectiveCollectionAmount = baseCollectionAmount * _currentNectarMultiplierFromUpgrades;
            float finalCollectionAmount = _effectiveCollectionAmount * (_isInQueenAura ? _queenAmountMultiplier : 1f);
            float finalCollectionTime = baseCollectionTime * (_isInQueenAura ? _queenTimeMultiplier : 1f);
            // --- Fim do cálculo para coleta ---

            // Debug.Log($"{gameObject.name} - Chegou na flor. Calculando Coleta. InAura: {_isInQueenAura}, TempoFinal: {finalCollectionTime}, QtdFinal: {finalCollectionAmount}");

            // 2. Esperar Coleta
            yield return new WaitForSeconds(finalCollectionTime);

            // 3. Adicionar Recurso
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Nectar, finalCollectionAmount);


            // --- Processamento e Depósito ---

            // 4. Mover para o Honeycomb
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(honeycomb.position)));

            // --- CHEGOU NO HONEYCOMB: Calcular AGORA para processamento (se a aura afetar) ---
            float finalProcessingTime = baseProcessingTime * (_isInQueenAura ? _queenTimeMultiplier : 1f);
            // --- Fim do cálculo para processamento ---

            // 5. Esperar Processamento
            yield return new WaitForSeconds(finalProcessingTime);
            if (GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Nectar, finalCollectionAmount)) // Usa a QTD que foi coletada
            {
                GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.MelProcessado, finalCollectionAmount); // Adiciona a mesma QTD como processado
            } else { yield return new WaitForSeconds(1f); continue; }


            // 6. Mover para a Hive
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(hive.position)));

            // --- CHEGOU NA HIVE: Calcular AGORA para depósito (se a aura afetar) ---
            float finalDepositTime = baseDepositTime * (_isInQueenAura ? _queenTimeMultiplier : 1f);
            // Calcula o bônus de produção final (upgrade + possível boost de quantidade da rainha)
             float finalProductionMultiplier = _currentProductionMultiplierFromUpgrades * (_isInQueenAura ? _queenAmountMultiplier : 1f); // Exemplo: Rainha tbm aumenta produção final
             float melFinalAmount = finalCollectionAmount * finalProductionMultiplier; // Aplica bônus de produção sobre o que foi coletado/processado
            // --- Fim do cálculo para depósito ---

            // 7. Esperar Depósito
            yield return new WaitForSeconds(finalDepositTime);
            if(GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.MelProcessado, finalCollectionAmount)) // Remove o processado
            {
                 GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, melFinalAmount); // Adiciona o mel final calculado
            } else { yield return new WaitForSeconds(1f); continue; }

            // Pausa antes do próximo ciclo
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