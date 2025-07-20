using System.Collections;
using UnityEngine;

public class HotelEmployee : BaseNPC
{
    protected override void Start()
    {
        base.Start();
        // Çalışanların özel saatli görevleri burada olacak
    }
    
    private float aggressionDistance = 4f;

    protected override void Update()
    {
        base.Update(); // 💥 Takı kovalamayı aktif tutar

        if (isReacting) return;

        CheckCrowProximityAndAttack();
    }

   

    private void CheckCrowProximityAndAttack()
    {
        GameObject crow = GameObject.FindGameObjectWithTag("lb_bird");
        if (crow == null || isReacting) return;

        float distance = Vector3.Distance(transform.position, crow.transform.position);
        if (distance < aggressionDistance)
        {
            isReacting = true;
            StopAllAnimations();
            StartCoroutine(ChaseThenThrow()); // 💥 Kovalayıp taş fırlatsın
        }
    }


    protected override NPCReactionType GetReactionType()
    {
        return NPCReactionType.ChaseAndThrow;
    }
    
    protected override IEnumerator JewelryChase()
    {
        GameObject crow = GameObject.FindGameObjectWithTag("lb_bird");
        if (crow == null) yield break;

        currentTarget = crow;
        agent.speed = runSpeed * 3f;
        animator.SetBool("isRunning", true);

        while (isChasingCrowForJewelry && crow != null)
        {
            if (!IsJewelryStillStolen() || currentStress <= 0f)
            {
                StopChasingCrow();
                yield break;
            }

            agent.SetDestination(crow.transform.position);
            FaceTarget(crow);

            currentStress -= 10f;
            currentStress = Mathf.Clamp(currentStress, 0, 100f);
            if (stressBar != null) stressBar.UpdateBar(currentStress);

            yield return StartCoroutine(ThrowOnceThenRun());
            yield return new WaitForSeconds(3f);
        }

        animator.SetBool("isRunning", false);
        currentTarget = null;
        agent.ResetPath();
        isChasingCrowForJewelry = false;
    }


}