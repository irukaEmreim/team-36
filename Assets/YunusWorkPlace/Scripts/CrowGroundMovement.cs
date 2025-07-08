using System.Collections;
using UnityEngine;

public class CrowGroundMovement : MonoBehaviour
{
    [Header("Hareket")]
    public float moveSpeed = 4f;
    public float hopCoolDown = 0.9f;
    public float hopForce = 2.5f;
    public float rotationSpeed = 10f;

    [Header("Kamera")]
    public Transform cameraTransform;

    private Animator animator;
    private Rigidbody rb;
    private CrowController crowController;

    private float lastHopTime = 0f;


    private float finalMoveSpeed;
    private float finalHopForce;

    private string lastTrigger = "";

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        crowController = GetComponent<CrowController>();
    }

    private void Start()
    {
        finalMoveSpeed = moveSpeed;
        finalHopForce = hopForce;
    }

    public void Move(Vector2 groundInput)
    {
        if (groundInput == Vector2.zero)
            return;

        // Kameraya gore bakis
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(camForward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        Vector3 moveDir = Vector3.zero;

        if (groundInput.y > 0)
            moveDir += camForward; 

        if (groundInput.y < 0)
        {
            moveSpeed *= 0.5f;
            hopForce *= 0.5f;
            moveDir -= camForward; 
        }

        if (groundInput.x > 0)
        {
            moveSpeed *= 0.6f;
            hopForce *= 0.6f;
            moveDir += cameraTransform.right; 
        }

        if (groundInput.x < 0)
        {
            moveSpeed *= 0.6f;
            hopForce *= 0.6f;
            moveDir -= cameraTransform.right;
        }

        moveSpeed = finalMoveSpeed;
        hopForce = finalHopForce;

        moveDir.y = 0;
        moveDir.Normalize();

        if (Time.time - lastHopTime >= hopCoolDown)
        {
            Vector3 hopVelocity = (moveDir * moveSpeed) + (Vector3.up * hopForce);
            rb.velocity = hopVelocity;
            lastHopTime = Time.time;

            string triggerToSet = "";

            if (crowController.isGrounded)
            {
                if (groundInput.x < 0)
                    triggerToSet = "HopLeft";
                else if (groundInput.x > 0)
                    triggerToSet = "HopRight";
                else if (groundInput.y < 0)
                    triggerToSet = "HopBack";
                else if (groundInput.y > 0)
                    triggerToSet = "HopForward";

                if (!string.IsNullOrEmpty(triggerToSet) && triggerToSet != lastTrigger)
                {
                    animator.SetTrigger(triggerToSet);
                    lastTrigger = triggerToSet;
                }
            }
        }
    }
    public void ResetHopTrigger()
    {
        lastTrigger = "";
    }
}
