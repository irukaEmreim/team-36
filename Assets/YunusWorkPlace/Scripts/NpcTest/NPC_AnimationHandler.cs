using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_AnimationHandler : MonoBehaviour
{

    private Animator animator;

    public string[] idleAnimations;
    public string[] danceAnimations;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayRun() => SetBool("isRunning");
    public void PlayWalk() => SetBool("isWalking");
    public void PlayYell() => SetBool("isYelling");

    public void PlayIdle() => ClearAll();
    public void StopAll() => ClearAll();

    private void SetBool(string name)
    {
        ClearAll();
        animator.SetBool(name, true);
    }

    public void ClearAll()
    {
        animator.SetBool("isRunning", false);
        animator.SetBool("isYelling", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isReacting", false);
        animator.SetBool("isFleeing", false);
        animator.SetBool("isSitting", false);
    }

    public void PlayOneShot(string triggerName)
    {
        ClearAll();
        animator.Play(triggerName);
    }

    public string GetRandomIdle()
    {
        return idleAnimations[Random.Range(0, idleAnimations.Length)];
    }

    public string GetRandomDance()
    {
        return danceAnimations[Random.Range(0, danceAnimations.Length)];
    }


}
