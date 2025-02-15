using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCCollector : MonoBehaviour
{
    public Transform resourceTarget; // Objeto onde o NPC coletará o recurso
    public Transform homeBase; // Ponto de origem, pode ser alterado dinamicamente
    public float collectionTime = 2f; // Tempo para coletar o recurso

    private NavMeshAgent agent;
    private bool collecting = false;
    private bool returningHome = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToResource();
    }

    void Update()
    {
        if (!collecting && !returningHome && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            StartCoroutine(CollectResource());
        }
        else if (returningHome && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            returningHome = false;
            GoToResource(); // Após chegar em casa, voltar a coletar
        }
    }

    void GoToResource()
    {
        if (resourceTarget != null)
        {
            agent.SetDestination(resourceTarget.position);
        }
    }

    IEnumerator CollectResource()
    {
        collecting = true;
        yield return new WaitForSeconds(collectionTime); // Simula o tempo de coleta
        collecting = false;
        ReturnHome();
    }

    void ReturnHome()
    {
        if (homeBase != null)
        {
            returningHome = true;
            agent.SetDestination(homeBase.position);
        }
    }

    public void SetNewHomeBase(Transform newHome)
    {
        homeBase = newHome;
    }

    public void SetNewResourceTarget(Transform newTarget)
    {
        resourceTarget = newTarget;
    }
}
