using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackStone : MonoBehaviour
{

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Human"))
        {
            other.gameObject.GetComponent<NPC_Base>().TakeDamage(20);
            print("Human'a vurdu");
        }
    }

    


}
