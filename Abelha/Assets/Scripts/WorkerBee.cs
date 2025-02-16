using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WorkerBee : MonoBehaviour
{
    public Transform flower;
    public Transform honeycomb;
    public Transform hive;

    public float collectionTime = 2f;

    private NavMeshAgent agent;
    private enum BeeState { GoingToFlower, Collecting, GoingToHoneycomb, ProducingHoney, GoingToHive, Depositing }
    private BeeState currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToFlower();
    }

    void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            switch (currentState)
            {
                case BeeState.GoingToFlower:
                    StartCoroutine(CollectNectar());
                    break;
                case BeeState.GoingToHoneycomb:
                    StartCoroutine(ProduceHoney());
                    break;
                case BeeState.GoingToHive:
                    StartCoroutine(DepositHoney());
                    break;
            }
        }
    }

    void GoToFlower()
    {
        currentState = BeeState.GoingToFlower;
        agent.SetDestination(flower.position);
    }

    IEnumerator CollectNectar()
    {
        currentState = BeeState.Collecting;
        yield return new WaitForSeconds(collectionTime);
        ResourceManager.Instance.AddNectar(1);
        GoToHoneycomb();
    }

    void GoToHoneycomb()
    {
        currentState = BeeState.GoingToHoneycomb;
        agent.SetDestination(honeycomb.position);
    }

    IEnumerator ProduceHoney()
    {
        currentState = BeeState.ProducingHoney;
        yield return new WaitForSeconds(collectionTime);
        ResourceManager.Instance.ConvertNectarToHoney();
        
        // Agora o mel produzido pela abelha é diretamente contabilizado no carriedHoney
        ResourceManager.Instance.carriedHoney += ResourceManager.Instance.honeyPerCycle;

        GoToHive();
    }

    void GoToHive()
    {
        currentState = BeeState.GoingToHive;
        agent.SetDestination(hive.position);
    }

    IEnumerator DepositHoney()
    {
        currentState = BeeState.Depositing;
        yield return new WaitForSeconds(collectionTime);
        
        // Total de mel carregado para depósito
        int totalCarried = ResourceManager.Instance.carriedHoney;
        int depositAmount = Mathf.Min(totalCarried, ResourceManager.Instance.beeCarryLimit);

        // Deposita no StoredHoney
        ResourceManager.Instance.storedHoney += depositAmount;
        ResourceManager.Instance.carriedHoney -= depositAmount; // Remove o mel depositado

        // Se ainda houver mel, faça outra viagem à colmeia
        if (ResourceManager.Instance.carriedHoney > 0)
        {
            GoToHive();
        }
        else
        {
            GoToFlower();
        }
    }
}
