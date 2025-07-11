using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CrowIndirectAttack : MonoBehaviour
{
    [Header("Raycast Ayarlari")]
    public Transform rayOriginTransform;
    public float rayDistance = 3f;
    public LayerMask fallableLayer;

    [Header("UI")]
    public GameObject pressTextUI;

    [Header("Force Ayarlari")]
    public float throwForce = 10f;

    [Header("Pigeon Ayarlari")]
    public float transformPosY = 7f;
    public LayerMask pigeonLayer;

    [Header("Uçuş Ayarları")]
    public float flyDuration = 10f;
    private float timer = 0f;

    private bool pigeonFly = false;
    private PigeonController currentPigeon;
    private Vector3 flyDirection;

    private void Update()
    {
        InDirectAttack();

        HandleRaycastAndInput();

        HandleUI();
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

    private void HandleRaycastAndInput()
    {
        Ray ray = new Ray(rayOriginTransform.position, rayOriginTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, pigeonLayer))
        {
            if (hit.collider.CompareTag("Pigeon"))
            {

                if (Input.GetKeyDown(KeyCode.E))
                {
                    currentPigeon = hit.collider.GetComponent<PigeonController>();
                    if (currentPigeon != null)
                    {
                        flyDirection = rayOriginTransform.forward; // o anki bakış yönü
                        pigeonFly = true;
                        timer = flyDuration;
                    }
                }

                Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.cyan);
                return;
            }
        }

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.gray);
    }



    private void InDirectAttack()
    {
        Ray ray = new Ray(rayOriginTransform.position, rayOriginTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, fallableLayer))
        {
            if (hit.collider.CompareTag("FallableObject"))
            {

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Rigidbody rb = hit.collider.attachedRigidbody;
                    if (rb != null)
                    {
                        Vector3 throwDir = rayOriginTransform.forward + Vector3.up * 0.2f;
                        rb.AddForce(throwDir.normalized * throwForce, ForceMode.Impulse);
                    }
                }

                Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.green);
                return;
            }
        }

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
    }

    private void HandleUI()
    {
        Ray ray = new Ray(rayOriginTransform.position, rayOriginTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, fallableLayer) || Physics.Raycast(ray, out hit, rayDistance, pigeonLayer))
        {
            pressTextUI.SetActive(true);
        }
        else
        {
            pressTextUI.SetActive(false);
        }
    }

}
