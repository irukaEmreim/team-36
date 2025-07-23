using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crow_Health : Crow_Base
{
    [Header("UI")]
    public Image healthIcon;
    private Material iconMaterial;

    protected override void Awake()
    {
        base.Awake();
        if (healthIcon != null)
        {
            iconMaterial = healthIcon.material;
        }
        UpdateHealthVisual();
    }

    private void Update()
    {
        UpdateHealthVisual();
        print(currentHealth);
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(15);
        }
    }

    private void UpdateHealthVisual()
    {
        if (iconMaterial == null) return;

        float healthRatio = (float)currentHealth / maxHealth;
        iconMaterial.SetFloat("_Health", healthRatio);

    }
    

    
}
