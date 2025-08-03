using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using UnityEngine;

public class Item_Damage : MonoBehaviour
{
    public float damage = 10;
    public bool isPushed = false;

    public float etkiSuresi = 10f;
    public float _timer = 0f; 
    void Update()
    {
        if (isPushed)
        {
            _timer += Time.deltaTime;
            if (_timer >= etkiSuresi)
            {
                _timer = 0f;
                isPushed = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Human"))
        {
            if (gameObject.CompareTag("FallableObject"))
            {
                if (isPushed)
                {
                    other.gameObject.GetComponent<NPC_Base_Test>().TakeDamage(damage);
                }
            }
            else
            {
                other.gameObject.GetComponent<NPC_Base_Test>().TakeDamage(damage);
            }
        }
    }
    ////////////////////////////////////////////////// HANGİSİ OLACAK BAKARIZZ
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Human"))
        {
            if (gameObject.CompareTag("FallableObject"))
            {
                if (isPushed)
                {
                    collision.gameObject.GetComponent<NPC_Base_Test>().TakeDamage(damage);
                }
            }
            else
            {
                collision.gameObject.GetComponent<NPC_Base_Test>().TakeDamage(damage);
            }
        }
    }
}
