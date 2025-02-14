using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : MonoBehaviour
{
    public Transform targetPosition; // Defina o ponto de destino no Inspector
    private Vector3 startPosition;
    private NavMeshAgent agent;
    private bool goingToTarget = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        agent.SetDestination(targetPosition.position);
    }

    void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            goingToTarget = !goingToTarget;
            agent.SetDestination(goingToTarget ? targetPosition.position : startPosition);
        }
    }
}
