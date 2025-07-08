using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrowHealth : MonoBehaviour
{
    [Header("Can")]
    public int maxHealth = 100;
    public int currentHealth;

    private Animator animator;

    [Header("UI")]
    public Image healthIcon;                     
    private Material iconMaterial;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        if (healthIcon != null)
        {
            iconMaterial = healthIcon.material;
        }

        UpdateHealthVisual();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthVisual();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Hit animasyonu varsa tetiklenebilir
            // animator.SetTrigger("Hit"); gibi
        }
    }

    private void Die()
    {
        Debug.Log("ÖLDÜ");

        animator.SetBool("Die", true);

        GetComponent<CrowController>().enabled = false;
        GetComponent<CrowDirectAttack>().enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = true;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("OHHH İYİYİM ARTIK.. Yeni Can : " + currentHealth);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void UpdateHealthVisual()
    {
        float healthRatio = (float)currentHealth / maxHealth;

        if (iconMaterial != null)
        {
            iconMaterial.SetFloat("_Health", healthRatio); // Shader'daki _Health parametresi
        }
    }

    private void Update()
    {
        UpdateHealthVisual();
    }
}
