using UnityEngine;

public class DamageTester : MonoBehaviour
{
    public float testDamageAmount = 10f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            BaseNPC[] allNPCs = FindObjectsOfType<BaseNPC>();

            foreach (BaseNPC npc in allNPCs)
            {
                if (npc is Guest guest)
                {
                    guest.TakeDamage(testDamageAmount);
                }
                else if (npc is HotelEmployee employee)
                {
                    employee.TakeDamage(testDamageAmount);
                }
                else
                {
                    npc.TakeDamage(testDamageAmount);
                }
            }

            Debug.Log($"Tuşa basıldı. {allNPCs.Length} NPC'ye {testDamageAmount} hasar verildi.");
        }
    }
}