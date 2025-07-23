using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird_Controller : MonoBehaviour
{
    public float maxFlyHeight = 7f;
    public float flySpeed = 5f;
    public float riseSpeed = 3f;

    private Rigidbody rb;
    public bool isFlying = false;
    private Vector3 flyDirection;

    public List<Transform> birdsTranforms;
    public BoxCollider pigeonTriggerCollider;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void FlyAway(Vector3 direction)
    {
        if (isFlying) return;

        isFlying = true;
        pigeonTriggerCollider.enabled = true;
        flyDirection = direction.normalized;
        flyDirection.y = 0; // Y ekseni

        rb.useGravity = false;
    }

    private void Update()
    {
        if (!isFlying) return;
        
        Vector3 currentPos = transform.position;

        for (int i = 0; i < birdsTranforms.Count; i++)
        {
            birdsTranforms[i].rotation = Quaternion.LookRotation(flyDirection);
        }

        if (currentPos.y < maxFlyHeight)
        {
            // yüksel
            rb.velocity = Vector3.up * riseSpeed + flyDirection * flySpeed;
        }
        else
        {
            // Maksimum yükseklik ileri
            rb.velocity = flyDirection * flySpeed;
        }
    }
}
