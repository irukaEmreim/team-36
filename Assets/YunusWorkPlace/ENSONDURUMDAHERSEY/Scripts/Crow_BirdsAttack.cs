using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crow_BirdsAttack : MonoBehaviour
{
    [Header("Uçuş Ayarları")]
    private Crow_Collect crow_Collect;
    public LayerMask pigeonLayer;
    private Bird_Controller currentPigeon;
        private Vector3 flyDirection;
            private bool pigeonFly = false;

    public float flyDuration = 10f;
    private float timer = 0f;


    void Awake()
    {
        crow_Collect = GetComponent<Crow_Collect>();
    }
    void Update()
    {
        HandleBirdsAttack();

        if (pigeonFly)
        {
            timer -= Time.deltaTime;
            if (currentPigeon != null)
            {
                currentPigeon.FlyAway(flyDirection);
            }
            if (timer <= 0f)
            {
                pigeonFly = false;
                currentPigeon = null;
                flyDirection = Vector3.zero;
                timer = flyDuration;
            }
        }
    }

    private void HandleBirdsAttack()
    {
        Ray ray = new Ray(crow_Collect.rayOriginTransform.position, crow_Collect.rayOriginTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, crow_Collect.rayDistance, pigeonLayer))
        {
            crow_Collect.otherBirds = true;
            if (hit.collider.CompareTag("Pigeon"))
            {

                if (Input.GetKeyDown(KeyCode.E))
                {
                    currentPigeon = hit.collider.GetComponent<Bird_Controller>();
                    if (currentPigeon != null)
                    {
                        print("TAMAMDIRRRR");
                        flyDirection = crow_Collect.rayOriginTransform.forward; // o anki bakış yönü
                        pigeonFly = true;
                        timer = flyDuration;
                    }
                }

                Debug.DrawRay(ray.origin, ray.direction * crow_Collect.rayDistance, Color.cyan);
                return;
            }
        }
        else
        {
            crow_Collect.otherBirds = false;
        }
    }
}
