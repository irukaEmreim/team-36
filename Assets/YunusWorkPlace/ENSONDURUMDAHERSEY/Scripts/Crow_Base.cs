using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class Crow_Base : MonoBehaviour
{
    [Header("Can Ayarlari")]
    [SerializeField] protected int maxHealth = 100;
    protected int currentHealth;

    [Header("Componentlar")]
    protected Animator animator;
    protected Rigidbody rb;
    protected AudioSource audioSource;
    public bool isDied = false;
    [HideInInspector] protected Transform cameraTransform;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
    }



    #region Can Fonksiyonlari

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth <= 0)
        {
            isDied = true;
            Die();
        }
    }

    public virtual void Die()
    {
        // stop feeding into any more movement
    isDied = true;
    animator.SetTrigger("Die");

    // make sure physics is driving us again
    rb.isKinematic = false;
    rb.useGravity  = true;

    // allow falling, but optionally lock rotation so it doesn't tumble
    rb.constraints = RigidbodyConstraints.FreezeRotation;

    // disable all of your crow‐movement scripts
    enabled = false; // disables Crow_Base itself
    foreach (var comp in new MonoBehaviour[] {
        GetComponent<Crow_MainController>(),
        GetComponent<Crow_GroundMovement>(),
        GetComponent<Crow_Flight>(),
        GetComponent<Crow_ThrowItem>(),
        GetComponent<GakTimer>()
    })
        if (comp != null) comp.enabled = false;
    }

    public int GetCurrentHealth() => currentHealth;

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    #endregion

    #region Kamera Baglama

    public void SetCameraTransform(Transform cam) => cameraTransform = cam;

    #endregion
}
