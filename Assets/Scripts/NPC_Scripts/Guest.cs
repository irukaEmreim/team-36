using UnityEngine;
using System.Collections;

public class Guest : BaseNPC
{
    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);

        if (isReacting) return;

        if (currentStress <= maxStress * 0.5f)
        {
            StartCoroutine(LowStressReaction());
        }
        else
        {
            StartCoroutine(RunThenYell(2f, 2f));
        }
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

        agent.ResetPath();
        transform.rotation = myChair.rotation;
        animator.SetBool("isWalking", false);

        animator.SetBool("isSitting", true); // 🔁 Bool parametresine göre geçiş
        isSitting = true;

        // Oturma sırasında oturuyormuş gibi "talking" animasyonu yapılabilir
        animator.SetBool("SittingTalk", true);
        yield return new WaitForSeconds(60f);
        animator.SetBool("SittingTalk", false);

        animator.SetBool("isSitting", false);
        isSitting = false;

        yield return new WaitForSeconds(1f);

        ChairManager.Instance.ReleaseChair(myChair);
        myChair = null;

        // ✅ Oturma süreci bitti, kontrolü AnimationControl'e geri ver
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
