using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crow_PushItem : Crow_Base
{
    private Crow_Collect crow_Collect;
    public LayerMask fallableLayer;
    public float pushForce = 10f;
    protected override void Awake()
    {
        base.Awake();
        crow_Collect = GetComponent<Crow_Collect>();
    }

    void Update()
    {
        ThrowAttack();
    }
    private void ThrowAttack()
    {
        Ray ray = new Ray(crow_Collect.rayOriginTransform.position, crow_Collect.rayOriginTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, crow_Collect.rayDistance, fallableLayer))
        {
                crow_Collect.pushable = true;
            if (hit.collider.CompareTag("FallableObject"))
            {

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Rigidbody rb = hit.collider.attachedRigidbody;
                    if (rb != null)
                    {
                        Vector3 throwDir = crow_Collect.rayOriginTransform.forward + Vector3.up * 0.2f;
                        rb.AddForce(throwDir.normalized * pushForce, ForceMode.Impulse);
                        hit.transform.gameObject.GetComponent<Item_Damage>().isPushed = true;
                        hit.transform.gameObject.GetComponent<Item_Damage>()._timer = 0f;
                    }
                }

                Debug.DrawRay(ray.origin, ray.direction * crow_Collect.rayDistance, Color.green);
                return;
            }
        }
            else
            {
                crow_Collect.pushable = false;
            }

        Debug.DrawRay(ray.origin, ray.direction * crow_Collect.rayDistance, Color.red);
    }
}
