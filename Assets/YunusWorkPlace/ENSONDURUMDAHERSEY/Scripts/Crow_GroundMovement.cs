using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crow_GroundMovement : Crow_Base
{
    [Header("Hareket Degerleri")]
    public float moveSpeed = 4f;
    public float hopCoolDown = 1f;
    public float hopForce = 2.5f;
    public float rotationSpeed = 10f;

    // privler burda
    private float lastHopTime = 0f;
    private float finalMoveSpeed;
    private float finalHopForce;
    private string lastTrigger = "";
    private Crow_MainController crow_MainController;

    protected override void Awake()
    {
        base.Awake(); // base'ten Rigidbody, Animator vs alıyoruz
        finalMoveSpeed = moveSpeed;
        finalHopForce = hopForce;
        crow_MainController = GetComponent<Crow_MainController>();
    }

    
    public void Move(Vector2 groundInput)
    {
        if (groundInput == Vector2.zero)
            return;

        if (cameraTransform == null)
        {
            Debug.LogWarning("CrowGroundMovement: Kamera atanmamış!");
            return;
        }
        if (crow_MainController.isGrounded)
        {
            // Kameraya göre ileri yön
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            Vector3 moveDir = Vector3.zero;

            float speedMod = 1f;
            float forceMod = 1f;

            if (groundInput.y < 0) { speedMod *= 0.5f; forceMod *= 0.5f; }
            if (groundInput.x != 0) { speedMod *= 0.6f; forceMod *= 0.6f; }

            moveDir += camForward * groundInput.y;
            moveDir += cameraTransform.right * groundInput.x;

            moveDir.y = 0;
            moveDir.Normalize();

            if (Time.time - lastHopTime >= hopCoolDown)
            {
                Vector3 hopVelocity = (moveDir * moveSpeed * speedMod) + (Vector3.up * hopForce * forceMod);
                rb.velocity = hopVelocity;
                lastHopTime = Time.time;

                string triggerToSet = "";

                if (groundInput.x < 0) triggerToSet = "HopLeft";
                else if (groundInput.x > 0) triggerToSet = "HopRight";
                else if (groundInput.y < 0) triggerToSet = "HopBack";
                else if (groundInput.y > 0) triggerToSet = "HopForward";

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
