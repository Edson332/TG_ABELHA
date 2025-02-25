using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WorkerBee : MonoBehaviour
{
    [Header("Locais de Destino")]
    public Transform flower;     // Local de coleta de nectar
    public Transform honeycomb;  // Local de processamento (transforma nectar em mel processado)
    public Transform hive;       // Local de depósito (converte mel processado em mel final)

    [Header("Parâmetros de Tempo")]
    public float collectionTime = 2f;   // Tempo para coletar o nectar na flor
    public float processingTime = 2f;   // Tempo para processar o nectar em mel processado
    public float depositTime = 2f;      // Tempo para depositar o mel na colmeia

    [Header("Capacidade de Carga")]
    public float collectionAmount = 1f; // Quantidade de nectar coletada por ciclo

    [Header("Configuração Adicional")]
    public float initialDelay = 0f;           // Atraso inicial para diferenciar o ciclo das abelhas
    public float destinationOffsetRadius = 1f;  // Raio para posicionamento aleatório próximo ao destino

    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Opcional: ajustar a prioridade de evitação para cada abelha (valor entre 0 e 99; quanto menor, mais "forte" ela é)
        agent.avoidancePriority = Random.Range(0, 100);
        StartCoroutine(StartWorker());
    }

    private IEnumerator StartWorker()
    {
        // Aguarda um tempo inicial para evitar que todas iniciem ao mesmo tempo
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        yield return StartCoroutine(WorkerRoutine());
    }

    private IEnumerator WorkerRoutine()
    {
        while (true)
        {
            // 1. Ir até a flor para coletar o nectar com um pequeno offset
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(flower.position)));
            Debug.Log("Coletando nectar...");
            yield return new WaitForSeconds(collectionTime);
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Nectar, collectionAmount);

            // 2. Ir até o honeycomb para processar o nectar
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(honeycomb.position)));
            Debug.Log("Processando nectar em mel processado...");
            yield return new WaitForSeconds(processingTime);
            GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Nectar, collectionAmount);
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.MelProcessado, collectionAmount);

            // 3. Ir até a colmeia para depositar o mel processado
            yield return StartCoroutine(MoveToTarget(GetRandomDestination(hive.position)));
            Debug.Log("Depositando mel na colmeia...");
            yield return new WaitForSeconds(depositTime);
            GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.MelProcessado, collectionAmount);
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, collectionAmount);

            // Pequena pausa antes de reiniciar o ciclo
            yield return new WaitForSeconds(1f);
        }
    }

    // Retorna um destino aleatório próximo ao ponto base, dentro do raio definido
    private Vector3 GetRandomDestination(Vector3 basePosition)
    {
        Vector2 randomOffset = Random.insideUnitCircle * destinationOffsetRadius;
        // Mantém a mesma altura (y) do destino base
        return basePosition + new Vector3(randomOffset.x, 0, randomOffset.y);
    }

    // Utiliza o NavMeshAgent para mover o agente até o destino
    private IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
    }
}
