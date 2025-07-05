using UnityEngine;
using UnityEngine.AI;
using Microlight.MicroBar;

public abstract class BaseNPC : MonoBehaviour
{
    [Header("Stres Barı")]
    public MicroBar stressBar;
    public float maxStress = 100f;
    protected float currentStress;

    [Header("Koşma Ayarları")]
    public float runDuration = 2f;
    public float runSpeed = 5f;
    public float normalSpeed = 1.5f;
    public float runDistance = 5f;

    protected Animator animator;
    protected NavMeshAgent agent;
    protected bool isReacting = false;

    protected virtual void Start()
    {
        currentStress = maxStress;
        stressBar.Initialize(maxStress);
        stressBar.UpdateBar(currentStress);

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // test tuşu
        {
            TakeDamage(20f);
        }
    }

    public virtual void TakeDamage(float amount)
    {
        if (isReacting) return;

        currentStress -= amount;
        currentStress = Mathf.Clamp(currentStress, 0, maxStress);
        stressBar.UpdateBar(currentStress);

        if (currentStress <= maxStress * 0.5f)
        {
            StartCoroutine(ReactToStress());
        }
    }

    protected abstract System.Collections.IEnumerator ReactToStress();
}