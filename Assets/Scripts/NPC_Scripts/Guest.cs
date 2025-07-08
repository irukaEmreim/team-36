using UnityEngine;
using System.Collections;

public class Guest : BaseNPC
{
    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);

        if (isSitting)
        {
            Debug.Log("💺 Otururken hasar aldı");
            StartCoroutine(SittingDodgeReaction());
            return;
        }

        if (isReacting) return;

        if (currentStress <= maxStress * 0.5f)
            StartCoroutine(LowStressReaction());
        else
            StartCoroutine(RunThenYell(2f, 2f));
    }

    private IEnumerator SittingDodgeReaction()
    {
        isReacting = true;
        StopAllAnimations();

        Debug.Log("▶ SittingDodgeReaction Başladı");

        agent.isStopped = true;
        agent.ResetPath();
        animator.SetBool("SittingTalk", false);
        yield return null;

        animator.SetBool("SittingDodge", true);
        Debug.Log("🌀 SittingDodge TRUE");

        yield return new WaitForSeconds(2f);

        animator.SetBool("SittingDodge", false);
        Debug.Log("🛑 SittingDodge FALSE");

        yield return null;
        animator.SetBool("SittingTalk", true);
        Debug.Log("💬 Tekrar SittingTalk");

        isReacting = false;
    }






    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (!isReacting && !isSitting)
                StartCoroutine(GoSitAndEat());
        }
    }

    private bool isSitting = false;
    private Transform myChair = null;

    private IEnumerator GoSitAndEat()
    {
        
        isReacting = true;

        // 🛑 AnimationControl'u devre dışı bırak
        var animCtrl = GetComponent<AnimationControl>();
        if (animCtrl != null)
            animCtrl.isExternallyControlled = true;

        myChair = ChairManager.Instance.GetAvailableChair();

        if (myChair == null)
        {
            Debug.LogWarning("Boş sandalye yok.");
            isReacting = false;
            if (animCtrl != null)
                animCtrl.isExternallyControlled = false;
            yield break;
        }

        agent.SetDestination(myChair.position);
        animator.SetBool("isWalking", true);
        agent.isStopped = false;

        while (Vector3.Distance(transform.position, myChair.position) > 1f)
            yield return null;

        // Sandalyeye vardıktan sonra:
        agent.ResetPath();
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        transform.rotation = myChair.rotation;
        animator.SetBool("isWalking", false);
        animator.SetBool("isSitting", true);

        isSitting = true;

        // Oturma sırasında oturuyormuş gibi "talking" animasyonu yapılabilir
        animator.SetBool("SittingTalk", true);
        yield return new WaitForSeconds(60f);
        animator.SetBool("SittingTalk", false);

        // Kalkış animasyonu tetikleniyor
        Debug.Log("🪑 Kalkış başlatıldı");
        animator.SetBool("isSitting", false);

// 🕓 Kalkış animasyonunun bittiğini bekle
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("SitToStand"))
        {
            Debug.Log("🕐 Kalkıyor...");
            yield return null;
        }

// ✅ Kalktıktan sonra durumları temizle
        isSitting = false;
        ChairManager.Instance.ReleaseChair(myChair);
        myChair = null;

        // ✅ Oturma süreci bitti, kontrolü AnimationControl'e geri ver
        agent.Warp(transform.position); // 💥 BU SATIR ÖNEMLİ
        // Kalkınca tekrar aktif et
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;

        if (animCtrl != null)
            animCtrl.isExternallyControlled = false;

        isReacting = false;
    }



    IEnumerator RunThenYell(float runTime, float yellTime)
    {
        isReacting = true;
        StopAllAnimations();

        animator.SetBool("isRunning", true);
        agent.isStopped = false;

        agent.speed = 4.5f;
        
        Vector3 target = GetRandomNavmeshPoint();
        agent.SetDestination(target);

        Debug.Log($"{gameObject.name} koşuyor → Hedef: {target}"); // 🔍 DEBUG

        yield return new WaitForSeconds(runTime);

        animator.SetBool("isRunning", false);
        agent.ResetPath();

        animator.SetBool("isAngry", true);
        yield return new WaitForSeconds(yellTime);

        animator.SetBool("isAngry", false);
        isReacting = false;
    }


    private IEnumerator LowStressReaction()
    {
        isReacting = true;
        StopAllAnimations();

        animator.SetBool("isRunning", true);
        agent.speed = 4.5f;
        agent.SetDestination(GetRandomNavmeshPoint());

        yield return new WaitForSeconds(4f);

        animator.SetBool("isRunning", false);
        agent.ResetPath();

        animator.SetBool("isAngry", true);
        yield return new WaitForSeconds(4f); // Yelling süresi kadar
        animator.SetBool("isAngry", false);


        isReacting = false;
    }

}
