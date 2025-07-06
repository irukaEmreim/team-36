using UnityEngine;
using System.Collections;

public class HotelEmployee : BaseNPC
{
    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);

        if (isReacting) return;

        if (currentStress <= maxStress * 0.5f)
        {
            StartCoroutine(JustYell(4f));
        }
        else
        {
            StartCoroutine(RunThenYell(2f, 2f));
        }
    }

    IEnumerator JustYell(float time)
    {
        isReacting = true;
        StopAllAnimations();

        animator.SetBool("isYelling", true);
        yield return new WaitForSeconds(time);
        animator.SetBool("isYelling", false);

        isReacting = false;
    }

    IEnumerator RunThenYell(float runTime, float yellTime)
    {
        isReacting = true;
        StopAllAnimations();
        agent.isStopped = false;

        animator.SetBool("isRunning", true);
        agent.speed = 5f;

        Vector3 runTarget = transform.position + (Random.insideUnitSphere * 5f);
        runTarget.y = transform.position.y;

        agent.SetDestination(runTarget);

        yield return new WaitForSeconds(runTime);

        animator.SetBool("isRunning", false);
        agent.ResetPath();

        animator.SetBool("isYelling", true);
        yield return new WaitForSeconds(yellTime);
        animator.SetBool("isYelling", false);

        isReacting = false;
    }
}
