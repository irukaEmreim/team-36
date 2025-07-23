using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crow_BeeAttack : MonoBehaviour
{
    public LayerMask BeeLayer;
        private Crow_Collect crow_Collect;
    private Bee_HiveController bee_HiveController;


    private void Awake()
    {
        crow_Collect = GetComponent<Crow_Collect>();
    }


    private void Update()
    {
        HandleBeeAttack();
    }


    private void HandleBeeAttack()
    {
        Ray ray = new Ray(crow_Collect.rayOriginTransform.position, crow_Collect.rayOriginTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, crow_Collect.rayDistance, BeeLayer))
        {
            //crow_Collect.otherBirds = true;
            if (hit.collider.CompareTag("Bee"))
            {

                if (Input.GetKeyDown(KeyCode.E))
                {
                    bee_HiveController = hit.collider.GetComponent<Bee_HiveController>();
                    if (bee_HiveController != null)
                    {
                        //bee_HiveController.activated = true;
                        bee_HiveController.ActivateBeehive();
                    }
                }

                Debug.DrawRay(ray.origin, ray.direction * crow_Collect.rayDistance, Color.cyan);
                return;
            }
        }
        else
        {
            //crow_Collect.otherBirds = false;
        }
    }
}
