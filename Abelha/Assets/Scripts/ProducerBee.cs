using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ProducerBee : MonoBehaviour
{
    [Header("Locais de Destino")]
    public Transform flower;     // Usado para coletar nectar se necessário
    public Transform honeycomb;  // Local de processamento do nectar em mel processado
    public Transform hive;       // Local de depósito do mel final

    [Header("Parâmetros de Tempo")]
    public float productionTime = 2f;   // Tempo para processar o nectar no honeycomb
    public float depositTime = 2f;      // Tempo para depositar o mel na colmeia
    public float collectionTime = 2f;   // Tempo para coletar nectar na flor (se necessário)

    [Header("Capacidade de Produção")]
    public float productionAmount = 1f; // Quantidade de nectar processada por ciclo

    [Header("Configuração Adicional")]
    public float initialDelay = 0f;           // Atraso inicial para diferenciar o ciclo
    public float destinationOffsetRadius = 1f;  // Raio para posicionamento aleatório próximo aos destinos

    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Ajusta a prioridade de evitação para minimizar colisões com WorkerBees
        agent.avoidancePriority = Random.Range(0, 100);
        StartCoroutine(StartProducer());
    }

    private IEnumerator StartProducer()
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);
        yield return StartCoroutine(ProducerRoutine());
    }

    private IEnumerator ProducerRoutine()
    {
        while (true)
        {
            // 1. Verifica se há nectar disponível na reserva global para produção
            float availableNectar = GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Nectar);
            if (availableNectar >= productionAmount)
            {
                // Vai até o honeycomb para processar o nectar
                yield return StartCoroutine(MoveToTarget(GetRandomDestination(honeycomb.position)));
                Debug.Log("ProducerBee: Processando nectar em mel processado...");
                yield return new WaitForSeconds(productionTime);
                // Processa o nectar: remove do recurso global e adiciona mel processado
                GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Nectar, productionAmount);
                GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.MelProcessado, productionAmount);

                // Vai até a colmeia para depositar o mel processado
                yield return StartCoroutine(MoveToTarget(GetRandomDestination(hive.position)));
                Debug.Log("ProducerBee: Depositando mel na colmeia...");
                yield return new WaitForSeconds(depositTime);
                GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.MelProcessado, productionAmount);
                GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, productionAmount);
            }
            else
            {
                // Se não houver nectar suficiente, vai até a flor para coletar nectar.
                // Isso ocorre somente se a reserva global estiver abaixo do necessário,
                // garantindo que não roube nectar das WorkerBees.
                yield return StartCoroutine(MoveToTarget(GetRandomDestination(flower.position)));
                Debug.Log("ProducerBee: Coletando nectar (reserva insuficiente)...");
                yield return new WaitForSeconds(collectionTime);
                // Antes de coletar, verifica novamente se o recurso continua insuficiente
                availableNectar = GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Nectar);
                if (availableNectar < productionAmount)
                {
                    GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Nectar, productionAmount);
                }
            }

            // Pequena pausa antes de iniciar o próximo ciclo
            yield return new WaitForSeconds(1f);
        }
    }

    // Gera um destino aleatório próximo à posição base para evitar que várias abelhas se sobreponham
    private Vector3 GetRandomDestination(Vector3 basePosition)
    {
        Vector2 randomOffset = Random.insideUnitCircle * destinationOffsetRadius;
        return basePosition + new Vector3(randomOffset.x, 0, randomOffset.y);
    }

    // Move a abelha até o destino usando o NavMeshAgent
    private IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
    }
}
