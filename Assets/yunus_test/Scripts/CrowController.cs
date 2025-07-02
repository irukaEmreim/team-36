using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowController : MonoBehaviour
{
    
    private CrowGroundMovement crowGroundMovement;
    private CrowFlight crowFlight;
    private CrowAnimations crowAnimation;
    // private CrowAudio crowAudio;
    private CrowDirectAttack crowDirectAttack;


    private enum CrowState { Idle, Walking, Flying, Attacking }
    private CrowState currentState;

    void Start()
    {
        crowGroundMovement = GetComponent<CrowGroundMovement>();
        crowFlight = GetComponent<CrowFlight>();
        crowAnimation = GetComponent<CrowAnimations>();
        crowDirectAttack = GetComponent<CrowDirectAttack>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();      
    }

    private void HandleInput()
    {
        if (true)
        {
            
        }
    }
}
