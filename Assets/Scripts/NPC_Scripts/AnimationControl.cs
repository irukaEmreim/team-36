using UnityEngine;
using UnityEngine.AI;
using Microlight.MicroBar;

public class AnimationControl : BaseNPC
{
    [HideInInspector]
    public bool isExternallyControlled = false;

    public float roamRadius = 10f;
    public float actionDuration = 5f;

    private enum ActionPhase { Walk, Dance, SelfCheck }
    private ActionPhase currentPhase = ActionPhase.Walk;


    private float timer = 0f;
    private bool isActing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        base.Start(); // BaseNPC setup
        if (GetComponent<Guest>() == null && GetComponent<HotelEmployee>() == null)
        {
            if (Random.value < 0.2f)
                gameObject.AddComponent<HotelEmployee>();
            else
                gameObject.AddComponent<Guest>();
        }
        // 🔁 Eğer atanmamışsa, kendisi bulsun
        if (stressBar == null)
            stressBar = GetComponentInChildren<MicroBar>();

        currentStress = maxStress;

        if (stressBar != null)
        {
            stressBar.Initialize(maxStress);
            stressBar.UpdateBar(currentStress);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} için stressBar bulunamadı!");
        }

        StartNextAction();
    }



    void Update()
    {
        if (isReacting || isExternallyControlled) return;

        if (!isActing) return;

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
        if (isReacting || isExternallyControlled) return;

        isActing = true;

        switch (currentPhase)
        {
            case ActionPhase.Walk:
                Vector3 dest = GetRandomNavmeshPoint();
                agent.isStopped = false; // 💥 Bu da önemli
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
            Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
            randomDirection.y = 0;
            Vector3 candidate = transform.position + randomDirection;

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

        agent.isStopped = false;  // 💥 Bu kritik!
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

}
