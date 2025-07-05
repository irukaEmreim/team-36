using UnityEngine;
using Microlight.MicroBar;
using UnityEngine.AI;

public class StressBarController : MonoBehaviour
{
    [Header("MicroBar")]
    public MicroBar stressBar;
    public float maxStress = 100f;
    private float currentStress;

    [Header("Koşma Ayarları")]
    public float runDuration = 2f;
    public float runSpeed = 5f;
    public float normalSpeed = 1.5f;
    public float runDistance = 5f;

    [Header("Bileşenler")]
    private Animator animator;
    private NavMeshAgent agent;
    private AnimationControl animationControl;

    private bool isReacting = false;

    void Start()
    {
        currentStress = maxStress;
        stressBar.Initialize(maxStress);
        stressBar.UpdateBar(currentStress);

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        animationControl = GetComponent<AnimationControl>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(15f);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isReacting) return;

        currentStress -= amount;
        currentStress = Mathf.Clamp(currentStress, 0, maxStress);
        stressBar.UpdateBar(currentStress);

        if (currentStress <= maxStress * 0.5f)
        {
            StartCoroutine(ReactWithRunAndAnger());
        }
        else
        {
            Debug.Log($"{gameObject.name} şu an sakin. Henüz tepki vermedi.");
        }
    }

    System.Collections.IEnumerator ReactWithRunAndAnger()
    {
        isReacting = true;
        animationControl.isExternallyControlled = true;

        // 1️⃣ Koşma
        animator.SetBool("isRunning", true);
        agent.speed = runSpeed;

        Vector3 runTarget = transform.position + (Random.insideUnitSphere * runDistance);
        runTarget.y = transform.position.y;
        agent.SetDestination(runTarget);

        yield return new WaitForSeconds(runDuration);

        // 2️⃣ Koşmayı durdur
        animator.SetBool("isRunning", false);
        agent.speed = normalSpeed;
        agent.ResetPath();

        // 3️⃣ Doğrudan kızma animasyonunu tetikle
        animator.SetTrigger("isAngry");
        yield return new WaitForSeconds(2f);

        // 4️⃣ Dış kontrolü bırak
        animationControl.isExternallyControlled = false;
        isReacting = false;
    }
}
