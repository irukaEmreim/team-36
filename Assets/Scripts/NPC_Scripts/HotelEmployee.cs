using UnityEngine;
using System.Collections;

public class HotelEmployee : BaseNPC
{
    
    
    
    public RuntimeAnimatorController Employee;
    public override void TakeDamage(float amount)
    {
        currentStress -= amount;
        currentStress = Mathf.Clamp(currentStress, 0, maxStress);

        if (stressBar != null)
            stressBar.UpdateBar(currentStress);

        if (!isReacting)
        {
            StartCoroutine(ChaseThenThrow());
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

    private IEnumerator ChaseThenThrow()
    {
        isReacting = true;
        StopAllAnimations();

        agent.speed = runSpeed;

        GameObject player = GameObject.FindGameObjectWithTag("lb_bird");
        currentTarget = player;
        if (player != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);

            float chaseDuration = 2f;
            float timer = 0f;

            animator.SetBool("isRunning", true);
            while (timer < chaseDuration)
            {
                agent.SetDestination(player.transform.position);
                FaceTarget(player); // 👈 Koşarken yüzünü döndür
                timer += Time.deltaTime;
                yield return null;
            }

            agent.ResetPath();
            animator.SetBool("isRunning", false);
        }

        // 👇 THROW sırasında da dönsün
        animator.SetBool("throw", true);

        float throwTime = 1f;
        float throwTimer = 0f;
        while (throwTimer < throwTime)
        {
            throwTimer += Time.deltaTime;
            if (player != null)
                FaceTarget(player);
            yield return null;
        }

        animator.SetBool("throw", false);

        agent.speed = normalSpeed;
        isReacting = false;
    }
}
