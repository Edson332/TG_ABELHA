using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WorkerBee : MonoBehaviour
{
    public Transform flower;     // Onde coleta o néctar
    public Transform honeycomb;  // Onde fabrica o mel
    public Transform hive;       // Onde deposita o mel

    public float collectionTime = 2f;
    public int nectarPerTrip = 1;
    public int honeyPerTrip = 1;

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
        ResourceManager.Instance.AddNectar(nectarPerTrip);
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
        ResourceManager.Instance.ConvertNectarToHoney(honeyPerTrip);
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
        ResourceManager.Instance.DepositHoney(honeyPerTrip);
        GoToFlower();
    }
}
