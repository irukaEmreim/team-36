using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(StoneThrower))]
public class EmployeeDailyRoutine : MonoBehaviour
{
    private enum State { Wander, Chase, GoingToHotel }

    [Header("Speeds")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 5.5f;

    [Header("Wander Settings")]
    public float wanderRadius = 20f;
    public float idleMinTime = 1f;
    public float idleMaxTime = 3f;
    public string groundTag = "Ground";

    [Header("Crow Engagement")]
    public Transform crowTransform;
    public float detectionRadius = 10f;
    public float chaseDuration = 10f;
    public float closeThrowDistance = 2f;

    [Header("Throw Timing")]
    public float firstThrowDelay = 2.2f;
    public float totalThrowDuration = 4.5f;
    public float secondThrowDelay = 2.6f;

    [Header("Stuck Detection")]
    public float stuckThreshold = 1f;
    public float velocityEpsilon = 0.1f;

    private NavMeshAgent agent;
    private Animator animator;
    private StoneThrower stoneThrower;

    private State wanderState = State.Wander;
    private Coroutine wanderCoroutine;
    private Coroutine chaseCoroutine;

    [Header("Hotel Exit")]
    public Transform hotelExitPoint;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stoneThrower = GetComponent<StoneThrower>();
    }

    void Start()
    {
        agent.speed = walkSpeed;
        wanderCoroutine = StartCoroutine(WanderRoutine());
    }
    public float timer = 0f;
    void Update()
    {
        timer += Time.deltaTime;
        // Eğer wander halindeyken crow menzile girerse chase başlat
        if (wanderState == State.Wander
            && Vector3.Distance(transform.position, crowTransform.position) < detectionRadius)
        {
            StopCoroutine(wanderCoroutine);
            chaseCoroutine = StartCoroutine(ThrowAndChaseRoutine());
        }

        UpdateAnimations();

        if (timer >= 660 && wanderState != State.GoingToHotel)
        {
            StopAllPhaseCoroutines();
            StartCoroutine(GoToHotelAndDisappear());
            // otele dön
        }

    }

    private IEnumerator WanderRoutine()
    {
        wanderState = State.Wander;
        agent.speed = walkSpeed;

        while (true)
        {
            // Rastgele nokta seç
            Vector2 rnd = Random.insideUnitCircle * wanderRadius;
            Vector3 samplePos = transform.position + new Vector3(rnd.x, 0, rnd.y);

            // Ground + NavMesh kontrolü
            if (Physics.Raycast(samplePos + Vector3.up * 5f, Vector3.down, out var hit, 10f)
             && hit.collider.CompareTag(groundTag)
             && NavMesh.SamplePosition(hit.point, out var navHit, 2f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(navHit.position);

                yield return new WaitUntil(() =>
                    !agent.pathPending &&
                    agent.remainingDistance <= agent.stoppingDistance);

                // Idle
                agent.isStopped = true;
                yield return new WaitForSeconds(Random.Range(idleMinTime, idleMaxTime));
            }
            else yield return null;
        }
    }

    public IEnumerator ThrowAndChaseRoutine()
    {
        wanderState = State.Chase;

        // --- İlk Taş Atışı ---
        agent.isStopped = true;
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetTrigger("Throw");

        float t = 0f;
        bool thrown = false;
        while (t < totalThrowDuration)
        {
            // Her kare kargaya dön
            var lookPos = crowTransform.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);

            if (!thrown && t >= firstThrowDelay)
            {
                thrown = true;
                stoneThrower.ThrowStone();
            }

            t += Time.deltaTime;
            yield return null;
        }

        // --- Kovalamaca ---
        agent.isStopped = false;
        agent.speed = runSpeed;

        float chaseT = 0f;
        float stuckT = 0f;
        while (chaseT < chaseDuration)
        {
            // Eğer çok yaklaştıysa kovalamayı bitir
            if (Vector3.Distance(transform.position, crowTransform.position) < closeThrowDistance)
                break;

            agent.SetDestination(crowTransform.position);

            if (agent.velocity.magnitude < velocityEpsilon) stuckT += Time.deltaTime;
            else stuckT = 0f;

            if (stuckT > stuckThreshold) break;

            chaseT += Time.deltaTime;
            yield return null;
        }

        // --- İkinci Taş Atışı ---
        agent.isStopped = true;
        animator.SetTrigger("Throw");

        float tt = 0f;
        while (tt < secondThrowDelay)
        {
            // Yine her kare kargaya dön
            var lookPos2 = crowTransform.position;
            lookPos2.y = transform.position.y;
            transform.LookAt(lookPos2);

            tt += Time.deltaTime;
            yield return null;
        }
        stoneThrower.ThrowStone();
        yield return new WaitForSeconds(2.2f);

        // --- Yeniden Wander’a dön ---
        agent.speed = walkSpeed;
        wanderCoroutine = StartCoroutine(WanderRoutine());
    }

    private void UpdateAnimations()
    {
        if (wanderState == State.Wander)
        {
            // Sadece Idle/Walk
            float v = agent.velocity.magnitude;
            bool walking = v > velocityEpsilon;
            animator.SetBool("isWalking", walking);
            animator.SetBool("isIdle", !walking);
            animator.SetBool("isRunning", false);
        }
        else if(wanderState == State.Chase) // Chase esnasında Run
        {
            animator.SetBool("isRunning", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", false);
        }
    }


    public void StopAllPhaseCoroutines()
{
    if (wanderCoroutine != null)
    {
        StopCoroutine(wanderCoroutine);
        wanderCoroutine = null;
    }

    if (chaseCoroutine != null)
    {
        StopCoroutine(chaseCoroutine);
        chaseCoroutine = null;
    }

    agent.isStopped = true;
}



private IEnumerator GoToHotelAndDisappear()
{
    if (hotelExitPoint == null)
    {
        Debug.LogWarning($"{gameObject.name} → hotelExitPoint atanmamış!");
        yield break;
    }

    wanderState = State.GoingToHotel;
    agent.isStopped = false;
    agent.speed = walkSpeed;
    agent.SetDestination(hotelExitPoint.position);

    animator.SetBool("isRunning", false);
    animator.SetBool("isWalking", true);
    animator.SetBool("isIdle", false);
    Debug.Log($"{gameObject.name} → Otele gidiyor...");

    // Gerçekten hedefe varana kadar bekle
    while (!HasReachedDestination())
    {
        yield return null;
    }
// Vardıktan sonra yürümeyi bırak
agent.isStopped = true;
animator.SetBool("isWalking", false);
    Debug.Log($"{gameObject.name} → Otele vardı, yok ediliyor.");
    yield return new WaitForSeconds(0.5f);
    Destroy(gameObject);
}

private bool HasReachedDestination()
{
    if (!agent.pathPending)
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }
    }
    return false;
}



}
