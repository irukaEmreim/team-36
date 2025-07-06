using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackStone : MonoBehaviour
{

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Human"))
        {
            // other.gameObject.GetComponent<HumanController>().TakeDamage(1);
            print("Human'a vurdu");
        }
    }

    


}
