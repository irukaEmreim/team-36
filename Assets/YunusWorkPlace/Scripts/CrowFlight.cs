using Cinemachine;
using UnityEngine;

public class CrowFlight : MonoBehaviour
{
    [Header("Uçuş Ayarları")]
    public float targetFlyHeight = 0.4f;      
    public float takeoffSpeed = 5f;         
    public float flightSpeed = 10f;         

    [Header("Durum Bayrakları")]
    private bool canFreeFly = false;        
    private bool isFlightStarted = false;   
    private bool isFlightEnded = false;     

    private CrowController crowController;
    private Animator animator;
    private Rigidbody rb;

    [Header("Kamera Ayarları")]
    public CinemachineFreeLook cinemachineFreeLook;
    public Transform cameraTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        crowController = GetComponent<CrowController>();
    }

    // .. kadar yukselir space basinca
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
            changeCinemachineCameraValuesToFly();

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

    public void EndFlight()
    {
        isFlightEnded = true;
        isFlightStarted = false;
        rb.useGravity = true;
        canFreeFly = false;
        changeCinemachineCameraValuesToGround();
    }

    public bool IsAtFlightHeight()
    {
        return transform.position.y >= targetFlyHeight;
    }

    public Vector3 lastFlyDirection = Vector3.forward;
    public void Fly(Vector2 flyInput, Transform cameraTransform)
    {
        if (!canFreeFly) return;

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

            // ileri bakis
            float currentRotationY = transform.rotation.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, currentRotationY, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void Update()
    {
        if (isFlightStarted)
            changeCinemachineCameraValuesToFly();

        if (isFlightEnded)
            changeCinemachineCameraValuesToGround();

        if (crowController.isGrounded)
            animator.SetBool("Flying", false);

        if (!canFreeFly && !crowController.isGrounded)
        {
            float currentRotationY = transform.rotation.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, currentRotationY, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    public void changeCinemachineCameraValuesToFly()
    {
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
        cinemachineFreeLook.m_Orbits[0].m_Radius = Mathf.Lerp(cinemachineFreeLook.m_Orbits[0].m_Radius, 5f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[1].m_Radius = Mathf.Lerp(cinemachineFreeLook.m_Orbits[1].m_Radius, 7f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[2].m_Radius = Mathf.Lerp(cinemachineFreeLook.m_Orbits[2].m_Radius, 5f, Time.deltaTime * 10f);

        cinemachineFreeLook.m_Orbits[0].m_Height = Mathf.Lerp(cinemachineFreeLook.m_Orbits[0].m_Height, 0.2f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[1].m_Height = Mathf.Lerp(cinemachineFreeLook.m_Orbits[1].m_Height, 1.5f, Time.deltaTime * 10f);
        cinemachineFreeLook.m_Orbits[2].m_Height = Mathf.Lerp(cinemachineFreeLook.m_Orbits[2].m_Height, 2f, Time.deltaTime * 10f);

        cinemachineFreeLook.m_YAxis.m_InvertInput = false;
    }

    public bool IsFreeFlightActive()
    {
        return canFreeFly;
    }
}
