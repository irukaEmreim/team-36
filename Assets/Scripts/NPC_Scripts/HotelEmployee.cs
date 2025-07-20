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
        CheckIfJewelryStolen(); // Takı kontrolü çalışsın

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
    
   


}