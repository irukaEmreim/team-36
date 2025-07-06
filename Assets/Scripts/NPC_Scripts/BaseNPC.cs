using UnityEngine;
using UnityEngine.AI;
using Microlight.MicroBar;
using System.Collections;

public abstract class BaseNPC : MonoBehaviour
{
    
    protected Animator animator;
    protected NavMeshAgent agent;
    public MicroBar stressBar;
    public float runSpeed = 5f;
    public float normalSpeed = 1.5f;

    protected float maxStress = 100f;
    protected float currentStress;

    protected bool isReacting = false;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        currentStress = maxStress;

        // 🔍 Eğer inspector'da atanmadıysa otomatik bul
        if (stressBar == null)
            stressBar = GetComponentInChildren<MicroBar>();

        if (stressBar != null)
        {
            stressBar.Initialize(maxStress);
            stressBar.UpdateBar(currentStress);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} için stressBar atanmamış veya bulunamadı!");
        }
        
        if (stressBar == null)
        {
            Debug.LogWarning($"{gameObject.name} için stressBar atanamadı!");
        }
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{gameObject.name} NavMesh üstünde değil!");
        }
        Debug.Log($"{gameObject.name} → Speed: {agent.speed}, isStopped: {agent.isStopped}, hasPath: {agent.hasPath}");



    }



    protected virtual void Update()
    {
    }
    protected IEnumerator SimpleReact()
    {
        isReacting = true;
        StopAllAnimations();

        agent.ResetPath();
        agent.isStopped = false;
        agent.speed = runSpeed;

        animator.SetBool("isRunning", true);

        int maxTries = 5;
        bool validTargetFound = false;
        Vector3 runTarget = Vector3.zero;

        for (int i = 0; i < maxTries; i++)
        {
            runTarget = GetRandomNavmeshPoint();

            if (TryGetValidPath(out runTarget))
            {
                agent.SetDestination(runTarget);
                Debug.Log($"{gameObject.name} ✅ Hedef → {runTarget}");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} ❌ Geçerli path oluşturulamadı.");
            }

            yield return null; // bir frame bekle ki agent hasPath güncellensin

            if (agent.hasPath)
            {
                validTargetFound = true;
                Debug.Log($"{gameObject.name} ✅ Hedef → {runTarget}");
                break;
            }
        }

        if (!validTargetFound)
        {
            Debug.LogWarning($"{gameObject.name} ❌ Uygun hedef bulunamadı, olduğu yerde kalıyor.");
        }

        yield return new WaitForSeconds(2f);

        animator.SetBool("isRunning", false);
        agent.ResetPath();
        agent.speed = normalSpeed;

        animator.SetBool("isYelling", true);
        yield return new WaitForSeconds(2f);
        animator.SetBool("isYelling", false);

        isReacting = false;
    }
    
    bool TryGetValidPath(out Vector3 result)
    {
        int attempts = 10;
        NavMeshPath path = new NavMeshPath();

        for (int i = 0; i < attempts; i++)
        {
            Vector3 candidate = GetRandomNavmeshPoint();

            if (NavMesh.CalculatePath(agent.transform.position, candidate, NavMesh.AllAreas, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    result = candidate;
                    return true;
                }
            }
        }

        result = agent.transform.position; // fallback
        return false;
    }



    protected bool TryGetSafeNavmeshTarget(out Vector3 result, float radius = 10f)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * radius;
            randomDir.y = 0;
            Vector3 candidate = transform.position + randomDir;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = transform.position;
        return false;
    }



    protected Vector3 GetRandomNavmeshPoint(float roamRadius = 10f)
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



    public virtual void TakeDamage(float amount)
    {
        currentStress -= amount;
        currentStress = Mathf.Clamp(currentStress, 0, maxStress);

        if (stressBar != null)
            stressBar.UpdateBar(currentStress);

        if (!isReacting)
        {
            StartCoroutine(SimpleReact());  // 👈 Eksik olan buydu
        }
    }




    protected void StopAllAnimations()
    {
        animator.SetBool("DoDance", false);
        animator.SetBool("DoDance2", false);
        animator.SetBool("DoSelfCheck", false);
        animator.SetBool("DoPhoneTalk", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isYelling", false);

        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }
}