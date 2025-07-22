using UnityEngine;
using UnityEngine.AI;

public class AnimationControl : MonoBehaviour
{
    [HideInInspector] public bool isExternallyControlled = false;

    public float roamRadius = 10f;
    public float actionDuration = 5f;

    private NavMeshAgent agent;
    private Animator animator;

    private enum ActionPhase { Walk, Dance, SelfCheck }
    private ActionPhase currentPhase = ActionPhase.Walk;

    private float timer = 0f;
    private bool isActing = false;
    private bool isReacting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isReacting || isExternallyControlled) return;
        if (!isActing) return;

        timer -= Time.deltaTime;

        if (currentPhase == ActionPhase.Walk)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    agent.ResetPath();
                    animator.SetBool("isWalking", false);
                }
            }
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
        if (isReacting || isExternallyControlled) return;
        isActing = true;

        switch (currentPhase)
        {
            case ActionPhase.Walk:
                Vector3 dest = GetRandomNavmeshPoint();
                agent.isStopped = false;
                if (agent.isOnNavMesh)
                {
                    agent.SetDestination(dest);
                }
                else
                {
                    Debug.LogWarning($"{name} → AnimationControl → agent NavMesh'te değil, hedef atanamadı!");
                }

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
        StopAllAnimations();
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    void MoveToNextPhase()
    {
        currentPhase = (ActionPhase)(((int)currentPhase + 1) % 3);
    }

    Vector3 GetRandomNavmeshPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 random = Random.insideUnitSphere * roamRadius;
            random.y = 0;
            Vector3 candidate = transform.position + random;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
                return hit.position;
        }

        return transform.position;
    }

    public void StopAllAnimations()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("DoDance", false);
        animator.SetBool("DoSelfCheck", false);

        agent.isStopped = false;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }
}
