using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CrowController;

public class CrowController : MonoBehaviour
{
    private CrowGroundMovement crowGroundMovement;
    private CrowFlight crowFlight;
    private CrowDirectAttack crowDirectAttack;
    public Transform cameraTransform;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded = true;

    private Rigidbody rb;
    private Animator animator;

    public enum CrowState
    {
        Idle,
        GroundMovement,
        Flight,
        DirectAttack
    }

    private CrowState currentState;

    private float realGroundCheckDistance;
    private bool timerStarted = false;
    private float timer = 2.0f;

    private void Awake()
    {
        crowGroundMovement = GetComponent<CrowGroundMovement>();
        crowFlight = GetComponent<CrowFlight>();
        crowDirectAttack = GetComponent<CrowDirectAttack>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        realGroundCheckDistance = groundCheckDistance;
        currentState = CrowState.Idle;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleInput();

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
        
        Vector3 origin = transform.position;
        Vector3 direction = Vector3.down;
        isGrounded = Physics.Raycast(origin, direction, groundCheckDistance, groundLayer);
        Debug.DrawRay(origin, direction * groundCheckDistance, isGrounded ? Color.green : Color.red);

        if (!isGrounded)
        {
            animator.SetBool("Flying", true);
        }
        else
        {
            currentState = CrowState.GroundMovement;
            crowFlight.EndFlight();
            animator.SetBool("Flying", false);
        }

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
            crowDirectAttack.ThrowRock();
        }

        if (Input.GetMouseButtonDown(0))
        {
            crowDirectAttack.GakAttack();
        }
    }

    public CrowState GetState()
    {
        return currentState;
    }
}
