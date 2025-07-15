using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class NPC_Movement : MonoBehaviour
{

    private NavMeshAgent agent;
    private NPC_Base npc;
    private NPC_AnimationHandler animationHandler;
    private Animator animator;  
    public float normalSpeed = 3.5f;
    public float runSpeed = 8f;

    void Awake()
    {
        animationHandler = GetComponent<NPC_AnimationHandler>();
        agent = GetComponent<NavMeshAgent>();
        npc = GetComponent<NPC_Base>();
        animator = GetComponent<Animator>();
    }

    public void MoveTo(Vector3 destination, bool isRunning = false)
    {
        agent.speed = isRunning ? runSpeed : normalSpeed;
        agent.isStopped = false;
        agent.SetDestination(destination);
        npc.currentState = isRunning ? NPC_Base.NPCState.Running : NPC_Base.NPCState.Walking;
    }

    public void Stop()
    {
        agent.isStopped = true;
        agent.ResetPath();
        npc.currentState = NPC_Base.NPCState.Idle;
    }

    public bool HasReachedDestination()
    {
        return !agent.pathPending && agent.remainingDistance < 0.5f;
    }


    public Vector3 GetRandomPoint(float minDistace = 5f, float maxDistance = 15f)
    {
        for (int i = 0; i < 10; i++)
        {
            float randomRadius = Random.Range(minDistace, maxDistance);
            Vector3 randomDir = Random.insideUnitSphere * randomRadius;
            randomDir.y = 0;
 
            Vector3 candidate = transform.position + randomDir;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, randomRadius, NavMesh.AllAreas))
                return hit.position;
        }

        return transform.position;
    }

public Vector3 GetSafePointAwayFrom(Vector3 origin, float minDistance = 8f, float maxDistance = 20f)
{
    for (int i = 0; i < 20; i++)
    {
        float randomRadius = Random.Range(minDistance, maxDistance);
        Vector3 randomDir = Random.insideUnitSphere * randomRadius;
        randomDir.y = 0;

        Vector3 candidate = transform.position + randomDir;

        float distanceToOrigin = Vector3.Distance(candidate, origin);

        // Origin'den uzaklaşmak için mesafe kontrolü
        if (distanceToOrigin < minDistance)
            continue;

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            return hit.position;
    }

    return transform.position;
}


}
