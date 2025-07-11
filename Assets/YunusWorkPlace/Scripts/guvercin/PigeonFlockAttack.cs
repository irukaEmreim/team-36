using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PigeonFlockAttack : MonoBehaviour
{
    public ParticleSystem pigeonPoopEffect;


    private void Update()
    {
        if (npcTriggered)
        {
            PigeonAttack();
        }

    }

    private List<GameObject> npcs = new List<GameObject>();     // BU KISMA GEREK YOK DIREKT TRIGGER'DA HASAR VERİLİP PARTICLE CALISTIRILIR
    private void PigeonAttack()
    {
        foreach (var item in npcs)
        {
            print(item.gameObject.name+"Pisle Altına ");
            // item.GetComponent<NpcCan>().TakeDamage(1); BÖYLELİ BİR ŞEY OLACAK
        }
    }


    private bool npcTriggered = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Human"))
        {
            npcTriggered = true;
            npcs.Add(other.gameObject);
            pigeonPoopEffect.Play();        // kus pislmee efekt
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Human"))
        {
            npcTriggered = false;
            npcs.Remove(other.gameObject);
        }
    }


}
