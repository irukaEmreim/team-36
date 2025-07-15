using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class NPC_ActionCycle : MonoBehaviour
{
    private NPC_Base npc;
    private NPC_Movement movement;
    private NPC_AnimationHandler anim;
    private Animator animator;
    private bool isCycling = false;

    public float moveDistance = 10f; // NPC'nin hareket edebileceği mesafe

    void Awake()
    {
        npc = GetComponent<NPC_Base>();
        movement = GetComponent<NPC_Movement>();
        anim = GetComponent<NPC_AnimationHandler>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        StartCoroutine(ActionLoop());
    }

    IEnumerator ActionLoop()
    {
        isCycling = true;
        while (npc.currentState != NPC_Base.NPCState.Running)
        {
            if (npc.currentState == NPC_Base.NPCState.Reacting)
            {
                yield return null; // bekle kaçş bitene kadar
                continue;
            }

            // Rastgele Nokta
            Vector3 target = movement.GetRandomPoint(10f);
            movement.MoveTo(target);
            anim.PlayWalk();

            yield return new WaitUntil(() => movement.HasReachedDestination());
            movement.Stop();

            // Rastgele Idle
            string idleAnim = anim.GetRandomIdle();
            anim.PlayOneShot(idleAnim);
            yield return null;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float duration = stateInfo.length;
            yield return new WaitForSeconds(duration);

            int danceChance = Random.Range(0, 100);
            if (danceChance < 20) // %20 dans yapma şansı
            {
                string danceAnim = anim.GetRandomDance();
                anim.PlayOneShot(danceAnim);
                yield return null;
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                duration = stateInfo.length;
                yield return new WaitForSeconds(duration); // Dans animasyonunun bitmesini bekle
            }


        }
    }

    public void StopCycle() => isCycling = false;


}