using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class AnimationControl : MonoBehaviour
{
    public float roamRadius = 10f;
    public float minActionTime = 6f;
    public float maxActionTime = 9f;
    [HideInInspector]
    public bool isExternallyControlled = false;


    private NavMeshAgent agent;
    private Animator animator;

    private float waitTimer = 0f;
    private bool isWaiting = false;
    private string currentAction = "";

    private string[] idleActions = { "DoDance", "DoDance2", "DoExercise", "DoSelfCheck", "DoPhoneTalk" };
    private string[] movementActions = { "isPhoneWalking",  };

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        

        agent.ResetPath(); // 🔒 Hedef sıfırla
        agent.velocity = Vector3.zero; // ⛔ Başlangıç kaymasını durdur

        PlayRandomAction(); // ilk aksiyon başlat
    }


    void Update()
    {
        float speed = agent.velocity.magnitude;
        bool isMoving = speed > 0.1f;

        // Normal yürüme animasyonu
        animator.SetBool("isWalking", isMoving && currentAction == "isWalking" && !isExternallyControlled);



        // Eğer KOŞUYORSA ve hedefi bittiyse → yeni nokta ver
        if (animator.GetBool("isRunning") && (!agent.hasPath || agent.remainingDistance < 0.5f))
        {
            agent.speed = 4.5f; // Koşma hızı
            MoveToRandomPoint();
        }

        // Eğer yürüyorsa ama koşmuyorsa (normal yürüyüş)
        if (animator.GetBool("isWalking") && (!agent.hasPath || agent.remainingDistance < 0.5f))
        {
            agent.speed = 1.5f;
            MoveToRandomPoint();
        }
        
        // Eğer hedefe ulaştıysa ve beklemiyorsa, yeni animasyon başlat
        if (!isWaiting)
        {
            isWaiting = true;
            PlayRandomAction();
        }

        // Sabit animasyon süresi dolduysa → yeni aksiyona geç
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f && currentAction != "")
            {
                ResetAllBools(); // 🔴 tüm animasyonları temizle
                animator.SetBool(currentAction, false); // ekstra güvenlik
                currentAction = "";

                agent.ResetPath();
                isWaiting = false;
            }

        }

    }


    void PlayRandomAction()
    {
        ResetAllBools();     // tüm bool animasyonları sıfırla
        currentAction = "";  // aktif aksiyonu boşalt


        int type = Random.Range(0, 2); // 0 = idle action, 1 = phone walk, 2 = run

        if (type == 0) // Sabit animasyon
        {
            currentAction = idleActions[Random.Range(0, idleActions.Length)];
            animator.SetBool(currentAction, true);
            waitTimer = Random.Range(minActionTime, maxActionTime);

            agent.ResetPath(); // ⛔ durdur
            agent.velocity = Vector3.zero; // 🛑 kaymayı engelle

            Debug.Log("🎬 Yeni aksiyon başladı: " + currentAction);
        }

        else if (type == 1) // Telefonla yürüme
        {
            currentAction = "isPhoneWalking";
            animator.SetBool(currentAction, true);
            agent.speed = 1.5f;
            MoveToRandomPoint();
            waitTimer = Random.Range(minActionTime, maxActionTime);
            Debug.Log("🎬 Yeni aksiyon başladı: " + currentAction);

        }
        else // Koşma
        {
            currentAction = "isRunning";
            animator.SetBool(currentAction, true);
            agent.speed = 4.5f;
            MoveToRandomPoint();
            waitTimer = Random.Range(minActionTime, maxActionTime);
            Debug.Log("🎬 Yeni aksiyon başladı: " + currentAction);

        }
    }

    void MoveToRandomPoint()
    {
        int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
            randomDirection.y = 0; // YÜKSEKLİK KARMAŞASI OLMASIN
            Vector3 targetPosition = transform.position + randomDirection;

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                Debug.Log("✅ Hedef bulundu: " + hit.position);
                return;
            }
        }

        Debug.LogWarning("❌ Geçerli hedef bulunamadı, karakter yerinde kalacak.");
        isWaiting = false;
    }


    void ResetAllBools()
    {
        agent.velocity = Vector3.zero;

        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isPhoneWalking", false);
          
        foreach (string action in idleActions)
            animator.SetBool(action, false);
    }

    bool IsInMovementAction()
    {
        return animator.GetBool("isRunning") || animator.GetBool("isPhoneWalking");
    }

    bool IsInIdleState()
    {
        // Animator’daki idle state’in adını kontrol et
        return animator.GetCurrentAnimatorStateInfo(0).IsName("idle_f_1_150f"); // animator'daki idle animasyon state'inin adını buraya yaz
    }
}
