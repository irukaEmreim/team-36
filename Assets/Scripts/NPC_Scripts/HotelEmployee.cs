using System.Collections;
using UnityEngine;

public class HotelEmployee : BaseNPC
{
    protected override IEnumerator ReactToStress()
    {
        isReacting = true;

        animator.SetBool("isRunning", true);
        agent.speed = runSpeed;
        Vector3 runTarget = transform.position + (Random.insideUnitSphere * runDistance);
        runTarget.y = transform.position.y;
        agent.SetDestination(runTarget);

        yield return new WaitForSeconds(runDuration);

        animator.SetBool("isRunning", false);
        agent.speed = normalSpeed;
        agent.ResetPath();

        yield return new WaitForSeconds(0.1f); // idle frame atlamadan bekle
        animator.SetTrigger("isAngry");

        yield return new WaitForSeconds(2f);
        isReacting = false;
    }
}