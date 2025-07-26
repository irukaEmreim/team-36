using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Crow_GroundMovement))]
[RequireComponent(typeof(Crow_Flight))]
[RequireComponent(typeof(Crow_DirectAttack))]
public class Crow_MainController : Crow_Base
{
    private Crow_GroundMovement crowGroundMovement;
    private Crow_Flight crowFlight;
    private Crow_DirectAttack crowDirectAttack;
    private Crow_ThrowItem crowThrowItem;


    [Header("Zemin Kontrol")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;
    public bool isGrounded = true;

    [Header("Kamera Transform")]
    public Transform main_camera;
    private float realGroundCheckDistance;
    private bool timerStarted = false;
    private float timer = 2.0f;

    public enum CrowState
    {
        Idle,
        GroundMovement,
        Flight,
        DirectAttack,
        CarryingItem
    }

    private CrowState currentState;

    protected override void Awake()
    {
        base.Awake(); // Crow_Base bileşenleri
        crowGroundMovement = GetComponent<Crow_GroundMovement>();
        crowFlight = GetComponent<Crow_Flight>();
        crowDirectAttack = GetComponent<Crow_DirectAttack>();
        crowThrowItem = GetComponent<Crow_ThrowItem>();
        if (main_camera != null)
        {
       //     print("KAMERA ATANDI");
            SetCameraTransform(main_camera);
            foreach (var mod in GetComponents<Crow_Base>())
                mod.SetCameraTransform(cameraTransform);
        }
    }


    private void Start()
    {
        realGroundCheckDistance = groundCheckDistance;
        currentState = CrowState.Idle;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

  

        // Kamera ataması gerekiyorsa dışarıdan SetCamera() çağrılabilir
    }


    private void Update()
    {
        if (!isDied)
        {
            HandleInput();
            HandleMovement();
            HandleGroundCheck();
            HandleTimer();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentState.IsGrounded())
            {
                timerStarted = true;
                crowFlight.PrepareFlight();
                currentState = CrowState.Flight;
                crowFlight.StartFlight();
                groundCheckDistance = 0f;
            }
            else if (currentState.IsFlying())
            {
                currentState = CrowState.GroundMovement;
                crowFlight.EndFlight();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && currentState.IsFlying())
        {
            crowThrowItem.ThrowItem();
        }

        if (Input.GetMouseButtonDown(0))
        {
            crowDirectAttack.GakAttack();
        }
    }

    private void HandleMovement()
    {
        if (currentState.IsFlying())
        {
            rb.useGravity = false;

            Vector2 flyInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (!crowFlight.IsFreeFlightActive())
            {
                crowFlight.StartFlight();
            }
            else
            {
                crowFlight.Fly(flyInput, cameraTransform);
            }

            currentState = CrowState.Flight;
        }
        else if (currentState.IsGrounded())
        {
            rb.useGravity = true;

            Vector2 groundInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            crowGroundMovement.Move(groundInput);

            currentState = groundInput == Vector2.zero ? CrowState.Idle : CrowState.GroundMovement;
        }
    }


    private void HandleGroundCheck()
    {
        Vector3 origin = transform.position;
        Vector3 direction = Vector3.down;
        Debug.DrawRay(origin, (direction * groundCheckDistance) - new Vector3(0,0.5f,0), isGrounded ? Color.green : Color.red);

        if (Physics.Raycast(origin + new Vector3(0,0.5f,0), direction , groundCheckDistance, groundLayer))
        {
            isGrounded = true;
        }
        else {
            isGrounded = false; }

        if (!isGrounded)
        {
         //   print("NASIL LO");
            animator.SetBool("Flying", true);
            if (crowFlight.canFreeFly == false && crowFlight.isFlightStarted)
            {
                float currentRotationY = transform.rotation.eulerAngles.y;
                Quaternion targetRotation = Quaternion.Euler(0f, currentRotationY, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        else
        {
         //   print("Şİmdi mi");
            currentState = CrowState.GroundMovement;
            crowFlight.EndFlight();
            animator.SetBool("Flying", false);
        }
    }

    private void HandleTimer()
    {
        if (timerStarted)
        {
            timer -= Time.deltaTime;
            if (timer <= 0.0f)
            {
                groundCheckDistance = realGroundCheckDistance;
                timerStarted = false;
                timer = 2.0f;
            }
        }
    }

    public CrowState GetState() => currentState;
}

