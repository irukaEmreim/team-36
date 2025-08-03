using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GakTimer : MonoBehaviour
{

    [SerializeField] private Material gakMaterial;
    [SerializeField] private float cooldownTime = 10f;
    private float currentCooldown = 0f;
    public Crow_DirectAttack crow_DirectAttack;
    private float pulseTime = 0f;
    [SerializeField] private float pulseDuration = 0.3f;
    [SerializeField] private float pulseIntensity = 0.1f;

    private void Update()
    {
         // Gak giriş kontrolü
        if (Input.GetMouseButtonDown(0))
        {
            if (currentCooldown <= 0f)
            {
                crow_DirectAttack.GakAttack();
                currentCooldown = cooldownTime;
            }
            else
            {
                PlayPulse(); // cooldown aktifken tıklanırsa sadece pulse oynar
            }
        }

        // Cooldown güncellemesi
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            float progress = currentCooldown / cooldownTime;
            gakMaterial.SetFloat("_CooldownProgress", progress);
        }
        

        if (pulseTime > 0f)
        {
            pulseTime -= Time.deltaTime;
            float pulseValue = Mathf.Sin((1 - (pulseTime / pulseDuration)) * Mathf.PI) * pulseIntensity;
            gakMaterial.SetFloat("_PulseAmount", pulseValue);
        }
        else
        {
            gakMaterial.SetFloat("_PulseAmount", 0f);
        }


    }
    
    public void PlayPulse()
{
    pulseTime = pulseDuration;
}
}
