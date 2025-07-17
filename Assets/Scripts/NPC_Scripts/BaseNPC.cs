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
        agent.speed = runSpeed;
        agent.SetDestination(GetRandomNavmeshPoint());

        yield return new WaitForSeconds(2f);
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
    
    

    protected Vector3 GetRandomNavmeshPoint(float radius = 10f)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 random = Random.insideUnitSphere * radius;
            random.y = 0;
            Vector3 point = transform.position + random;

            if (NavMesh.SamplePosition(point, out NavMeshHit hit, radius, NavMesh.AllAreas))
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
}
