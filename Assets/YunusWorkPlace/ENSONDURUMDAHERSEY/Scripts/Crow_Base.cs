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
    protected bool isDied = false;
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
            Die();
        }
    }

    public virtual void Die()
    {
        animator.SetBool("Die", true);
        rb.isKinematic = true;
        rb.useGravity = true;

        isDied = true;
        enabled = false;
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
