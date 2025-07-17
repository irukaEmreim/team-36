using UnityEngine;

public class HotelEmployee : BaseNPC
{
    protected override void Start()
    {
        base.Start();
        // Çalışanların özel saatli görevleri burada olacak
    }

    protected override NPCReactionType GetReactionType()
    {
        return NPCReactionType.ChaseAndThrow;
    }
}