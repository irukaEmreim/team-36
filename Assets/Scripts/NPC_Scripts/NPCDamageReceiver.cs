using UnityEngine;

public class NPCDamageReceiver : MonoBehaviour
{
    private bool pigeonNearby = false;
    private BaseNPC npc;

    private void Awake()
    {
        npc = GetComponent<BaseNPC>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PigeonController>())
        {
            pigeonNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PigeonController>())
        {
            pigeonNearby = false;
        }
    }

    void Update()
    {
        if (pigeonNearby && Input.GetMouseButtonDown(0))
        {
            npc.TakeDamage(10f);
            Debug.Log($"{gameObject.name} → PIGEON'DAN HASAR ALDI!");
        }
    }
}