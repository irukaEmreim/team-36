
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Microlight.MicroBar;

public enum NPCReactionType { Flee, ChaseAndThrow }
public static class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public abstract class BaseNPC : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected Animator animator;
    protected GameObject currentTarget;
    protected float currentStress = 100f;
    protected float maxStress = 100f;
    protected Microlight.MicroBar.MicroBar stressBar;

    protected float runSpeed = 4.5f;
    protected float normalSpeed = 1.5f;
    protected bool isReacting = false;

    protected virtual void Start()
    {
        
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.autoBraking = false;
        agent.angularSpeed = 1200f;
        agent.acceleration = 40f;

        if (stressBar == null)
            stressBar = GetComponentInChildren<MicroBar>();

        if (stressBar != null)
        {
            stressBar.Initialize(maxStress);
            stressBar.UpdateBar(currentStress);
        }
        StartCoroutine(WaitUntilNavReady());
    }
    private IEnumerator WaitUntilNavReady()
    {
        int attempts = 0;
        while (!agent.isOnNavMesh && attempts < 50)
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{name} → 5 saniye geçti ama hâlâ NavMesh'te değil! Agent devre dışı bırakılıyor.");
            agent.enabled = false;
            yield break; // 💥 coroutine’i bitir
        }
        else
        {
            Debug.Log($"{name} → Artık NavMesh üzerinde. Hazır!");
            yield return new WaitForSeconds(0.5f); // Sabitleşme süresi
        }

    }
    public void StartRoaming()
    {
        StartCoroutine(WaitForNavMeshThenRoam());
    }

    private IEnumerator WaitForNavMeshThenRoam()
    {
        int tries = 0;
        while (!agent.isOnNavMesh && tries < 50)
        {
            yield return new WaitForSeconds(0.1f);
            tries++;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name} → StartRoaming: NavMesh'e oturamadı!");
            yield break;
        }
        else
        {
            Debug.Log($"{name} → StartRoaming: NavMesh hazır, random roam başlatılıyor.");
            yield return null; // ✅ Buraya ekle
        }
        StartCoroutine(RandomRoamForSeconds(60));



        Debug.Log($"{name} → StartRoaming: NavMesh hazır, random roam başlatılıyor.");
        StartCoroutine(RandomRoamForSeconds(60));
    }


    protected virtual void Update()
    {
        if (isChasingCrowForJewelry)
        {
            Debug.Log($"{name} → Takip hâlâ devam ediyor mu?");
            if (!IsJewelryStillStolen())
            {
                Debug.Log($"{name} → Takı artık bende değilmiş! Takibi bırakıyor.");
                StopChasingCrow();
            }
        }
        if (IsStuck())
            Debug.LogWarning($"{name} → NPC sıkışmış gibi gözüküyor!");

    }

    public virtual IEnumerator RandomRoamForSeconds(float duration)
    {
        Debug.LogWarning($"{name} → RandomRoamForSeconds() bu NPC türünde tanımlı değil.");
        yield break;
    }

    public virtual void TakeDamage(float amount)
    {
        currentStress -= amount;
        currentStress = Mathf.Clamp(currentStress, 0, 100f);

        if (stressBar != null)
            stressBar.UpdateBar(currentStress);

        if (isReacting) return;

        isReacting = true;
        StopAllAnimations();

        switch (GetReactionType())
        {
            case NPCReactionType.Flee:
                StartCoroutine(FleeThenYell());
                break;
            case NPCReactionType.ChaseAndThrow:
                StartCoroutine(ChaseThenThrow());
                break;
        }
    }
    
    

    
    protected IEnumerator FleeThenYell()
    {
        animator.SetBool("isRunning", true);
        agent.speed = (currentStress < maxStress * 0.5f) ? runSpeed * 2f : runSpeed;
        Vector3 target = GetRandomNavmeshPoint(15f, 7f);
        agent.SetDestination(target);

        float timeElapsed = 0f;
        float maxChaseTime = 3f;

        while (timeElapsed < maxChaseTime)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
                break;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        agent.ResetPath();
        animator.SetBool("isRunning", false);

        // ❗ "isYelling" parametresi varsa bağır, yoksa geç
        if (animator.HasParameter("isYelling"))
        {
            animator.SetBool("isYelling", true);
            yield return new WaitForSeconds(2f);
            animator.SetBool("isYelling", false);
        }

        isReacting = false;
    }


    protected IEnumerator ChaseThenThrow()
    {
        GameObject crow = GameObject.FindGameObjectWithTag("lb_bird");
        if (crow == null)
        {
            isReacting = false;
            yield break;
        }

        currentTarget = crow;
        agent.speed = runSpeed;
        animator.SetBool("isRunning", true);

        float chaseTime = 4f;
        float t = 0f;

        while (t < chaseTime)
        {
            agent.SetDestination(currentTarget.transform.position);
            t += Time.deltaTime;
            yield return null;
        }

        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

      

        // ✅ ÖNCE animasyon bool açık
        animator.SetBool("throw", true);

        // ⏱ Gerçek anim süresi kadar bekle
        yield return new WaitForSeconds(4f);

        // ✅ KAPAT
        animator.SetBool("throw", false);

        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;

        currentTarget = null;
        isReacting = false;
    }


  

    protected void FaceTarget(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 15f); // daha hızlı dönsün
        }
    }

    protected bool IsStuck()
    {
        return !agent.pathPending && !agent.hasPath && agent.remainingDistance == Mathf.Infinity;
    }


    protected Vector3 GetRandomNavmeshPoint(float maxRadius = 20f, float minDistance = 5f)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 random = Random.insideUnitSphere * maxRadius;
            random.y = 0;
            Vector3 candidate = transform.position + random;

            if (Vector3.Distance(transform.position, candidate) < minDistance)
                continue;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, maxRadius, NavMesh.AllAreas))
            {
                if (Vector3.Angle(Vector3.up, hit.normal) > 35f)
                    continue;

                // ✅ PATH HESAPLAMA KISMI
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    return hit.position;
                }
                else
                {
                    Debug.LogWarning($"{name} → path.status = {path.status}, path çizilemedi!");
                }

            }
        }

        Debug.LogWarning($"{name} → GetRandomNavmeshPoint başarısız oldu. Yakına fallback hedef atanıyor.");
        return transform.position + transform.forward * 1.5f; // 1.5 birim ileri git

    }



    protected void StopAllAnimations()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);

        if (animator.HasParameter("isYelling"))
            animator.SetBool("isYelling", false);

        if (animator.HasParameter("DoDance"))
            animator.SetBool("DoDance", false);

        if (animator.HasParameter("DoSelfCheck"))
            animator.SetBool("DoSelfCheck", false);

        agent.ResetPath();
    }



    protected abstract NPCReactionType GetReactionType();

    protected bool IsJewelryStillStolen()
    {
        GameObject crow = GameObject.FindGameObjectWithTag("lb_bird");
        if (crow == null) return false;

        CrowCollect cc = crow.GetComponent<CrowCollect>();
        if (cc == null || cc.collectedDiamond == null) return false;

        return IsMyDiamondStolen(cc.collectedDiamond);
    }

    protected bool IsMyDiamondStolen(GameObject diamond)
    {
        return diamond != null && diamond.name.ToLower().Contains("diamond") && !diamond.transform.IsChildOf(transform);
    }

    public virtual void StopChasingCrow()
    {
        if (!isChasingCrowForJewelry) return;

        Debug.Log($"{gameObject.name} → Takı bırakıldı, kovalamayı bırakıyor.");
        isChasingCrowForJewelry = false;

        if (jewelryChaseRoutine != null)
            StopCoroutine(jewelryChaseRoutine);
        jewelryChaseRoutine = null;

        animator.SetBool("isRunning", false);
        animator.SetBool("throw", false);
        currentTarget = null;

        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.Warp(transform.position);

        StartCoroutine(ResumeRoamAfterCooldown());
    }

    protected virtual IEnumerator ResumeRoamAfterCooldown()
    {
        yield break;
    }

    protected bool isChasingCrowForJewelry = false;
    protected Coroutine jewelryChaseRoutine;

    public virtual void OnJewelryStolen()
    {
        if (isChasingCrowForJewelry) return;

        Debug.Log($"{name} → Takısı çalındı! Kargayı kovalamaya başlıyor.");

        isReacting = false;
        StopAllAnimations();

        isChasingCrowForJewelry = true;
        if (jewelryChaseRoutine != null)
            StopCoroutine(jewelryChaseRoutine);
        jewelryChaseRoutine = StartCoroutine(JewelryChase());
    }

    protected virtual IEnumerator JewelryChase()
    {
        GameObject crow = GameObject.FindGameObjectWithTag("lb_bird");
        if (crow == null) yield break;

        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;

        currentTarget = crow;
        agent.speed = runSpeed * 3f;
        animator.SetBool("isRunning", true);

        while (isChasingCrowForJewelry && crow != null)
        {
            agent.SetDestination(crow.transform.position);
            FaceTarget(crow);

            currentStress -= 10f;
            currentStress = Mathf.Clamp(currentStress, 0, 100f);
            if (stressBar != null) stressBar.UpdateBar(currentStress);

            yield return new WaitForSeconds(3f);
        }

        animator.SetBool("isRunning", false);
        currentTarget = null;
        agent.ResetPath();
        isChasingCrowForJewelry = false;
    }
}
