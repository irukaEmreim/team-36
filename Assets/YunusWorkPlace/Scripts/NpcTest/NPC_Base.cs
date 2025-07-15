using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Microlight.MicroBar;

public class NPC_Base : MonoBehaviour
{
    [Header("Stress Settings")]
    public float maxStress = 100f;
    public float currentStress;
    public NPCState currentState = NPCState.Idle;

    public enum NPCState
    {
        Idle,
        Walking,
        Running,
        Attacking,
        Reacting,
        Fleeing,
        Sitting,
    }


    [HideInInspector] public NPC_StressBar stressBar;
    [HideInInspector] public NPC_Movement movement;
    [HideInInspector] public NPC_ReactionHandler reaction;
    [HideInInspector] public NPC_AnimationHandler animHandler;
    [HideInInspector] public NPC_Attack attack;


    void Awake()
    {
        stressBar = GetComponentInChildren<NPC_StressBar>();
        movement = GetComponent<NPC_Movement>();
        reaction = GetComponent<NPC_ReactionHandler>();
        animHandler = GetComponent<NPC_AnimationHandler>();
        attack = GetComponent<NPC_Attack>();
    }

    void Start()
    {
        currentStress = maxStress;
        stressBar?.Initialize(maxStress);
        stressBar?.UpdateBar(currentStress);
    }

    public void TakeDamage(float amount)
    {
        reaction?.HandleReaction();
        currentStress -= amount;
        currentStress = Mathf.Clamp(currentStress, 0, maxStress);
        stressBar?.UpdateBar(currentStress);
    }
}
