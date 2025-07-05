using UnityEngine;
using UnityEngine.AI;

public class AnimationControl : MonoBehaviour
{
    public float roamRadius = 10f;
    public float actionDuration = 5f;

    private NavMeshAgent agent;
    private Animator animator;
    [HideInInspector]
    public bool isExternallyControlled = false;

    private enum ActionPhase { Walk, Dance, SelfCheck }
    private ActionPhase currentPhase = ActionPhase.Walk;

    private float timer = 0f;
    private bool isActing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        StartNextAction();
    }

    void Update()
    {
        if (!isActing)
            return;

        timer -= Time.deltaTime;

        if (currentPhase == ActionPhase.Walk && (!agent.hasPath || agent.remainingDistance < 0.5f))
        {
            agent.ResetPath();
            animator.SetBool("isWalking", false);
        }

        if (timer <= 0f)
        {
            EndCurrentAction();
            MoveToNextPhase();
            StartNextAction();
        }
    }

    void StartNextAction()
    {
        isActing = true;

        switch (currentPhase)
        {
            case ActionPhase.Walk:
                Vector3 dest = GetRandomNavmeshPoint();
                agent.SetDestination(dest);
                agent.speed = 1.5f;
                animator.SetBool("isWalking", true);
                break;

            case ActionPhase.Dance:
                animator.SetBool("DoDance", true);
                break;

            case ActionPhase.SelfCheck:
                animator.SetBool("DoSelfCheck", true);
                break;
        }

        timer = actionDuration;
    }

    void EndCurrentAction()
    {
        isActing = false;

        animator.SetBool("isWalking", false);
        animator.SetBool("DoDance", false);
        animator.SetBool("DoSelfCheck", false);
    }

    void MoveToNextPhase()
    {
        currentPhase = (ActionPhase)(((int)currentPhase + 1) % 3); // Walk → Dance → SelfCheck → Walk...
    }

    Vector3 GetRandomNavmeshPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
            randomDirection.y = 0;
            Vector3 candidate = transform.position + randomDirection;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
                return hit.position;
        }

        return transform.position; // fallback
    }
}
