using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Microlight.MicroBar;

public enum NPCReactionType { Flee, ChaseAndThrow }

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
        
        if (stressBar == null)
            stressBar = GetComponentInChildren<MicroBar>();

        if (stressBar != null)
        {
            stressBar.Initialize(maxStress);
            stressBar.UpdateBar(currentStress);
        }

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        if (currentTarget != null)
            FaceTarget(currentTarget);

        if (isChasingCrowForJewelry)
        {
            Debug.Log($"{name} → Takip hâlâ devam ediyor mu?"); // 🔍 EKLE

            if (!IsJewelryStillStolen())
            {
                Debug.Log($"{name} → Takı artık bende değilmiş! Takibi bırakıyor."); // 🔍 EKLE
                StopChasingCrow();
            }
        }
    }


    public virtual void TakeDamage(float amount)
    {
        // 🔧 STRESS AZALT
        currentStress -= amount;
        currentStress = Mathf.Clamp(currentStress, 0, 100f);

        if (stressBar != null)
            stressBar.UpdateBar(currentStress);

        // 🔒 REACT EDİYORSA DUR
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
        Vector3 target = GetRandomNavmeshPoint(15f, 7f); // Daha uzak hedef
        agent.SetDestination(target);

        float timeElapsed = 0f;
        float maxChaseTime = 3f;

        while (timeElapsed < maxChaseTime)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
                break; // hedefe ulaştıysa erken bitir

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        agent.ResetPath();
        animator.SetBool("isRunning", false);

        animator.SetBool("isYelling", true);
        yield return new WaitForSeconds(2f);
        animator.SetBool("isYelling", false);

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
            FaceTarget(currentTarget);
            t += Time.deltaTime;
            yield return null;
        }

        // 🔒 DURDUR
        // ⛔ HER ŞEYİ DURDUR
        agent.ResetPath();                  // rotayı iptal et
        agent.velocity = Vector3.zero;     // hareketi kes
        agent.isStopped = true;            // agent'ı durdur
        agent.updatePosition = false;      // pozisyon güncellenmesin
        agent.updateRotation = false;      // rotation da NPC'den gelsin

        animator.SetBool("isRunning", false);

// 🧍 Sabit dursun ama sana baksın
        FaceTarget(currentTarget);

// 🧱 Pozisyonu sabitle (yüksek hassasiyet için)
        transform.position = agent.transform.position;

// 🪨 THROW ANİMASYONU
        animator.SetBool("throw", true);
        yield return new WaitForSeconds(1f);
        animator.SetBool("throw", false);

// 🔓 Tekrar kontrolü aç
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;


        currentTarget = null;
        isReacting = false;
    }

    protected IEnumerator ThrowOnceThenRun()
    {
        agent.isStopped = true;               // 🔒 Hareketi durdur
        animator.SetBool("throw", true);

        yield return new WaitForSeconds(4f);  // 🧨 Taş atma süresi

        animator.SetBool("throw", false);     // 🔚 Animasyonu kapat

        yield return new WaitForSeconds(1f);  // 🔁 Geçiş süresi gibi

        agent.isStopped = false;              // 🔓 Tekrar koşmaya başlasın
        agent.updatePosition = true;
        agent.updateRotation = true;

        animator.SetBool("isRunning", true);  // ✅ Tekrar koşmaya geç
    }




    protected void FaceTarget(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }
    
    

    protected Vector3 GetRandomNavmeshPoint(float maxRadius = 10f, float minDistance = 0f)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 random = Random.insideUnitSphere * maxRadius;
            random.y = 0;
            Vector3 candidate = transform.position + random;

            if (Vector3.Distance(transform.position, candidate) < minDistance)
                continue;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, maxRadius, NavMesh.AllAreas))
                return hit.position;
        }

        return transform.position;
    }


    protected void StopAllAnimations()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isYelling", false);
        animator.SetBool("DoDance", false);
        animator.SetBool("DoSelfCheck", false);
        animator.ResetTrigger("throw");
        agent.ResetPath();
    }

    protected abstract NPCReactionType GetReactionType();
    
    //TAKI-----------------------------------------------------
    //TAKI-----------------------------------------------------
    //TAKI-----------------------------------------------------
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
        agent.Warp(transform.position); // sabitle

        // 👇 yeni coroutine çağrısı
        StartCoroutine(ResumeRoamAfterCooldown());
        
    }
    protected virtual IEnumerator ResumeRoamAfterCooldown()
    {
        yield break; // default: hiçbir şey yapmaz
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
            // Karga takip
            agent.SetDestination(crow.transform.position);
            FaceTarget(crow);

            // Stres azaltma
            currentStress -= 10f;
            currentStress = Mathf.Clamp(currentStress, 0, 100f);
            if (stressBar != null) stressBar.UpdateBar(currentStress);

            // Throw trigger kararları subclass'a bağlı
            yield return new WaitForSeconds(3f);
        }

        // Bittiğinde reset
        animator.SetBool("isRunning", false);
        currentTarget = null;
        agent.ResetPath();
        isChasingCrowForJewelry = false;
    }

}
