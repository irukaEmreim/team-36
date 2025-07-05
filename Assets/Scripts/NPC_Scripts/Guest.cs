using System.Collections;
using UnityEngine;

public class Guest : BaseNPC
{
    protected override IEnumerator ReactToStress()
    {
        isReacting = true;

        animator.SetBool("isRunning", true);
        agent.speed = runSpeed;
        Vector3 runTarget = transform.position + (Random.insideUnitSphere * runDistance * 1.5f); // daha uzak kaçar
        runTarget.y = transform.position.y;
        agent.SetDestination(runTarget);

        yield return new WaitForSeconds(runDuration + 1f); // daha uzun süre kaçar

        animator.SetBool("isRunning", false);
        agent.speed = normalSpeed;
        agent.ResetPath();

        // Kızmaz, panikler belki?
        animator.SetTrigger("isScared"); // varsa
        yield return new WaitForSeconds(2f);

        isReacting = false;
    }
}