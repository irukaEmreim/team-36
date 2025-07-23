using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Crow_Flight : Crow_Base
{
    [Header("Uçuş Ayarları")]
    public float targetFlyHeight = 0.4f;
    public float takeoffSpeed = 5f;
    public float flightSpeed = 10f;

    [Header("Uçuş Durumları")]
    public bool canFreeFly = false;
    public bool isFlightStarted = false;
    private bool isFlightEnded = false;

    [Header("Kamera")]
    public CinemachineFreeLook cinemachineFreeLook;

    public Vector3 lastFlyDirection = Vector3.forward;

    protected override void Awake()
    {
        base.Awake(); // base sınıftan bileşenleri alıyoruz
    }
    

    private void Update()
    {
        if (isFlightStarted)
            changeCinemachineCameraValuesToFly();

        if (isFlightEnded)
            changeCinemachineCameraValuesToGround();

    }

    public void PrepareFlight()
    {
        targetFlyHeight = transform.position.y + 0.4f;
    }

    public void StartFlight()
    {
        if (!isFlightStarted)
        {
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            animator.SetBool("Flying", true);
            isFlightStarted = true;
            isFlightEnded = false;
        }

        if (!canFreeFly)
        {
            if (transform.position.y < targetFlyHeight)
            {
                rb.velocity = new Vector3(0, takeoffSpeed, 0);
            }
            else
            {
                rb.velocity = Vector3.zero;
                canFreeFly = true;
            }
        }
    }

    public void Fly(Vector2 flyInput, Transform cameraTransform = null)
    {
        if (!canFreeFly) return;

        if (cameraTransform == null && this.cameraTransform != null)
            cameraTransform = this.cameraTransform;

        if (cameraTransform == null)
        {
            Debug.LogWarning("CrowFlight: Kamera atanmadı.");
            return;
        }

        Vector3 camForward = cameraTransform.forward.normalized;
        Vector3 camRight = cameraTransform.right.normalized;

        Vector3 moveDirection = (camForward * flyInput.y + camRight * flyInput.x).normalized;

        if (moveDirection != Vector3.zero)
        {
            rb.velocity = moveDirection * flightSpeed;
            lastFlyDirection = moveDirection;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else
        {
            rb.velocity = Vector3.zero;

            float currentRotationY = transform.rotation.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, currentRotationY, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    public void EndFlight()
    {
        isFlightStarted = false;
        isFlightEnded = true;
        canFreeFly = false;
        rb.useGravity = true;
    }

    public bool IsFreeFlightActive()
    {
        return canFreeFly;
    }



    public void changeCinemachineCameraValuesToFly()
    {
        if (cinemachineFreeLook == null) return;

        cinemachineFreeLook.m_Orbits[0].m_Radius = Mathf.Lerp(cinemachineFreeLook.m_Orbits[0].m_Radius, 3f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[1].m_Radius = Mathf.Lerp(cinemachineFreeLook.m_Orbits[1].m_Radius, 7f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[2].m_Radius = Mathf.Lerp(cinemachineFreeLook.m_Orbits[2].m_Radius, 3f, Time.deltaTime * 10f);

        cinemachineFreeLook.m_Orbits[0].m_Height = Mathf.Lerp(cinemachineFreeLook.m_Orbits[0].m_Height, -4f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[1].m_Height = Mathf.Lerp(cinemachineFreeLook.m_Orbits[1].m_Height, 0.3f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[2].m_Height = Mathf.Lerp(cinemachineFreeLook.m_Orbits[2].m_Height, 4f, Time.deltaTime * 10f);

        cinemachineFreeLook.m_YAxis.m_InvertInput = false;
    }

    public void changeCinemachineCameraValuesToGround()
    {
        if (cinemachineFreeLook == null) return;

        cinemachineFreeLook.m_Orbits[0].m_Radius = Mathf.Lerp(cinemachineFreeLook.m_Orbits[0].m_Radius, 5f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[1].m_Radius = Mathf.Lerp(cinemachineFreeLook.m_Orbits[1].m_Radius, 7f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[2].m_Radius = Mathf.Lerp(cinemachineFreeLook.m_Orbits[2].m_Radius, 5f, Time.deltaTime * 10f);

        cinemachineFreeLook.m_Orbits[0].m_Height = Mathf.Lerp(cinemachineFreeLook.m_Orbits[0].m_Height, 0.2f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[1].m_Height = Mathf.Lerp(cinemachineFreeLook.m_Orbits[1].m_Height, 1.5f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[2].m_Height = Mathf.Lerp(cinemachineFreeLook.m_Orbits[2].m_Height, 2f, Time.deltaTime * 10f);

        cinemachineFreeLook.m_YAxis.m_InvertInput = false;
    }

}
