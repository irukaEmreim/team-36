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
