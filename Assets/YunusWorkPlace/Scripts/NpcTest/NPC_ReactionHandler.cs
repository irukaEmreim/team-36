using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_ReactionHandler : MonoBehaviour
{

    private NPC_Base npc;
    private NPC_Movement movement;
    private NPC_AnimationHandler animHandler;
    private Animator animator;
    private NPC_ActionCycle actionCycle;
    public float fleeMinDistance = 10f; // NPC'nin kaçacağı mesafe
    public float fleeMaxDistance = 20f; // NPC'nin kaçabileceği maksimum mesafe
    public float fleeDuration = 2f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        npc = GetComponent<NPC_Base>();
        movement = GetComponent<NPC_Movement>();
        animHandler = GetComponent<NPC_AnimationHandler>();
        actionCycle = GetComponent<NPC_ActionCycle>();
    }

    public void HandleReaction()
    {
        if (npc.currentState == NPC_Base.NPCState.Reacting) return;

        StartCoroutine(Flee());
    }

    IEnumerator Flee()
    {
        npc.currentState = NPC_Base.NPCState.Reacting;

        //anim durdur
        animHandler.StopAll();
        movement.Stop();

        // rastgele kaçış noktası
        Vector3 fleePoint = movement.GetSafePointAwayFrom(transform.position, fleeMinDistance, fleeMaxDistance);
        movement.MoveTo(fleePoint, true);
        animHandler.PlayRun();

        yield return new WaitUntil(() => movement.HasReachedDestination());

        movement.Stop();
        string idleAnim = animHandler.GetRandomIdle();
        animHandler.PlayOneShot(idleAnim);
        npc.currentState = NPC_Base.NPCState.Idle;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float duration = stateInfo.length;
        print("BEKLE");
        yield return new WaitForSeconds(duration);
        print("ÇALIŞ");


   }


}
